import { OpenAPIRoute } from "chanfana";
import { z } from "zod";
import { type AppContext } from "../types";

const ScheduleNotificationRequestSchema = z.object({
	trackedItemId: z.string(),
	trackedItemName: z.string(),
	nextOccurrenceTime: z.number(), // Unix timestamp in milliseconds
	subscription: z.any().optional(), // Push subscription data
});

export class ScheduleNotification extends OpenAPIRoute {
	schema = {
		tags: ["Notifications"],
		summary: "Schedule a push notification alarm for a tracked item",
		request: {
			body: {
				content: {
					"application/json": {
						schema: ScheduleNotificationRequestSchema,
					},
				},
			},
		},
		responses: {
			"200": {
				description: "Notification scheduled successfully",
				content: {
					"application/json": {
						schema: z.object({
							success: z.boolean(),
							scheduledFor: z.string().optional(),
							delayMs: z.number().optional(),
							message: z.string().optional(),
						}),
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
			"500": {
				description: "Server error",
				content: {
					"application/json": {
						schema: z.object({ error: z.string() }),
					},
				},
			},
		},
	};

	async handle(c: AppContext) {
		const body = await c.req.json();
		const result = ScheduleNotificationRequestSchema.safeParse(body);
		
		if (!result.success) {
			return c.json({ error: "Invalid request body" }, 400);
		}
		
		const data = result.data;

		// Get the Durable Object namespace
		const notificationNamespace = c.env.NOTIFICATION_ALARM;
		if (!notificationNamespace) {
			return c.json({ error: "Notification service not configured" }, 500);
		}

		// Use the tracked item ID as the Durable Object ID
		// This ensures one alarm per tracked item
		const id = notificationNamespace.idFromName(data.trackedItemId);
		const stub = notificationNamespace.get(id);

		// Forward the request to the Durable Object
		const response = await stub.fetch(new Request("http://notification/schedule", {
			method: "POST",
			headers: { "Content-Type": "application/json" },
			body: JSON.stringify(data),
		}));

		const responseData = await response.json();
		return c.json(responseData, response.status as any);
	}
}
