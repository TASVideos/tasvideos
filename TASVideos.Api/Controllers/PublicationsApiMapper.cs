using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace TASVideos.Api.Controllers;

// TODO: old swagger UI did client side validation of int, not letting non-ints be typed in, how is this done?
// JWT authentication
internal static class PublicationsApiMapper
{
	public static void Map(WebApplication app)
	{
		app.MapGet("api/v1/publications/{id}", async (int id, ApplicationDbContext db) =>
		{
			var pub = await db.Publications
				.ToPublicationsResponse()
				.SingleOrDefaultAsync(p => p.Id == id);

			return pub is null
				? Results.NotFound()
				: Results.Ok(pub);
		})
		.WithTags("Publications")
		.WithSummary("Returns a publication with the given id.")
		.Produces<PublicationsResponse>()
		.WithOpenApi(g =>
		{
			g.Responses.AddGeneric400();
			g.Responses.Add404ById("publication");
			return g;
		});

		app.MapGet("api/v1/publications", async (PublicationsRequest request, IValidator<PublicationsRequest> validator, ApplicationDbContext db) =>
		{
			var validationResult = validator.Validate(request);
			if (!validationResult.IsValid)
			{
				return Results.ValidationProblem(validationResult.ToDictionary());
			}

			var pubs = (await db.Publications
				.FilterByTokens(request)
				.ToPublicationsResponse()
				.SortBy(request)
				.Paginate(request)
				.ToListAsync())
			.FieldSelect(request);

			return Results.Ok(pubs);
		})
		.WithTags("Publications")
		.WithSummary("Returns a list of publications, filtered by the given criteria.")
		.Produces<IEnumerable<PublicationsResponse>>()
		.WithOpenApi(g =>
		{
			g.Parameters.AddFromQuery("systems", "The system codes to filter by", typeof(string));
			g.Parameters.AddFromQuery("classNames", "The publication class names to filter by", typeof(string));
			g.Parameters.AddFromQuery("startYear", "The start year to filter by", typeof(int));
			g.Parameters.AddFromQuery("endYear", "The end year to filter by", typeof(int));
			g.Parameters.AddFromQuery("genreNames", "the genres to filter by", typeof(string));
			g.Parameters.AddFromQuery("tagNames", "the names of the publication tags to filter by", typeof(string));
			g.Parameters.AddFromQuery("flagNames", "the names of the publication flags to filter by", typeof(string));
			g.Parameters.AddFromQuery("authorIds", "the ids of the authors to filter by", typeof(string));
			g.Parameters.AddFromQuery("showObsoleted", "indicates whether or not to return obsoleted publications", typeof(bool));
			g.Parameters.AddFromQuery("onlyObsoleted", "indicates whether or not to only return obsoleted publications", typeof(bool));
			g.Parameters.AddFromQuery("gameIds", "the ids of the games to filter by", typeof(string));
			g.Parameters.AddFromQuery("gameGroupIds", "the ids of the game groups to filter by", typeof(string));
			g.Parameters.AddBaseQueryParams();
			g.Responses.AddGeneric400();
			return g;
		});
	}
}
