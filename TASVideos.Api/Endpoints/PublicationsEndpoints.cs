using Microsoft.AspNetCore.Builder;

namespace TASVideos.Api;

// TODO: old swagger UI did client side validation of int, not letting non-ints be typed in, how is this done?
internal static class PublicationsEndpoints
{
	public static WebApplication MapPublications(this WebApplication app)
	{
		var group = app.MapGroup("api/v1/publications").WithTags("Publications");

		group.MapGet("{id}", async (int id, ApplicationDbContext db) =>
		{
			var pub = await db.Publications
				.ToPublicationsResponse()
				.SingleOrDefaultAsync(p => p.Id == id);

			return pub is null
				? Results.NotFound()
				: Results.Ok(pub);
		})
		.WithSummary("Returns a publication with the given id.")
		.Produces<PublicationsResponse>()
		.WithOpenApi(g =>
		{
			g.Responses.AddGeneric400();
			g.Responses.Add404ById("publication");
			return g;
		});

		group.MapGet("", async ([AsParameters]PublicationsRequest request, IValidator<PublicationsRequest> validator, ApplicationDbContext db) =>
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
			g.Parameters.AddStringFromQuery("systems", "The system codes to filter by");
			g.Parameters.AddStringFromQuery("classNames", "The publication class names to filter by");
			g.Parameters.AddIntFromQuery("startYear", "The start year to filter by");
			g.Parameters.AddIntFromQuery("endYear", "The end year to filter by");
			g.Parameters.AddStringFromQuery("genreNames", "the genres to filter by");
			g.Parameters.AddStringFromQuery("tagNames", "the names of the publication tags to filter by");
			g.Parameters.AddStringFromQuery("flagNames", "the names of the publication flags to filter by");
			g.Parameters.AddStringFromQuery("authorIds", "the ids of the authors to filter by");
			g.Parameters.AddBoolFromQuery("showObsoleted", "indicates whether or not to return obsoleted publications");
			g.Parameters.AddBoolFromQuery("onlyObsoleted", "indicates whether or not to only return obsoleted publications");
			g.Parameters.AddStringFromQuery("gameIds", "the ids of the games to filter by");
			g.Parameters.AddStringFromQuery("gameGroupIds", "the ids of the game groups to filter by");
			g.Parameters.AddBaseQueryParams();
			g.Responses.AddGeneric400();
			return g;
		});

		return app;
	}
}
