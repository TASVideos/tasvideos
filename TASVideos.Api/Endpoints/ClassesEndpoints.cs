﻿using Microsoft.AspNetCore.Builder;

namespace TASVideos.Api;

internal static class ClassesEndpoints
{
	public static WebApplication MapClasses(this WebApplication app)
	{
		var group = app.MapApiGroup("Classes");

		group
			.MapGet("{id}", async (int id, IClassService classService) => ApiResults.OkOr404(await classService.GetById(id)))
			.DocumentIdGet("publication class", typeof(PublicationClass));

		group
			.MapGet("", async (IClassService classService) => Results.Ok(await classService.GetAll()))
			.WithSummary("Returns a list of available publication classes.")
			.Produces<IEnumerable<PublicationClass>>()
			.WithOpenApi();

		return app;
	}
}