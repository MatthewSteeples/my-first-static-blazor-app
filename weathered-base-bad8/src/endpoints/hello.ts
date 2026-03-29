import { OpenAPIRoute } from "chanfana";
import { z } from "zod";
import { type AppContext } from "../types";

export class Hello extends OpenAPIRoute {
	schema = {
		tags: ["Hello"],
		summary: "Hello world",
		responses: {
			"200": {
				description: "Returns a hello world greeting",
				content: {
					"text/plain": {
						schema: z.string(),
					},
				},
			},
		},
	};

	async handle(c: AppContext) {
		return c.text("hello world");
	}
}
