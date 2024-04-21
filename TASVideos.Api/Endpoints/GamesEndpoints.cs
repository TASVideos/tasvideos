using Microsoft.AspNetCore.Builder;
using TASVideos.Data.Entity.Game;

namespace TASVideos.Api;

internal static class GamesEndpoints
{
	public static WebApplication MapGames(this WebApplication app)
	{
		var group = app.MapApiGroup("Games");

		group
			.MapGet("{id}", async (int id, ApplicationDbContext db) => ApiResults.OkOr404(
				await db.Games
					.ToGamesResponse()
					.SingleOrDefaultAsync(g => g.Id == id)))
			.DocumentIdGet("game", typeof(GamesResponse));

		group.MapGet("", async ([AsParameters]GamesRequest request, HttpContext context, ApplicationDbContext db) =>
		{
			var validationError = ApiResults.Validate(request, context);
			if (validationError is not null)
			{
				return validationError;
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
			g.Parameters.Describe<GamesRequest>();
			g.Responses.AddGeneric400();
			return g;
		});

		return app;
	}
}
