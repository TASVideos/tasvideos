using Microsoft.AspNetCore.Builder;
using TASVideos.Data.Entity.Game;

namespace TASVideos.Api;

internal static class GamesEndpoints
{
	public static WebApplication MapGames(this WebApplication app)
	{
		var group = app.MapApiGroup("Games");

		group.MapGet("{id}", async (int id, ApplicationDbContext db) =>
		{
			var pub = await db.Games
				.ToGamesResponse()
				.SingleOrDefaultAsync(p => p.Id == id);
			return pub is null
				? Results.NotFound()
				: Results.Ok(pub);
		})
		.DocumentIdGet("Returns a game with the given id.", "game", typeof(GamesResponse));

		group.MapGet("", async ([AsParameters]GamesRequest request, IValidator<GamesRequest> validator, ApplicationDbContext db) =>
		{
			var validationResult = validator.Validate(request);
			if (!validationResult.IsValid)
			{
				return ApiResults.ValidationError(validationResult);
			}

			var games = (await db.Games.ForSystemCodes(request.SystemCodes)
					.ToGamesResponse()
					.SortBy(request)
					.Paginate(request)
					.ToListAsync())
				.FieldSelect(request);

			return Results.Ok(games);
		})
		.WithSummary("Returns a list of available games.")
		.Produces<IEnumerable<GamesResponse>>()
		.WithOpenApi(g =>
		{
			g.Parameters.AddStringFromQuery("systems", "The system codes to filter by");
			g.Parameters.AddBaseQueryParams();
			g.Responses.AddGeneric400();
			return g;
		});

		return app;
	}
}
