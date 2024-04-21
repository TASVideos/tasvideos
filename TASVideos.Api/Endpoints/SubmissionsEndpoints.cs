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
			.DocumentIdGet("submission", typeof(SubmissionsResponse));

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
		.WithSummary("Returns a list of submissions, filtered by the given criteria.")
		.Produces<IEnumerable<SubmissionsResponse>>()
		.WithOpenApi(g =>
		{
			g.Parameters.Describe("User", "the author/submitter name to filter by");
			g.Parameters.Describe("Statuses", "the statuses to filter by");
			g.Parameters.Describe("StartYear", "The start year to filter by");
			g.Parameters.Describe("EndYear", "The end year to filter by");
			g.Parameters.Describe("Systems", "The system codes to filter by");
			g.Parameters.Describe("Games", "the ids of the games to filter by");
			g.Parameters.Describe("StartType", "Gets the start type of the movie. 0 = Power On, 1 = Sram, 2 = Savestate");
			g.Parameters.DescribeBaseQueryParams();
			g.Responses.AddGeneric400();
			return g;
		});

		return app;
	}
}
