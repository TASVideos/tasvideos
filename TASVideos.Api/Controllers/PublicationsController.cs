using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Api.Requests;
using TASVideos.Api.Responses;
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

		/// <summary>
		/// Initializes a new instance of the <see cref="PublicationsController"/> class. 
		/// </summary>
		public PublicationsController(ApplicationDbContext db)
		{
			_db = db;
		}

		/// <summary>
		/// Gets all the things
		/// </summary>
		[HttpGet]
		public async Task<IActionResult> GetAll(PublicationsRequest request)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest();
			}

			// TODO: automapper
			// TODO: set up global exception handling to return a json payload from api calls but error page for page calls
			try
			{
				var pubs = (await _db.Publications
				.Select(p => new PublicationsResponse
				{
					Id = p.Id,
					Title = p.Title,
					Branch = p.Branch,
					EmulatorVersion = p.EmulatorVersion
				})
				.SortBy(request)
				.Paginate(request)
				.ToListAsync())
				.FieldSelect(request);

				return Ok(pubs);
			}
			catch (Exception ex)
			{
				return StatusCode(500);
			}
		}
	}
}
