import { OpenAPIRoute } from "chanfana";
import { type AppContext } from "../types";
import { z } from "zod";

export class Hello extends OpenAPIRoute {
	schema = {
		tags: ["Hello"],
		summary: "Hello world",
		responses: {
			"200": {
				description: "Returns a hello world greeting",
				content: {
					"text/plain": {
						schema: z.string().openapi({ example: "hello world" }),
					},
				},
			},
		},
	};

	async handle(c: AppContext) {
		return c.text("hello world");
	}
}
