using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace TASVideos.Api.Controllers;

internal static class SystemsApiMapper
{
	public static void Map(WebApplication app)
	{
		app.MapGet("api/v1/systems/{id}", async (int id, IGameSystemService systemService) =>
		{
			var system = (await systemService.GetAll())
				.SingleOrDefault(p => p.Id == id);

			return system is null
				? Results.NotFound()
				: Results.Ok(system);
		})
		.WithTags("Systems")
		.WithSummary("Returns a game systems with the given id.")
		.Produces<SystemsResponse>()
		.WithOpenApi(g =>
		{
			g.Responses.AddGeneric400();
			g.Responses.Add404ById("system");
			return g;
		});

		app.MapGet("api/v1/systems", async (IGameSystemService systemService)
				=> Results.Ok((object?)await systemService.GetAll()))
		.WithTags("Systems")
		.WithSummary("Returns a list of available game sytems, including supported framerates.")
		.Produces<IEnumerable<SystemsResponse>>();
	}
}
