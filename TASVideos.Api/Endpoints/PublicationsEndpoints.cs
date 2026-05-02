namespace TASVideos.Api.Endpoints;

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

		group.MapPost("{id:int}/regenerate-title", async (int id, ApplicationDbContext db, HttpContext context) =>
		{
			var authError = ApiResults.Authorize(PermissionTo.EditPublicationMetaData, context);
			if (authError is not null)
			{
				return authError;
			}

			var publication = await db.Publications
				.IncludeTitleTables()
				.FirstOrDefaultAsync(p => p.Id == id);

			if (publication is null)
			{
				return Results.NotFound();
			}

			publication.Title = publication.GenerateTitle();

			await db.SaveChangesAsync();

			return Results.Ok();
		})
		.WithSummary("Regenerates the title of the publication with the given id. This does not affect any YouTube titles.")
		.Produces(StatusCodes.Status200OK)
		.Produces(StatusCodes.Status404NotFound);

		return app;
	}
}
