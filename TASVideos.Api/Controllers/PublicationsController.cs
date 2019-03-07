using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;

namespace TASVideos.Api.Controllers
{
	/// <summary>
	/// The publications of tasvideos
	/// </summary>
	[AllowAnonymous]
	[Route("api/v1/[controller]")]
	public class PublicationsController : Controller
	{
		private readonly ApplicationDbContext _db;

		public PublicationsController(ApplicationDbContext db)
		{
			_db = db;
		}

		/// <summary>
		/// Gets all the things
		/// </summary>
		[HttpGet]
		public async Task<IActionResult> GetAll()
		{
			try
			{
				var pubs = await _db.Publications
				.Take(10)
				.Select(p => new
				{
					p.Id,
					p.Title,
					p.Branch,
					p.EmulatorVersion
				})
				.ToListAsync();

				return Ok(pubs);
			}
			catch (Exception ex)
			{
				return BadRequest();

			}


			return BadRequest();
		}
	}
}
