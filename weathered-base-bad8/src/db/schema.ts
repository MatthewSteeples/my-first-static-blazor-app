import { sqliteTable, text } from "drizzle-orm/sqlite-core";

export const syncEvents = sqliteTable("sync_events", {
	eventId: text("event_id").primaryKey(),
	eventType: text("event_type").notNull(),
	itemId: text("item_id").notNull(),
	timestamp: text("timestamp").notNull(),
	payload: text("payload"),
	receivedAt: text("received_at").notNull(),
});
