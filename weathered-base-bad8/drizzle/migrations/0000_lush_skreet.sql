CREATE TABLE sync_events (
	event_id text PRIMARY KEY NOT NULL,
	event_type text NOT NULL,
	item_id text NOT NULL,
	timestamp text NOT NULL,
	payload text,
	received_at text NOT NULL
);
