using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using AutoMapper.QueryableExtensions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

using TASVideos.Api.Requests;
using TASVideos.Api.Responses;
using TASVideos.Data;

namespace TASVideos.Api.Controllers
{
	/// <summary>
	/// The submissions of TASVideos
	/// </summary>
	[AllowAnonymous]
	[Route("api/v1/[controller]")]
	public class SubmissionsController : Controller
	{
		private readonly ApplicationDbContext _db;

		/// <summary>
		/// Initializes a new instance of the <see cref="SubmissionsController"/> class. 
		/// </summary>
		public SubmissionsController(ApplicationDbContext db)
		{
			_db = db;
		}

		/// <summary>
		/// Returns a submission with the given id
		/// </summary>
		/// <response code="200">Returns the list of publications</response>
		/// <response code="400">The request parameters are invalid</response>
		/// <response code="404">A submission with the given id was not found</response>
		[HttpGet("{id}")]
		[ProducesResponseType(typeof(SubmissionsResponse), 200)]
		public async Task<IActionResult> Get(int id)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest();
			}

			var sub = await _db.Submissions
				.ProjectTo<SubmissionsResponse>()
				.SingleOrDefaultAsync(p => p.Id == id);

			if (sub == null)
			{
				return NotFound();
			}

			return Ok(sub);
		}

		/// <summary>
		/// Returns a list of publications, filtered by the given criteria
		/// </summary>
		/// <response code="200">Returns the list of publications</response>
		/// <response code="400">The request parameters are invalid</response>
		[HttpGet]
		[ProducesResponseType(typeof(IEnumerable<SubmissionsResponse>), 200)]
		public async Task<IActionResult> GetAll(SubmissionsRequest request)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest();
			}

			var subs = (await _db.Submissions
				.ProjectTo<SubmissionsResponse>()
				.SortBy(request)
				.Paginate(request)
				.ToListAsync())
				.FieldSelect(request);

			return Ok(subs);
		}
	}
}
