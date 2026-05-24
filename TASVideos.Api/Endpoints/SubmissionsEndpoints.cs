namespace TASVideos.Api.Endpoints;

internal static class SubmissionsEndpoints
{
	public static WebApplication MapSubmissions(this WebApplication app)
	{
		var group = app.MapApiGroup("Submissions");

		group
			.MapGet("{id:int}", async (int id, ApplicationDbContext db) => ApiResults.OkOr404(
				await db.Submissions
					.ToSubmissionsResponse()
					.SingleOrDefaultAsync(p => p.Id == id)))
			.ProducesFromId<SubmissionsResponse>("submission");

		group.MapGet("", async ([AsParameters] SubmissionsRequest request, HttpContext context, ApplicationDbContext db) =>
		{
			var validationError = ApiResults.Validate(request, context);
			if (validationError is not null)
			{
				return validationError;
			}

			var subs = (await db.Submissions
				.FilterBy(request)
				.ToSubmissionsResponse()
				.SortAndPaginate(request)
				.ToListAsync())
				.FieldSelect(request);

			return Results.Ok(subs);
		})
		.Receives<SubmissionsRequest>()
		.ProducesList<SubmissionsResponse>("a list of submissions, searchable by the given criteria.");

		group.MapPost("{id:int}/regenerate-title", async (int id, ApplicationDbContext db, HttpContext context) =>
		{
			var authError = ApiResults.Authorize(PermissionTo.EditSubmissions, context);
			if (authError is not null)
			{
				return authError;
			}

			var submission = await db.Submissions
				.IncludeTitleTables()
				.FirstOrDefaultAsync(p => p.Id == id);

			if (submission is null)
			{
				return Results.NotFound();
			}

			submission.GenerateTitle();

			var topic = await db.ForumTopics.FindAsync(submission.TopicId);
			topic?.Title = submission.Title;

			await db.SaveChangesAsync();

			return Results.Ok();
		})
		.WithSummary("Regenerates the title of the submission with the given id.")
		.Produces(StatusCodes.Status200OK)
		.Produces(StatusCodes.Status404NotFound);

		return app;
	}
}
