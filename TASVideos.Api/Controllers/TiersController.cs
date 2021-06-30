using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Api.Filters;
using TASVideos.Api.Requests;
using TASVideos.Core.Services;
using TASVideos.Data.Entity;

namespace TASVideos.Api.Controllers
{
	/// <summary>
	/// The publication tiers of TASVideos.
	/// </summary>
	[AllowAnonymous]
	[Route("api/[controller]")]
	public class TiersController : Controller
	{
		private readonly ITierService _tierService;

		/// <summary>
		/// Initializes a new instance of the <see cref="TiersController"/> class.
		/// </summary>
		public TiersController(ITierService tierService)
		{
			_tierService = tierService;
		}

		/// <summary>
		/// Returns a list of available publication tiers.
		/// </summary>
		/// <response code="200">Returns the list of tiers.</response>
		[HttpGet]
		[ProducesResponseType(typeof(IEnumerable<Tier>), 200)]
		public async Task<IActionResult> GetAll()
		{
			var tiers = await _tierService.GetAll();
			return Ok(tiers);
		}

		/// <summary>
		/// Returns a publication tier with the given id.
		/// </summary>
		/// <response code="200">Returns a tier.</response>
		/// <response code="400">The request parameters are invalid.</response>
		/// <response code="404">A tier with the given id was not found.</response>
		[HttpGet("id")]
		public async Task<IActionResult> GetById(int id)
		{
			var tier = await _tierService.GetById(id);
			return tier is null
				? NotFound()
				: Ok(tier);
		}

		/// <summary>
		/// Creates a new publication tier
		/// </summary>
		/// <response code="201">The tag was created successfully.</response>
		/// <response code="400">The request parameters are invalid.</response>
		/// <response code="409">A tier with the given name already exists.</response>
		[HttpPost]
		[RequirePermission(PermissionTo.TierMaintenance)]
		public async Task<IActionResult> Create(TierAddEditRequest request)
		{
			var (id, result) = await _tierService.Add(new Tier
			{
				Name = request.Name,
				Weight = request.Weight,
				IconPath = request.IconPath,
				Link = request.Link
			});

			switch (result)
			{
				case TierEditResult.DuplicateName:
					ModelState.AddModelError(nameof(request.Name), $"{request.Name} already exists.");
					return Conflict(ModelState);
				case TierEditResult.Success:
					return Created(new Uri($"{Request.Path}/{id}", UriKind.Relative), id);
				default:
					return BadRequest();
			}
		}

		/// <summary>
		/// Updates an existing publication tier
		/// </summary>
		/// <response code="200">The tier was updated.</response>
		/// <response code="400">The request parameters are invalid.</response>
		/// <response code="404">A tier with the given id was not found.</response>
		/// <response code="409">A tier with the given name already exists.</response>
		[HttpPut("id")]
		[RequirePermission(PermissionTo.TierMaintenance)]
		public async Task<IActionResult> Update([FromQuery] int id, TierAddEditRequest request)
		{
			var result = await _tierService.Edit(id, new Tier
			{
				Name = request.Name,
				Weight = request.Weight,
				IconPath = request.IconPath,
				Link = request.Link
			});
			switch (result)
			{
				case TierEditResult.NotFound:
					return NotFound();
				case TierEditResult.DuplicateName:
					ModelState.AddModelError(nameof(request.Name), $"{request.Name} already exists.");
					return Conflict(ModelState);
				case TierEditResult.Success:
					return Ok();
				default:
					return BadRequest();
			}
		}

		/// <summary>
		/// Deletes an existing publication tier
		/// </summary>
		/// <response code="200">The tier was deleted successfully.</response>
		/// <response code="400">The request parameters are invalid.</response>
		/// <response code="404">A tier with the given id was not found.</response>
		/// <response code="409">The tier is in use and cannot be deleted.</response>
		[HttpDelete("id")]
		[RequirePermission(PermissionTo.TagMaintenance)]
		public async Task<IActionResult> Delete(int id)
		{
			var result = await _tierService.Delete(id);
			switch (result)
			{
				case TierDeleteResult.NotFound:
					return NotFound();
				case TierDeleteResult.InUse:
					ModelState.AddModelError("", "The tier is in use and cannot be deleted.");
					return Conflict(ModelState);
				case TierDeleteResult.Success:
					return Ok();
				default:
					return BadRequest();
			}
		}
	}
}
