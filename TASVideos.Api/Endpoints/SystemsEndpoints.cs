using Microsoft.AspNetCore.Builder;

namespace TASVideos.Api;

internal static class SystemsEndpoints
{
	public static WebApplication MapSystems(this WebApplication app)
	{
		var group = app.MapGroup("api/v1/systems").WithTags("Systems");

		group.MapGet("{id}", async (int id, IGameSystemService systemService) =>
			{
				var system = (await systemService.GetAll())
					.SingleOrDefault(p => p.Id == id);

				return system is null
					? Results.NotFound()
					: Results.Ok(system);
			})
			.WithSummary("Returns a game system with the given id.")
			.Produces<SystemsResponse>()
			.WithOpenApi(g =>
			{
				g.Responses.AddGeneric400();
				g.Responses.Add404ById("system");
				return g;
			});

		group.MapGet("", async (IGameSystemService systemService) => Results.Ok((object?)await systemService.GetAll()))
			.WithSummary("Returns a list of available game sytems, including supported framerates.")
			.Produces<IEnumerable<SystemsResponse>>()
			.WithOpenApi();

		return app;
	}
}
