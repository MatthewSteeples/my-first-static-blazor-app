import { OpenAPIRoute } from "chanfana";
import { z } from "zod";
import { type AppContext } from "../types";

const SyncRequestSchema = z
	.object({
		// If you don't pass an Authorization header, you may provide the token in the body.
		token: z.string().optional(),
		// Optional: public key JWK as an object or JSON string.
		// If omitted, the handler will attempt to read `jwk` from the JWT header.
		publicKeyJwk: z.union([z.record(z.string(), z.any()), z.string()]).optional(),
	})
	.optional();

function jsonResponse(c: AppContext, status: number, body: unknown) {
	return c.json(body as any, status as any);
}

function tryParseJson(value: string): unknown {
	try {
		return JSON.parse(value);
	} catch {
		return undefined;
	}
}

function asJsonWebKey(value: unknown): JsonWebKey | undefined {
	if (!value || typeof value !== "object") return undefined;
	return value as JsonWebKey;
}

function getBearerToken(c: AppContext): string | undefined {
	const auth = c.req.header("authorization") ?? c.req.header("Authorization");
	if (!auth) return undefined;

	const match = auth.match(/^Bearer\s+(.+)$/i);
	return match?.[1];
}

function base64UrlToUint8Array(input: string): Uint8Array {
	const b64 = input.replace(/-/g, "+").replace(/_/g, "/");
	const padLength = (4 - (b64.length % 4)) % 4;
	const padded = b64 + "=".repeat(padLength);
	const binary = atob(padded);
	const bytes = new Uint8Array(binary.length);
	for (let i = 0; i < binary.length; i++) bytes[i] = binary.charCodeAt(i);
	return bytes;
}

