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
			g.Parameters.Describe<PublicationsRequest>();
			g.Responses.AddGeneric400();
			return g;
		});

		return app;
	}
}
