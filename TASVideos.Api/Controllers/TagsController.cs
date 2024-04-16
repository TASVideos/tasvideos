using TASVideos.Api.Filters;

namespace TASVideos.Api.Controllers;

[AllowAnonymous]
[Route("api/v1/[controller]")]
public class TagsController(ITagService tagService) : Controller
{
	/// <summary>
	/// Creates a new tag
	/// </summary>
	/// <response code="201">The tag was created successfully.</response>
	/// <response code="400">The request parameters are invalid.</response>
	[HttpPost]
	[RequirePermission(PermissionTo.TagMaintenance)]
	public async Task<IActionResult> Create(TagAddEditRequest request)
	{
		var (id, result) = await tagService.Add(request.Code, request.DisplayName);

		switch (result)
		{
			case TagEditResult.DuplicateCode:
				ModelState.AddModelError(nameof(request.Code), $"{request.Code} already exists.");
				return Conflict(ModelState);
			case TagEditResult.Success:
				return Created(new Uri($"{Request.Path}/{id}", UriKind.Relative), id);
			default:
				return BadRequest();
		}
	}

	/// <summary>
	/// Updates an existing tag
	/// </summary>
	/// <response code="200">The tag was updated.</response>
	/// <response code="400">The request parameters are invalid.</response>
	/// <response code="404">A tag with the given id was not found.</response>
	[HttpPut("id")]
	[RequirePermission(PermissionTo.TagMaintenance)]
	public async Task<IActionResult> Update([FromQuery] int id, TagAddEditRequest request)
	{
		var result = await tagService.Edit(id, request.Code, request.DisplayName);
		switch (result)
		{
			case TagEditResult.NotFound:
				return NotFound();
			case TagEditResult.DuplicateCode:
				ModelState.AddModelError(nameof(request.Code), $"{request.Code} already exists.");
				return Conflict(ModelState);
			case TagEditResult.Success:
				return Ok();
			default:
				return BadRequest();
		}
	}

	/// <summary>
	/// Deletes an existing tag
	/// </summary>
	/// <response code="200">The tag was deleted successfully.</response>
	/// <response code="400">The request parameters are invalid.</response>
	/// <response code="404">A tag with the given id was not found.</response>
	/// <response code="409">The tag is in use and cannot be deleted.</response>
	[HttpDelete("id")]
	[RequirePermission(PermissionTo.TagMaintenance)]
	public async Task<IActionResult> Delete(int id)
	{
		var result = await tagService.Delete(id);
		switch (result)
		{
			case TagDeleteResult.NotFound:
				return NotFound();
			case TagDeleteResult.InUse:
				ModelState.AddModelError("", "The tag is in use and cannot be deleted.");
				return Conflict(ModelState);
			case TagDeleteResult.Success:
				return Ok();
			default:
				return BadRequest();
		}
	}
}
