using TASVideos.Core.Services.ExternalMediaPublisher;

namespace TASVideos.Pages.Submissions;

[RequirePermission(PermissionTo.DeprecateMovieParsers)]
public class DeprecateMovieFormatsModel(
	IMovieFormatDeprecator deprecator,
	ExternalMediaPublisher publisher)
	: BasePageModel
{
	public IReadOnlyDictionary<string, DeprecatedMovieFormat?> MovieExtensions { get; set; } = new Dictionary<string, DeprecatedMovieFormat?>();

	public async Task OnGet()
	{
		MovieExtensions = await deprecator.GetAll();
	}

	public async Task<IActionResult> OnPost(string extension, bool deprecate)
	{
		if (!deprecator.IsMovieExtension(extension))
		{
			return BadRequest($"Invalid format {extension}");
		}

		var result = deprecate
			? await deprecator.Deprecate(extension)
			: await deprecator.Allow(extension);

		if (result)
		{
			SuccessStatusMessage($"{extension} allowed successfully");
			await SendAnnouncement(extension, deprecate);
		}
		else
		{
			ErrorStatusMessage("Unable to save");
		}

		return BasePageRedirect("DeprecateMovieFormats");
	}

	private async Task SendAnnouncement(string extension, bool deprecate)
	{
		await publisher.SendSubmissionEdit(
			$"{extension} deprecation status set to {deprecate} by {User.Name()}",
			$"[{extension}]({{0}}) deprecation status set to {deprecate} by {User.Name()}",
			"",
			"");
	}
}
