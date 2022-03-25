using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TASVideos.Api.Requests;
using TASVideos.Api.Responses;
using TASVideos.Core;
using TASVideos.Data;
using TASVideos.Data.Entity.Game;

namespace TASVideos.Api.Controllers;

/// <summary>
/// The games of TASVideos that can be associated with content.
/// </summary>
[AllowAnonymous]
[Route("api/v1/[controller]")]
public class GamesController : Controller
{
	private readonly ApplicationDbContext _db;
	private readonly IMapper _mapper;

	/// <summary>
	/// Initializes a new instance of the <see cref="GamesController"/> class.
	/// </summary>
	public GamesController(ApplicationDbContext db, IMapper mapper)
	{
		_db = db;
		_mapper = mapper;
	}

	/// <summary>
	/// Returns a game with the given id.
	/// </summary>
	/// <response code="200">Returns a game.</response>
	/// <response code="400">The request parameters are invalid.</response>
	/// <response code="404">A publication with the given id was not found.</response>
	[HttpGet("{id}")]
	[ProducesResponseType(typeof(GamesResponse), 200)]
	public async Task<IActionResult> Get(int id)
	{
		var pub = await _mapper.ProjectTo<GamesResponse>(_db.Games)
			.SingleOrDefaultAsync(p => p.Id == id);
		return pub is null
			? NotFound()
			: Ok(pub);
	}

	/// <summary>
	/// Returns a list of available games.
	/// </summary>
	/// <response code="200">Returns the list of games.</response>
	/// /// <response code="400">The request parameters are invalid.</response>
	[HttpGet]
	[Validate]
	[ProducesResponseType(typeof(IEnumerable<GamesResponse>), 200)]
	public async Task<IActionResult> GetAll([FromQuery] GamesRequest request)
	{
		var games = (await _mapper.ProjectTo<GamesResponse>(
			_db.Games.ForSystemCodes(request.SystemCodes))
			.SortBy(request)
			.Paginate(request)
			.ToListAsync())
			.FieldSelect(request);

		return Ok(games);
	}
}
