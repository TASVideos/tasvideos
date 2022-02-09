using TASVideos.Api.Filters;
using TASVideos.Api.Requests;
using TASVideos.Data.Entity;

namespace TASVideos.Api.Controllers;

/// <summary>
/// The publication classes of TASVideos.
/// </summary>
[AllowAnonymous]
[Route("api/[controller]")]
public class ClassesController : Controller
{
	private readonly IClassService _classService;

	/// <summary>
	/// Initializes a new instance of the <see cref="ClassesController"/> class.
	/// </summary>
	public ClassesController(IClassService classService)
	{
		_classService = classService;
	}

	/// <summary>
	/// Returns a list of available publication classes.
	/// </summary>
	/// <response code="200">Returns the list of classes.</response>
	[HttpGet]
	[ProducesResponseType(typeof(IEnumerable<PublicationClass>), 200)]
	public async Task<IActionResult> GetAll()
	{
		var classes = await _classService.GetAll();
		return Ok(classes);
	}

	/// <summary>
	/// Returns a publication class with the given id.
	/// </summary>
	/// <response code="200">Returns a publication class.</response>
	/// <response code="400">The request parameters are invalid.</response>
	/// <response code="404">A publication class with the given id was not found.</response>
	[HttpGet("id")]
	public async Task<IActionResult> GetById(int id)
	{
		var publicationClass = await _classService.GetById(id);
		return publicationClass is null
			? NotFound()
			: Ok(publicationClass);
	}

	/// <summary>
	/// Creates a new publication class
	/// </summary>
	/// <response code="201">The publication class was created successfully.</response>
	/// <response code="400">The request parameters are invalid.</response>
	/// <response code="409">A publication class with the given name already exists.</response>
	[HttpPost]
	[RequirePermission(PermissionTo.ClassMaintenance)]
	public async Task<IActionResult> Create(ClassAddEditRequest request)
	{
		var (id, result) = await _classService.Add(new PublicationClass
		{
			Name = request.Name,
			Weight = request.Weight,
			IconPath = request.IconPath,
			Link = request.Link
		});

		switch (result)
		{
			case ClassEditResult.DuplicateName:
				ModelState.AddModelError(nameof(request.Name), $"{request.Name} already exists.");
				return Conflict(ModelState);
			case ClassEditResult.Success:
				return Created(new Uri($"{Request.Path}/{id}", UriKind.Relative), id);
			default:
				return BadRequest();
		}
	}

	/// <summary>
	/// Updates an existing publication class
	/// </summary>
	/// <response code="200">The class was updated.</response>
	/// <response code="400">The request parameters are invalid.</response>
	/// <response code="404">A class with the given id was not found.</response>
	/// <response code="409">A class with the given name already exists.</response>
	[HttpPut("id")]
	[RequirePermission(PermissionTo.ClassMaintenance)]
	public async Task<IActionResult> Update([FromQuery] int id, ClassAddEditRequest request)
	{
		var result = await _classService.Edit(id, new PublicationClass
		{
			Name = request.Name,
			Weight = request.Weight,
			IconPath = request.IconPath,
			Link = request.Link
		});
		switch (result)
		{
			case ClassEditResult.NotFound:
				return NotFound();
			case ClassEditResult.DuplicateName:
				ModelState.AddModelError(nameof(request.Name), $"{request.Name} already exists.");
				return Conflict(ModelState);
			case ClassEditResult.Success:
				return Ok();
			default:
				return BadRequest();
		}
	}

	/// <summary>
	/// Deletes an existing publication publication class
	/// </summary>
	/// <response code="200">The class was deleted successfully.</response>
	/// <response code="400">The request parameters are invalid.</response>
	/// <response code="404">A class with the given id was not found.</response>
	/// <response code="409">The class is in use and cannot be deleted.</response>
	[HttpDelete("id")]
	[RequirePermission(PermissionTo.ClassMaintenance)]
	public async Task<IActionResult> Delete(int id)
	{
		var result = await _classService.Delete(id);
		switch (result)
		{
			case ClassDeleteResult.NotFound:
				return NotFound();
			case ClassDeleteResult.InUse:
				ModelState.AddModelError("", "The publication class is in use and cannot be deleted.");
				return Conflict(ModelState);
			case ClassDeleteResult.Success:
				return Ok();
			default:
				return BadRequest();
		}
	}
}
