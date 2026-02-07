import { DurableObject } from "cloudflare:workers";

export interface PushSubscriptionData {
	endpoint: string;
	keys: {
		p256dh: string;
		auth: string;
	};
}

export interface AlarmData {
	trackedItemId: string;
	trackedItemName: string;
	nextOccurrenceTime: number; // Unix timestamp in milliseconds
	subscription?: PushSubscriptionData;
}

export class NotificationAlarm extends DurableObject {
	private alarmData: AlarmData | null = null;

	constructor(ctx: DurableObjectState, env: Env) {
		super(ctx, env);
	}

	async fetch(request: Request): Promise<Response> {
		const url = new URL(request.url);

		if (request.method === "POST" && url.pathname === "/schedule") {
			return this.scheduleAlarm(request);
		}

		if (request.method === "GET" && url.pathname === "/status") {
			return this.getStatus();
		}

		return new Response("Not found", { status: 404 });
	}

	async scheduleAlarm(request: Request): Promise<Response> {
		try {
			const data: AlarmData = await request.json();

			if (!data.trackedItemId || !data.nextOccurrenceTime) {
				return new Response(
					JSON.stringify({ error: "Missing required fields" }),
					{ status: 400, headers: { "Content-Type": "application/json" } }
				);
			}

			// Store alarm data
			this.alarmData = data;
			await this.ctx.storage.put("alarmData", data);

			// Calculate delay until alarm should fire
			const now = Date.now();
			const delayMs = data.nextOccurrenceTime - now;

			if (delayMs > 0) {
				// Schedule the alarm
				await this.ctx.storage.setAlarm(now + delayMs);

				return new Response(
					JSON.stringify({
						success: true,
						scheduledFor: new Date(data.nextOccurrenceTime).toISOString(),
						delayMs,
					}),
					{ status: 200, headers: { "Content-Type": "application/json" } }
				);
			} else {
				// Time has already passed, trigger immediately
				await this.alarm();
				return new Response(
					JSON.stringify({
						success: true,
						message: "Alarm triggered immediately (time already passed)",
					}),
					{ status: 200, headers: { "Content-Type": "application/json" } }
				);
			}
		} catch (error) {
			return new Response(
				JSON.stringify({ error: (error as Error).message }),
				{ status: 500, headers: { "Content-Type": "application/json" } }
			);
		}
	}

	async getStatus(): Promise<Response> {
		const alarmData = await this.ctx.storage.get<AlarmData>("alarmData");
		const currentAlarm = await this.ctx.storage.getAlarm();

		return new Response(
			JSON.stringify({
				alarmData,
				scheduledAlarmTime: currentAlarm
					? new Date(currentAlarm).toISOString()
					: null,
			}),
			{ status: 200, headers: { "Content-Type": "application/json" } }
		);
	}

	async alarm(): Promise<void> {
		// Retrieve the alarm data
		const data = await this.ctx.storage.get<AlarmData>("alarmData");

		if (!data) {
			console.warn("Alarm triggered but no data found");
			return;
		}

		console.log(
			`Alarm triggered for tracked item: ${data.trackedItemName} (${data.trackedItemId})`
		);

		// If subscription data is available, send push notification
		if (data.subscription) {
			await this.sendPushNotification(data);
		}

		// Clean up - alarm has fired
		await this.ctx.storage.delete("alarmData");
		this.alarmData = null;
	}

	private async sendPushNotification(data: AlarmData): Promise<void> {
		// TODO: Implement Web Push protocol when VAPID keys are configured
		// For now, just log the notification
		// Track implementation: https://github.com/MatthewSteeples/my-first-static-blazor-app/issues/TBD
		console.log(`Would send push notification for: ${data.trackedItemName}`);

		// In production, you would:
		// 1. Generate VAPID keys and store them securely
		// 2. Use web-push library or implement the Web Push protocol
		// 3. Send the notification to data.subscription endpoint
		// Example notification payload:
		const payload = {
			title: "Medication Reminder",
			body: `Time to take ${data.trackedItemName}`,
			icon: "/icon-192.png",
			badge: "/badge-72.png",
			data: {
				trackedItemId: data.trackedItemId,
				url: "/",
			},
		};

		console.log("Notification payload:", JSON.stringify(payload, null, 2));
	}
}
