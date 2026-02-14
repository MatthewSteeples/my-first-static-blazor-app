import { OpenAPIRoute } from "chanfana";
import { z } from "zod";
import { type AppContext } from "../types";
import {
	getBearerToken,
	extractJwk,
	verifyEs256JwtSignature,
	validateJwtClaimsAndKeyBinding,
} from "../auth/jwt";

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

		const jwk = extractJwk(token, c, parsedBody.data?.publicKeyJwk);
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
