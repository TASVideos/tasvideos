namespace TASVideos.Api;

internal static class ClassesEndpoints
{
	public static WebApplication MapClasses(this WebApplication app)
	{
		var group = app.MapApiGroup("Classes");

		group
			.MapGet("{id:int}", async (int id, IClassService classService) => ApiResults.OkOr404(await classService.GetById(id)))
			.ProducesFromId<PublicationClass>("publication class");

		group
			.MapGet("", async (IClassService classService) => Results.Ok(await classService.GetAll()))
			.ProducesList<PublicationClass>("a list of available publication classes");

		return app;
	}
}
