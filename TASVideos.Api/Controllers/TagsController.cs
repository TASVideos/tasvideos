using TASVideos.Api.Filters;
using TASVideos.Api.Requests;
using TASVideos.Core;
using TASVideos.Data.Entity;

namespace TASVideos.Api.Controllers;

/// <summary>
/// The publication tags that categorize publications
/// </summary>
[AllowAnonymous]
[Route("api/v1/[controller]")]
public class TagsController : Controller
{
	private readonly ITagService _tagService;

	/// <summary>
	/// Initializes a new instance of the <see cref="TagsController"/> class.
	/// </summary>
	public TagsController(ITagService tagService)
	{
		_tagService = tagService;
	}

	/// <summary>
	/// Returns a tag with the given id.
	/// </summary>
	/// <response code="200">Returns a tag.</response>
	/// <response code="400">The request parameters are invalid.</response>
	/// <response code="404">A tag with the given id was not found.</response>
	[HttpGet("id")]
	public async Task<IActionResult> GetById(int id)
	{
		var tag = await _tagService.GetById(id);
		return tag is null
			? NotFound()
			: Ok(tag);
	}

	/// <summary>
	/// Returns a list of available tags
	/// </summary>
	/// <response code="200">Returns the list of games.</response>
	/// <response code="400">The request parameters are invalid.</response>
	[HttpGet]
	public async Task<IActionResult> GetAll([FromQuery] ApiRequest request)
	{
		var tags = (await _tagService.GetAll())
			.AsQueryable()
			.SortBy(request)
			.Paginate(request)
			.AsEnumerable()
			.FieldSelect(request);

		return Ok(tags);
	}

	/// <summary>
	/// Creates a new tag
	/// </summary>
	/// <response code="201">The tag was created successfully.</response>
	/// <response code="400">The request parameters are invalid.</response>
	[HttpPost]
	[RequirePermission(PermissionTo.TagMaintenance)]
	public async Task<IActionResult> Create(TagAddEditRequest request)
	{
		var (id, result) = await _tagService.Add(request.Code, request.DisplayName);

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
		var result = await _tagService.Edit(id, request.Code, request.DisplayName);
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
		var result = await _tagService.Delete(id);
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
