using System.Collections.Generic;
using System.Threading.Tasks;

using AutoMapper.QueryableExtensions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Api.Requests;
using TASVideos.Api.Responses;
using TASVideos.Data;
using TASVideos.Data.Entity;

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
		/// Returns a publication with the given id
		/// </summary>
		/// <response code="200">Returns the list of publications</response>
		/// <response code="400">The request parameters are invalid</response>
		/// <response code="404">A publication with the given id was not found</response>
		[HttpGet("{id}")]
		public async Task<IActionResult> Get(int id)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest();
			}

			var pub = await _db.Publications
				.ProjectTo<PublicationsResponse>()
				.SingleOrDefaultAsync(p => p.Id == id);

			if (pub == null)
			{
				return NotFound();
			}

			return Ok(pub);
		}

		/// <summary>
		/// Returns a list of publications, filtered by the given criteria
		/// </summary>
		/// <response code="200">Returns the list of publications</response>
		/// <response code="400">The request parameters are invalid</response>
		[HttpGet]
		[ProducesResponseType(typeof(IEnumerable<PublicationsResponse>), 200)]
		public async Task<IActionResult> GetAll(PublicationsRequest request)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest();
			}

			var pubs = (await _db.Publications
				.FilterByTokens(request)
				.ProjectTo<PublicationsResponse>()
				.SortBy(request)
				.Paginate(request)
				.ToListAsync())
				.FieldSelect(request);

			return Ok(pubs);
		}
	}
}
