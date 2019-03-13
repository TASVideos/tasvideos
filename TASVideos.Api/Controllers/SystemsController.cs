using System.Collections.Generic;
using System.Threading.Tasks;

using AutoMapper.QueryableExtensions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Api.Responses;
using TASVideos.Data;

namespace TASVideos.Api.Controllers
{
	/// <summary>
	/// The game systems supported by TASVideos
	/// </summary>
	[AllowAnonymous]
	[Route("api/v1/[controller]")]
	public class SystemsController : Controller
	{
		private readonly ApplicationDbContext _db;

		/// <summary>
		/// Initializes a new instance of the <see cref="SystemsController"/> class. 
		/// </summary>
		public SystemsController(ApplicationDbContext db)
		{
			_db = db;
		}

		/// <summary>
		/// Returns a game system with the given id
		/// </summary>
		/// <response code="200">Returns the list of publications</response>
		/// <response code="400">The request parameters are invalid</response>
		/// <response code="404">A system with the given id was not found</response>
		[HttpGet("{id}")]
		[ProducesResponseType(typeof(SystemsResponse), 200)]
		public async Task<IActionResult> Get(int id)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var system = await _db.GameSystems
				.ProjectTo<SystemsResponse>()
				.SingleOrDefaultAsync(p => p.Id == id);

			if (system == null)
			{
				return NotFound();
			}

			return Ok(system);
		}

		/// <summary>
		/// Returns a list of available game systems, including supported framerates
		/// </summary>
		/// <response code="200">Returns the list of systems</response>
		[HttpGet]
		[ProducesResponseType(typeof(IEnumerable<SystemsResponse>), 200)]
		public async Task<IActionResult> GetAll()
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var systems = await _db.GameSystems
				.ProjectTo<SystemsResponse>()
				.ToListAsync();

			return Ok(systems);
		}
	}
}
