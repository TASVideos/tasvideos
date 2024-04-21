using Microsoft.AspNetCore.Builder;

namespace TASVideos.Api;

internal static class SystemsEndpoints
{
	public static WebApplication MapSystems(this WebApplication app)
	{
		var group = app.MapApiGroup("Systems");

		group.MapGet("{id}", async (int id, IGameSystemService systemService)
				=> ApiResults.OkOr404((await systemService.GetAll()).SingleOrDefault(p => p.Id == id)))
		.DocumentIdGet("system", typeof(SystemsResponse));

		group.MapGet("", async (IGameSystemService systemService) => Results.Ok((object?)await systemService.GetAll()))
			.WithSummary("Returns a list of available game sytems, including supported framerates.")
			.Produces<IEnumerable<SystemsResponse>>()
			.WithOpenApi();

		return app;
	}
}
