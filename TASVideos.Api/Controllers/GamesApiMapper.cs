using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using TASVideos.Data.Entity.Game;

namespace TASVideos.Api.Controllers;

internal static class GamesApiMapper
{
	public static void Map(WebApplication app)
	{
		app.MapGet("api/v1/games/{id}", async (int id, ApplicationDbContext db) =>
		{
			var pub = await db.Games
				.ToGamesResponse()
				.SingleOrDefaultAsync(p => p.Id == id);
			return pub is null
				? Results.NotFound()
				: Results.Ok(pub);
		})
		.WithTags("Games")
		.WithSummary("Returns a game with the given id.")
		.Produces<GamesResponse>()
		.WithOpenApi(g =>
		{
			g.Responses.AddGeneric400();
			g.Responses.Add404ById("game");
			return g;
		});

		app.MapGet("api/v1/games", async (GamesRequest request, IValidator<GamesRequest> validator, ApplicationDbContext db) =>
		{
			var validationResult = validator.Validate(request);
			if (!validationResult.IsValid)
			{
				return Results.ValidationProblem(validationResult.ToDictionary());
			}

			var games = (await db.Games.ForSystemCodes(request.SystemCodes)
					.ToGamesResponse()
					.SortBy(request)
					.Paginate(request)
					.ToListAsync())
				.FieldSelect(request);

			return Results.Ok(games);
		})
		.WithTags("Games")
		.WithSummary("Returns a list of available games.")
		.Produces<IEnumerable<GamesResponse>>()
		.WithOpenApi(g =>
		{
			g.Parameters.AddFromQuery("systems", "The system codes to filter by", typeof(string));
			g.Parameters.AddBaseQueryParams();
			g.Responses.AddGeneric400();
			return g;
		});
	}
}
