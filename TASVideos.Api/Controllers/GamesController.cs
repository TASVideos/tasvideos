using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using AutoMapper.QueryableExtensions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Api.Requests;
using TASVideos.Api.Responses;
using TASVideos.Data;

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
		/// Returns a list of available games
		/// </summary>
		/// <response code="200">Returns the list of games</response>
		/// /// <response code="400">The request parameters are invalid</response>
		[HttpGet]
		[ProducesResponseType(typeof(IEnumerable<SystemsResponse>), 200)]
		public async Task<IActionResult> GetAll(GamesRequest request)
		{
			if (!request.IsValidSort(typeof(GamesResponse)))
			{
				ModelState.AddModelError(nameof(request.Sort), "Invalid Sort parameter");
			}

			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var query = _db.Games.AsQueryable();

			if (request.SystemCodes.Any())
			{
				query = query.Where(g => request.SystemCodes.Contains(g.System.Code));
			}

			var games = (await query
				.ProjectTo<GamesResponse>()
				.SortBy(request)
				.Paginate(request)
				.ToListAsync())
				.FieldSelect(request);

			return Ok(games);
		}
	}
}
