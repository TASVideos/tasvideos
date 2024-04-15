using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace TASVideos.Api.Controllers;
internal static class SubmissionsApiMapper
{
	public static void Map(WebApplication app)
	{
		app.MapGet("api/v1/submissions/{id}", async (int id, ApplicationDbContext db) =>
		{
			var sub = await db.Submissions
				.ToSubmissionsResponse()
				.SingleOrDefaultAsync(p => p.Id == id);

			return sub is null
				? Results.NotFound()
				: Results.Ok(sub);
		})
		.WithTags("Submissions")
		.WithSummary("Returns a submission with the given id.")
		.Produces<SubmissionsResponse>()
		.WithOpenApi(g =>
		{
			g.Responses.AddGeneric400();
			g.Responses.Add404ById("submissions");
			return g;
		});

		app.MapGet("api/v1/submissions", async (SubmissionsRequest request, IValidator<SubmissionsRequest> validator, ApplicationDbContext db) =>
		{
			var validationResult = validator.Validate(request);
			if (!validationResult.IsValid)
			{
				return Results.ValidationProblem(validationResult.ToDictionary());
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
		.WithTags("Submissions")
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
	}
}
