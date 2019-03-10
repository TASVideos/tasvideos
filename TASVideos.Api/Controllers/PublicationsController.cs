using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Api.Requests;
using TASVideos.Api.Responses;
using TASVideos.Data;

/*
 * General API TODOs:
 * Not every column is sortable, how do we inform the user which can be sorted by?
 * Throw bad request when attempting to sort by a non-sortable column
 * Field selection is purely post processing and returns distinct objects,
 *	so the record count might be less than the requested count
 *  how do we document this? or do we want to try to do dynamic queryable field selection?
 */
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
				var query = _db.Publications.AsQueryable();
				
				if (!string.IsNullOrWhiteSpace(request.SystemCode))
				{
					query = query.Where(p => p.System.Code == request.SystemCode);
				}

				var pubs = (await query.Select(p => new PublicationsResponse
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
