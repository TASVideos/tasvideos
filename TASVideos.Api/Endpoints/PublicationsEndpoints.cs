using Microsoft.AspNetCore.Builder;

namespace TASVideos.Api;

internal static class PublicationsEndpoints
{
	public static WebApplication MapPublications(this WebApplication app)
	{
		var group = app.MapApiGroup("Publications");

		group
			.MapGet("{id}", async (int id, ApplicationDbContext db) => ApiResults.OkOr404(
				await db.Publications
					.ToPublicationsResponse()
					.SingleOrDefaultAsync(p => p.Id == id)))
			.DocumentIdGet("publication", typeof(PublicationsResponse));

		group.MapGet("", async ([AsParameters]PublicationsRequest request, HttpContext context, ApplicationDbContext db) =>
		{
			var validationError = ApiResults.Validate(request, context);
			if (validationError is not null)
			{
				return validationError;
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
		.WithSummary("Returns a list of publications, filtered by the given criteria.")
		.Produces<IEnumerable<PublicationsResponse>>()
		.WithOpenApi(g =>
		{
			g.Parameters.Describe("Systems", "The system codes to filter by");
			g.Parameters.Describe("ClassNames", "The publication class names to filter by");
			g.Parameters.Describe("StartYear", "The start year to filter by");
			g.Parameters.Describe("EndYear", "The end year to filter by");
			g.Parameters.Describe("GenreNames", "the genres to filter by");
			g.Parameters.Describe("TagNames", "the names of the publication tags to filter by");
			g.Parameters.Describe("FlagNames", "the names of the publication flags to filter by");
			g.Parameters.Describe("AuthorIds", "the ids of the authors to filter by");
			g.Parameters.Describe("ShowObsoleted", "indicates whether or not to return obsoleted publications");
			g.Parameters.Describe("OnlyObsoleted", "indicates whether or not to only return obsoleted publications");
			g.Parameters.Describe("GameIds", "the ids of the games to filter by");
			g.Parameters.Describe("GameGroupIds", "the ids of the game groups to filter by");
			g.Parameters.DescribeBaseQueryParams();
			g.Responses.AddGeneric400();
			return g;
		});

		return app;
	}
}
