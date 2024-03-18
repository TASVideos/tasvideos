namespace TASVideos.Api.Controllers;

/// <summary>
/// The submissions of TASVideos.
/// </summary>
[AllowAnonymous]
[Route("api/v1/[controller]")]
public class SubmissionsController(ApplicationDbContext db) : Controller
{
	/// <summary>
	/// Returns a submission with the given id.
	/// </summary>
	/// <response code="200">Returns the list of submissions.</response>
	/// <response code="400">The request parameters are invalid.</response>
	/// <response code="404">A submission with the given id was not found.</response>
	[HttpGet("{id}")]
	[ProducesResponseType(typeof(SubmissionsResponse), 200)]
	public async Task<IActionResult> Get(int id)
	{
		var sub = await db.Submissions
			.ToSubmissionsResponse()
			.SingleOrDefaultAsync(p => p.Id == id);

		return sub is null
			? NotFound()
			: Ok(sub);
	}

	/// <summary>
	/// Returns a list of submissions, filtered by the given criteria.
	/// </summary>
	/// <response code="200">Returns the list of submissions.</response>
	/// <response code="400">The request parameters are invalid.</response>
	[HttpGet]
	[Validate]
	[ProducesResponseType(typeof(IEnumerable<SubmissionsResponse>), 200)]
	public async Task<IActionResult> GetAll([FromQuery] SubmissionsRequest request)
	{
		var subs = (await db.Submissions
			.FilterBy(request)
			.ToSubmissionsResponse()
			.SortBy(request)
			.Paginate(request)
			.ToListAsync())
			.FieldSelect(request);

		return Ok(subs);
	}
}
