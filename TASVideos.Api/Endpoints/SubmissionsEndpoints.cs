using Microsoft.AspNetCore.Builder;

namespace TASVideos.Api;
internal static class SubmissionsEndpoints
{
	public static WebApplication MapSubmissions(this WebApplication app)
	{
		var group = app.MapApiGroup("Submissions");

		group
			.MapGet("{id}", async (int id, ApplicationDbContext db) => ApiResults.OkOr404(
				await db.Submissions
					.ToSubmissionsResponse()
					.SingleOrDefaultAsync(p => p.Id == id)))
			.ProducesFromId<SubmissionsResponse>("submission");

		group.MapGet("", async ([AsParameters]SubmissionsRequest request, HttpContext context, ApplicationDbContext db) =>
		{
			var validationError = ApiResults.Validate(request, context);
			if (validationError is not null)
			{
				return validationError;
			}

			var subs = (await db.Submissions
				.FilterBy(request)
				.ToSubmissionsResponse()
				.SortBy(request)
				.Paginate(request)
				.ToListAsync())
				.FieldSelect(request);

			return Results.Ok(subs);
		})
		.Receives<SubmissionsRequest>()
		.ProducesList<SubmissionsResponse>("a list of submissions, searchable by the given criteria.");

		return app;
	}
}
