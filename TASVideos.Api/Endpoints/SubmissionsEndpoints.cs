using Microsoft.AspNetCore.Builder;

namespace TASVideos.Api;
internal static class SubmissionsEndpoints
{
	public static WebApplication MapSubmissions(this WebApplication app)
	{
		var group = app.MapApiGroup("Submissions");

		group.MapGet("{id}", async (int id, ApplicationDbContext db) =>
		{
			var sub = await db.Submissions
				.ToSubmissionsResponse()
				.SingleOrDefaultAsync(p => p.Id == id);

			return sub is null
				? Results.NotFound()
				: Results.Ok(sub);
		})
		.WithSummary("Returns a submission with the given id.")
		.Produces<SubmissionsResponse>()
		.WithOpenApi(g =>
		{
			g.Responses.AddGeneric400();
			g.Responses.Add404ById("submissions");
			return g;
		});

		group.MapGet("", async ([AsParameters]SubmissionsRequest request, IValidator<SubmissionsRequest> validator, ApplicationDbContext db) =>
		{
			var validationResult = validator.Validate(request);
			if (!validationResult.IsValid)
			{
				return ApiResults.ValidationError(validationResult);
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
			g.Parameters.AddStringFromQuery("user", "the author/submitter name to filter by");
			g.Parameters.AddStringFromQuery("statuses", "the statuses to filter by");
			g.Parameters.AddIntFromQuery("startYear", "The start year to filter by");
			g.Parameters.AddIntFromQuery("endYear", "The end year to filter by");
			g.Parameters.AddStringFromQuery("systems", "The system codes to filter by");
			g.Parameters.AddStringFromQuery("games", "the ids of the games to filter by");
			g.Parameters.AddBaseQueryParams();
			g.Responses.AddGeneric400();
			return g;
		});

		return app;
	}
}
