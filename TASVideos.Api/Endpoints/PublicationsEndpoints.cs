namespace TASVideos.Api;

internal static class PublicationsEndpoints
{
	public static WebApplication MapPublications(this WebApplication app)
	{
		var group = app.MapApiGroup("Publications");

		group
			.MapGet("{id:int}", async (int id, ApplicationDbContext db) => ApiResults.OkOr404(
				await db.Publications
					.ToPublicationsResponse()
					.SingleOrDefaultAsync(p => p.Id == id)))
			.ProducesFromId<PublicationsResponse>("publication");

		group.MapGet("", async ([AsParameters] PublicationsRequest request, HttpContext context, ApplicationDbContext db) =>
		{
			var validationError = ApiResults.Validate(request, context);
			if (validationError is not null)
			{
				return validationError;
			}

			var pubs = (await db.Publications
				.FilterByTokens(request)
				.ToPublicationsResponse()
				.SortAndPaginate(request)
				.ToListAsync())
			.FieldSelect(request);

			return Results.Ok(pubs);
		})
		.Receives<PublicationsRequest>()
		.ProducesList<PublicationsResponse>("a list of publications, searchable by the given criteria");

		return app;
	}
}
