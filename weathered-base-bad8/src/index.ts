import { fromHono } from "chanfana";
import { Hono } from "hono";
import { Hello } from "./endpoints/hello";
import { Sync } from "./endpoints/sync";
import { SyncEvent } from "./endpoints/syncEvent";
import { TaskCreate } from "./endpoints/taskCreate";
import { TaskDelete } from "./endpoints/taskDelete";
import { TaskFetch } from "./endpoints/taskFetch";
import { TaskList } from "./endpoints/taskList";

// Re-export the Durable Object so the runtime can instantiate it
export { SyncDurableObject } from "./durable-objects/SyncDurableObject";

// Start a Hono app
const app = new Hono<{ Bindings: Env }>();

// Setup OpenAPI registry
const openapi = fromHono(app, {
	docs_url: "/",
});

// Register OpenAPI endpoints
openapi.get("/api/tasks", TaskList);
openapi.post("/api/tasks", TaskCreate);
openapi.get("/api/tasks/:taskSlug", TaskFetch);
openapi.delete("/api/tasks/:taskSlug", TaskDelete);
openapi.get("/api/hello", Hello);
openapi.get("/api/sync", Sync);
openapi.post("/api/sync", Sync);
openapi.post("/api/sync/event", SyncEvent);

// Export the Hono app
export default app;
