import { OpenAPIRoute } from "chanfana";
import { z } from "zod";
import { type AppContext } from "../types";
import {
	getBearerToken,
	extractJwk,
	verifyEs256JwtSignature,
	validateJwtClaimsAndKeyBinding,
	getJwtSubject,
} from "../auth/jwt";

const SyncEventBodySchema = z.object({
	eventId: z.string().uuid(),
	eventType: z.enum(["Created", "Updated", "Deleted"]),
	itemId: z.string().uuid(),
	timestamp: z.string(),
	payload: z.string().nullable().optional(),
});

function jsonResponse(c: AppContext, status: number, body: unknown) {
	return c.json(body as any, status as any);
}

export class SyncEvent extends OpenAPIRoute {
	schema = {
		tags: ["Sync"],
		summary: "Push a single sync event to the Durable Object store",
		request: {
			body: {
				content: {
					"application/json": {
						schema: SyncEventBodySchema,
					},
				},
				required: true,
			},
		},
		responses: {
			"200": {
				description: "Event stored successfully",
				content: {
					"application/json": {
						schema: z.object({
							stored: z.boolean(),
							eventId: z.string(),
						}),
					},
				},
			},
			"401": {
				description: "JWT missing or invalid",
				content: {
					"application/json": {
						schema: z.object({ error: z.string() }),
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
		// Authenticate
		const token = getBearerToken(c);
		if (!token) {
			return jsonResponse(c, 401, { error: "Missing JWT" });
		}

		const jwk = extractJwk(token, c);
		if (!jwk) {
			return jsonResponse(c, 401, { error: "Missing public key in JWT header" });
		}

		try {
			const valid = await verifyEs256JwtSignature(token, jwk);
			if (!valid) {
				return jsonResponse(c, 401, { error: "Invalid JWT signature" });
			}
			await validateJwtClaimsAndKeyBinding(token, jwk);
		} catch (error) {
			return jsonResponse(c, 401, { error: (error as Error)?.message ?? "Invalid token" });
		}

		// Parse body
		let body: unknown;
		try {
			body = await c.req.json().catch(() => undefined);
		} catch {
			body = undefined;
		}

		const parsed = SyncEventBodySchema.safeParse(body);
		if (!parsed.success) {
			return jsonResponse(c, 400, { error: "Invalid sync event body" });
		}

		// Route to Durable Object
		const thumbprint = getJwtSubject(token);
		const doId = c.env.SYNC_DO.idFromName(thumbprint);
		const stub = c.env.SYNC_DO.get(doId);

		const doResponse = await stub.fetch(new Request("https://do/event", {
			method: "POST",
			headers: { "Content-Type": "application/json" },
			body: JSON.stringify(parsed.data),
		}));

		const result = await doResponse.json() as any;
		return jsonResponse(c, doResponse.status, result);
	}
}
