using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using TASVideos.Api.Interfaces;

/*
 * General API TODOs:
 * Field selection is purely post processing and returns distinct objects,
 * so the record count might be less than the requested count
 * how do we document this? or do we want to try to do dynamic queryable field selection?
 */
namespace TASVideos.Api.Controllers;

/// <summary>
/// The VideoUpload of TASVideos.
/// </summary>
[AllowAnonymous]
[Route("api/v1/[controller]")]
public class VideoUploadController : Controller
{
	private readonly ILogger<VideoUploadController> _logger;
	private readonly IUsArchiveAPI _archiveAPI;
	private readonly IDifferentialVideoCreationService _differentialVideoCreationService;

	/// <summary>
	/// Initializes a new instance of the <see cref="VideoUploadController"/> class.
	/// </summary>
	public VideoUploadController(
		ILogger<VideoUploadController> logger,
		IUsArchiveAPI archiveApi,
		IDifferentialVideoCreationService differentialVideoCreationService)
	{
		_logger = logger;
		_archiveAPI = archiveApi;
		_differentialVideoCreationService = differentialVideoCreationService;
	}

	/// <summary>
	/// Allows Uploading of a video to be sent to USArchive
	/// </summary>
	/// <param name="file">The Video File</param>
	/// <returns>Friendly Response from ArchiveUs</returns>
	[HttpPost]
	public async Task<ActionResult> VideoUpload([FromForm] IFormFile file)
	{
		try
		{
			var newestVideo = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/videos/", $"{Guid.NewGuid()}_{file.FileName}");
			var steam = new FileStream(newestVideo, FileMode.Create);
			file.CopyTo(steam);
			var response = await _archiveAPI.UploadAsync(new VideoUpload() { /*do some magic */ });

			// UsArchive let us save?
			if (response.Success)
			{
				var timesWeCareAbout = _differentialVideoCreationService.DiffAsync(newestVideo, steam);

				// make the new vid using the timestamps above.
				return Ok(response);
			}

			return Accepted();
		}
		catch (Exception ex)
		{
			_logger.Log(LogLevel.Error, $"{ex.Message}. Length = {file.Length} \\n " +
														$"Name = {file.Name} \\n " +
														$"Content-Type = {file.ContentType} \\n");
			return BadRequest();
		}
	}
}
