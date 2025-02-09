using TASVideos.Data.Entity.Game;

namespace TASVideos.Api;

internal static class GamesEndpoints
{
	public static WebApplication MapGames(this WebApplication app)
	{
		var group = app.MapApiGroup("Games");

		group
			.MapGet("{id:int}", async (int id, ApplicationDbContext db) => ApiResults.OkOr404(
				await db.Games
					.ToGamesResponse()
					.SingleOrDefaultAsync(g => g.Id == id)))
			.ProducesFromId<GamesResponse>("game");

		group.MapGet("", async ([AsParameters] GamesRequest request, HttpContext context, ApplicationDbContext db) =>
		{
			var validationError = ApiResults.Validate(request, context);
			if (validationError is not null)
			{
				return validationError;
			}

			var games = (await db.Games.ForSystemCodes(request.SystemCodes.ToList())
					.ToGamesResponse()
					.SortAndPaginate(request)
					.ToListAsync())
				.FieldSelect(request);

			return Results.Ok(games);
		})
		.Receives<GamesRequest>()
		.ProducesList<GamesResponse>("a list of games");

		return app;
	}
}
