namespace TASVideos.Api;

internal static class SystemsEndpoints
{
	public static WebApplication MapSystems(this WebApplication app)
	{
		var group = app.MapApiGroup("Systems");

		group
			.MapGet("{id:int}", async (int id, IGameSystemService systemService) => ApiResults.OkOr404((await systemService.GetAll()).SingleOrDefault(p => p.Id == id)))
			.ProducesFromId<SystemsResponse>("system");

		group
			.MapGet("", async (IGameSystemService systemService) => Results.Ok(await systemService.GetAll()))
			.ProducesList<SystemsResponse>("a list of available game sytems, including supported framerates.");

		return app;
	}
}
