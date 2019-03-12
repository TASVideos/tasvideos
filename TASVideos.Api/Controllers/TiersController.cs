using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity;

namespace TASVideos.Api.Controllers
{
	/// <summary>
	/// The publication tiers of TASVideos
	/// </summary>
	[AllowAnonymous]
	[Route("api/avi/[controller]")]
	public class TiersController : Controller
	{
		private readonly ApplicationDbContext _db;

		/// <summary>
		/// Initializes a new instance of the <see cref="TiersController"/> class. 
		/// </summary>
		public TiersController(ApplicationDbContext db)
		{
			_db = db;
		}

		/// <summary>
		/// Returns a list of available publication tiers
		/// </summary>
		/// <response code="200">Returns the list of tiers</response>
		[HttpGet]
		[ProducesResponseType(typeof(IEnumerable<Tier>), 200)]
		public async Task<IActionResult> GetAll()
		{
			if (!ModelState.IsValid)
			{
				return BadRequest();
			}

			var tiers = await _db.Tiers.ToListAsync();
			return Ok(tiers);
		}
	}
}
