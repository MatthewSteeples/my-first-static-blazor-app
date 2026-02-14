import { DurableObject } from "cloudflare:workers";
import { drizzle, type DrizzleSqliteDODatabase } from "drizzle-orm/durable-sqlite";
import { migrate } from "drizzle-orm/durable-sqlite/migrator";
import { max } from "drizzle-orm";
import journal from "../../drizzle/migrations/meta/_journal.json";
import m0000 from "../../drizzle/migrations/0000_lush_skreet.sql";
import { syncEvents } from "../db/schema";

const ONE_WEEK_MS = 7 * 24 * 60 * 60 * 1000;
const THIRTY_DAYS_MS = 30 * 24 * 60 * 60 * 1000;

export class SyncDurableObject extends DurableObject<Env> {
	private db: DrizzleSqliteDODatabase;

	constructor(ctx: DurableObjectState, env: Env) {
		super(ctx, env);
		this.db = drizzle(ctx.storage);

		ctx.blockConcurrencyWhile(async () => {
			await migrate(this.db, {
				journal,
				migrations: {
					m0000: m0000,
				},
			});
			await this.ensureAlarm();
		});
	}

	private async ensureAlarm() {
		const existing = await this.ctx.storage.getAlarm();
		if (!existing) {
			await this.ctx.storage.setAlarm(Date.now() + ONE_WEEK_MS);
		}
	}

	async alarm() {
		const result = this.db
			.select({ latest: max(syncEvents.receivedAt) })
			.from(syncEvents)
			.get();

		const latestReceived = result?.latest;

		if (!latestReceived || Date.now() - new Date(latestReceived).getTime() > THIRTY_DAYS_MS) {
			await this.ctx.storage.deleteAll();
			return;
		}

		// Schedule next check
		await this.ctx.storage.setAlarm(Date.now() + ONE_WEEK_MS);
	}

	async fetch(request: Request): Promise<Response> {
		if (request.method !== "POST") {
			return Response.json({ error: "Method not allowed" }, { status: 405 });
		}

		try {
			const body = await request.json() as {
				eventId: string;
				eventType: string;
				itemId: string;
				timestamp: string;
				payload?: string | null;
			};

			const result = await this.db
				.insert(syncEvents)
				.values({
					eventId: body.eventId,
					eventType: body.eventType,
					itemId: body.itemId,
					timestamp: body.timestamp,
					payload: body.payload ?? null,
					receivedAt: new Date().toISOString(),
				})
				.onConflictDoNothing({ target: syncEvents.eventId });

			// Check if the row was actually inserted
			const inserted = (result as any)?.changes !== 0;

			return Response.json({
				stored: inserted,
				eventId: body.eventId,
			});
		} catch (error) {
			return Response.json(
				{ error: (error as Error)?.message ?? "Internal error" },
				{ status: 500 }
			);
		}
	}
}
