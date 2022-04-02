using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TASVideos.Api.Requests;
using TASVideos.Api.Responses;
using TASVideos.Core;
using TASVideos.Data;
using TASVideos.Data.Entity;

namespace TASVideos.Api.Controllers;

/// <summary>
/// The submissions of TASVideos.
/// </summary>
[AllowAnonymous]
[Route("api/v1/[controller]")]
public class SubmissionsController : Controller
{
	private readonly ApplicationDbContext _db;
	private readonly IMapper _mapper;

	/// <summary>
	/// Initializes a new instance of the <see cref="SubmissionsController"/> class.
	/// </summary>
	public SubmissionsController(ApplicationDbContext db, IMapper mapper)
	{
		_db = db;
		_mapper = mapper;
	}

	/// <summary>
	/// Returns a submission with the given id.
	/// </summary>
	/// <response code="200">Returns the list of publications.</response>
	/// <response code="400">The request parameters are invalid.</response>
	/// <response code="404">A submission with the given id was not found.</response>
	[HttpGet("{id}")]
	[ProducesResponseType(typeof(SubmissionsResponse), 200)]
	public async Task<IActionResult> Get(int id)
	{
		var sub = await _mapper
			.ProjectTo<SubmissionsResponse>(_db.Submissions)
			.SingleOrDefaultAsync(p => p.Id == id);

		return sub is null
			? NotFound()
			: Ok(sub);
	}

	/// <summary>
	/// Returns a list of publications, filtered by the given criteria.
	/// </summary>
	/// <response code="200">Returns the list of publications.</response>
	/// <response code="400">The request parameters are invalid.</response>
	[HttpGet]
	[Validate]
	[ProducesResponseType(typeof(IEnumerable<SubmissionsResponse>), 200)]
	public async Task<IActionResult> GetAll([FromQuery] SubmissionsRequest request)
	{
		var subs = (await _mapper.ProjectTo<SubmissionsResponse>(
			_db.Submissions
				.FilterBy(request))
			.SortBy(request)
			.Paginate(request)
			.ToListAsync())
			.FieldSelect(request);

		return Ok(subs);
	}
}