function uint8ArrayToBase64Url(bytes: Uint8Array): string {
	let binary = "";
	for (let i = 0; i < bytes.length; i++) binary += String.fromCharCode(bytes[i]);
	const b64 = btoa(binary);
	return b64.replace(/\+/g, "-").replace(/\//g, "_").replace(/=+$/g, "");
}

function decodeJwtJsonPart(encoded: string, partName: string): any {
	const bytes = base64UrlToUint8Array(encoded);
	const jsonText = new TextDecoder().decode(bytes);
	const parsed = tryParseJson(jsonText);
	if (!parsed || typeof parsed !== "object") {
		throw new Error(`Invalid JWT ${partName}`);
	}
	return parsed;
}

async function getEs256PublicJwkThumbprint(jwk: JsonWebKey): Promise<string> {
	// RFC 7638 thumbprint, specialized for ES256 EC public keys.
	if (jwk.kty !== "EC" || jwk.crv !== "P-256" || !jwk.x || !jwk.y) {
		throw new Error("Unsupported public key JWK (expected EC P-256 with x/y)");
	}

	// Canonical JSON members in lexicographic order: crv, kty, x, y
	const canonical = {
		crv: jwk.crv,
		kty: jwk.kty,
		x: jwk.x,
		y: jwk.y,
	};

	const json = JSON.stringify(canonical);
	const bytes = new TextEncoder().encode(json);
	const digest = new Uint8Array(await crypto.subtle.digest("SHA-256", bytes));
	return uint8ArrayToBase64Url(digest);
}

function bytesToBigInt(bytes: Uint8Array): bigint {
	let value = 0n;
	for (const b of bytes) value = (value << 8n) | BigInt(b);
	return value;
}

function bigIntToMinimalUnsignedBytes(value: bigint): Uint8Array {
	if (value === 0n) return new Uint8Array([0]);
	const bytes: number[] = [];
	let v = value;
	while (v > 0n) {
		bytes.push(Number(v & 0xffn));
		v >>= 8n;
	}
	bytes.reverse();

	// Ensure it's interpreted as positive in ASN.1 INTEGER.
	if (bytes[0] & 0x80) {
		bytes.unshift(0);
	}

	return new Uint8Array(bytes);
}

function rawEcdsaSigToDer(rawSig: Uint8Array): Uint8Array {
	// JWS ES256 signatures are raw R||S, each 32 bytes.
	// Some non-standard implementations may already send DER; accept it to be resilient.
	if (rawSig.length !== 64) {
		if (rawSig.length > 0 && rawSig[0] === 0x30) {
			return rawSig;
		}
		throw new Error(`Invalid ES256 signature length: ${rawSig.length}`);
	}

	const r = rawSig.slice(0, 32);
	const s = rawSig.slice(32, 64);
	const rInt = bigIntToMinimalUnsignedBytes(bytesToBigInt(r));
	const sInt = bigIntToMinimalUnsignedBytes(bytesToBigInt(s));

	// ASN.1 DER: SEQUENCE(INTEGER(r), INTEGER(s))
	const rPart = new Uint8Array([0x02, rInt.length, ...rInt]);
	const sPart = new Uint8Array([0x02, sInt.length, ...sInt]);
	const seqLen = rPart.length + sPart.length;
	if (seqLen > 0xff) {
		throw new Error("DER sequence too long");
	}
	return new Uint8Array([0x30, seqLen, ...rPart, ...sPart]);
}

async function verifyEs256JwtSignature(token: string, jwk: JsonWebKey): Promise<boolean> {
	const parts = token.split(".");
	if (parts.length !== 3) {
		throw new Error("Invalid JWT format");
	}

	const [encodedHeader, encodedPayload, encodedSignature] = parts;
	const headerJson = decodeJwtJsonPart(encodedHeader, "header");

	const alg = (headerJson as any).alg;
	if (alg !== "ES256") {
		throw new Error(`Unsupported JWT alg: ${String(alg)}`);
	}

	const signingInput = new TextEncoder().encode(`${encodedHeader}.${encodedPayload}`);
	const rawSig = base64UrlToUint8Array(encodedSignature);
    
	const key = await crypto.subtle.importKey(
		"jwk",
		jwk,
		{ name: "ECDSA", namedCurve: "P-256" },
		false,
		["verify"]
	);

	// WebCrypto (including Node's and Cloudflare Workers') expects the ECDSA signature in "raw" form (R||S).
	// Some environments/libraries may use DER; for resilience try raw first, then DER.
	const okRaw = await crypto.subtle.verify({ name: "ECDSA", hash: "SHA-256" }, key, rawSig, signingInput);
	if (okRaw)
        return true;
    
    const derSig = rawEcdsaSigToDer(rawSig);
	return crypto.subtle.verify({ name: "ECDSA", hash: "SHA-256" }, key, derSig, signingInput);
}

async function validateJwtClaimsAndKeyBinding(token: string, jwk: JsonWebKey): Promise<void> {
	const parts = token.split(".");
	if (parts.length !== 3) throw new Error("Invalid JWT format");
	const [encodedHeader, encodedPayload] = parts;

	const headerJson = decodeJwtJsonPart(encodedHeader, "header");
	const payloadJson = decodeJwtJsonPart(encodedPayload, "payload");

	const thumbprint = await getEs256PublicJwkThumbprint(jwk);
	const kid = (headerJson as any).kid;
	const sub = (payloadJson as any).sub;
	if (typeof kid !== "string" || kid.length === 0) {
		throw new Error("Missing JWT kid");
	}
	if (typeof sub !== "string" || sub.length === 0) {
		throw new Error("Missing JWT sub");
	}
	if (kid !== thumbprint) {
		throw new Error("JWT kid does not match public key");
	}
	if (sub !== thumbprint) {
		throw new Error("JWT sub does not match public key");
	}

	const iat = (payloadJson as any).iat;
	const exp = (payloadJson as any).exp;
	if (typeof iat !== "number" || !Number.isFinite(iat)) {
		throw new Error("Missing/invalid JWT iat");
	}
	if (typeof exp !== "number" || !Number.isFinite(exp)) {
		throw new Error("Missing/invalid JWT exp");
	}

	const nowSeconds = Math.floor(Date.now() / 1000);
	const clockSkewSeconds = 5 * 60; // allow small skew
	if (iat > nowSeconds + clockSkewSeconds) {
		throw new Error("JWT iat is in the future");
	}
	if (nowSeconds - clockSkewSeconds > exp) {
		throw new Error("JWT is expired");
	}

	const maxLifetimeSeconds = 30 * 24 * 60 * 60;
	if (exp - iat > maxLifetimeSeconds) {
		throw new Error("JWT lifetime exceeds 30 days");
	}
}

export class Sync extends OpenAPIRoute {
	schema = {
		tags: ["Sync"],
		summary: "Background sync endpoint (signature validation only)",
		request: {
			body: {
				content: {
					"application/json": {
						schema: SyncRequestSchema,
					},
				},
				required: false,
			},
		},
		responses: {
			"200": {
				description: "JWT signature is valid",
				content: {
					"application/json": {
						schema: z.object({ valid: z.literal(true) }),
					},
				},
			},
			"401": {
				description: "JWT missing/invalid signature",
				content: {
					"application/json": {
						schema: z.object({ valid: z.literal(false), error: z.string().optional() }),
					},
				},
			},
			"400": {
				description: "Bad request",
				content: {
					"application/json": {
						schema: z.object({ error: z.string() }),
					},
				},
			},
		},
	};

	async handle(c: AppContext) {
		let body: unknown;
		try {
			// Avoid throwing for empty bodies
			body = await c.req.json().catch(() => undefined);
		} catch {
			body = undefined;
		}

		const parsedBody = SyncRequestSchema.safeParse(body);
		if (!parsedBody.success) {
			return jsonResponse(c, 400, { error: "Invalid request body" });
		}

		const token = getBearerToken(c) ?? parsedBody.data?.token;
		if (!token) {
			return jsonResponse(c, 401, { valid: false, error: "Missing JWT" });
		}

		// Preferred: JWT header contains `jwk`.
		let jwk: JsonWebKey | undefined;
		try {
			const [encodedHeader] = token.split(".");
			const headerJson = tryParseJson(new TextDecoder().decode(base64UrlToUint8Array(encodedHeader)));
			jwk = asJsonWebKey((headerJson as any)?.jwk);
		} catch {
			// ignore
		}

		// Fallback: allow the client to send the public key JWK separately.
		if (!jwk) {
			const headerJwk = c.req.header("x-public-key-jwk") ?? c.req.header("X-Public-Key-Jwk");
			if (headerJwk) {
				jwk = asJsonWebKey(typeof headerJwk === "string" ? tryParseJson(headerJwk) : undefined);
			}
		}

		if (!jwk && parsedBody.data?.publicKeyJwk) {
			jwk = asJsonWebKey(
				typeof parsedBody.data.publicKeyJwk === "string"
					? tryParseJson(parsedBody.data.publicKeyJwk)
					: parsedBody.data.publicKeyJwk
			);
		}

		if (!jwk || typeof jwk !== "object") {
			return jsonResponse(c, 400, {
				error:
					"Missing public key JWK (provide JWT header `jwk`, `x-public-key-jwk`, or `publicKeyJwk` in body)",
			});
		}

		try {
			const valid = await verifyEs256JwtSignature(token, jwk);
			if (!valid) {
				return jsonResponse(c, 401, { valid: false });
			}
			await validateJwtClaimsAndKeyBinding(token, jwk);
			return jsonResponse(c, 200, { valid: true });
		} catch (error) {
			return jsonResponse(c, 401, { valid: false, error: (error as Error)?.message ?? "Invalid token" });
		}
	}
}
