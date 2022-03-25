using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TASVideos.Api.Requests;
using TASVideos.Api.Responses;
using TASVideos.Core;
using TASVideos.Data;
using TASVideos.Data.Entity;

/*
 * General API TODOs:
 * Field selection is purely post processing and returns distinct objects,
 * so the record count might be less than the requested count
 * how do we document this? or do we want to try to do dynamic queryable field selection?
 */
namespace TASVideos.Api.Controllers;

/// <summary>
/// The publications of TASVideos.
/// </summary>
[AllowAnonymous]
[Route("api/v1/[controller]")]
public class PublicationsController : Controller
{
	private readonly ApplicationDbContext _db;
	private readonly IMapper _mapper;

	/// <summary>
	/// Initializes a new instance of the <see cref="PublicationsController"/> class.
	/// </summary>
	public PublicationsController(ApplicationDbContext db, IMapper mapper)
	{
		_db = db;
		_mapper = mapper;
	}

	/// <summary>
	/// Returns a publication with the given id.
	/// </summary>
	/// <response code="200">Returns a publication.</response>
	/// <response code="400">The request parameters are invalid.</response>
	/// <response code="404">A publication with the given id was not found.</response>
	[HttpGet("{id}")]
	[ProducesResponseType(typeof(PublicationsResponse), 200)]
	public async Task<IActionResult> Get(int id)
	{
		var pub = await _mapper
			.ProjectTo<PublicationsResponse>(_db.Publications)
			.SingleOrDefaultAsync(p => p.Id == id);

		return pub is null
			? NotFound()
			: Ok(pub);
	}

	/// <summary>
	/// Returns a list of publications, filtered by the given criteria.
	/// </summary>
	/// <response code="200">Returns the list of publications.</response>
	/// <response code="400">The request parameters are invalid.</response>
	[HttpGet]
	[Validate]
	[ProducesResponseType(typeof(IEnumerable<PublicationsResponse>), 200)]
	public async Task<IActionResult> GetAll([FromQuery] PublicationsRequest request)
	{
		var pubs = (await _mapper
			.ProjectTo<PublicationsResponse>(
				_db.Publications
				.FilterByTokens(request))
			.SortBy(request)
			.Paginate(request)
			.ToListAsync())
			.FieldSelect(request);
		return Ok(pubs);
	}
}
