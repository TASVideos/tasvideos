using System.Collections.Generic;
using System.Threading.Tasks;

using AutoMapper.QueryableExtensions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Api.Requests;
using TASVideos.Api.Responses;
using TASVideos.Data;
using TASVideos.Data.Entity.Game;

namespace TASVideos.Api.Controllers
{
	/// <summary>
	/// The games of TASVideos that can be associated with content
	/// </summary>
	[AllowAnonymous]
	[Route("api/v1/[controller]")]
	public class GamesController : Controller
	{
		private readonly ApplicationDbContext _db;

		/// <summary>
		/// Initializes a new instance of the <see cref="GamesController"/> class. 
		/// </summary>
		public GamesController(ApplicationDbContext db)
		{
			_db = db;
		}

		/// <summary>
		/// Returns a game with the given id
		/// </summary>
		/// <response code="200">Returns a game</response>
		/// <response code="400">The request parameters are invalid</response>
		/// <response code="404">A publication with the given id was not found</response>
		[HttpGet("{id}")]
		[ProducesResponseType(typeof(GamesResponse), 200)]
		public async Task<IActionResult> Get(int id)
		{
			var pub = await _db.Games
				.ProjectTo<GamesResponse>()
				.SingleOrDefaultAsync(p => p.Id == id);

			if (pub == null)
			{
				return NotFound();
			}

			return Ok(pub);
		}

		/// <summary>
		/// Returns a list of available games
		/// </summary>
		/// <response code="200">Returns the list of games</response>
		/// /// <response code="400">The request parameters are invalid</response>
		[Validate]
		[HttpGet]
		[ProducesResponseType(typeof(IEnumerable<GamesResponse>), 200)]
		public async Task<IActionResult> GetAll(GamesRequest request)
		{
			var games = (await _db.Games
				.ForSystemCodes(request.SystemCodes)
				.ProjectTo<GamesResponse>()
				.SortBy(request)
				.Paginate(request)
				.ToListAsync())
				.FieldSelect(request);

			return Ok(games);
		}
	}
}
