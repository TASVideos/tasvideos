namespace TASVideos.Pages.Submissions;

[RequirePermission(PermissionTo.DeprecateMovieParsers)]
public class DeprecateMovieFormatsModel(IMovieFormatDeprecator deprecator, IExternalMediaPublisher publisher) : BasePageModel
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
			await publisher.SendDeprecation($"[{extension}]({{0}}) deprecation status set to {deprecate} by {User.Name()}");
		}
		else
		{
			ErrorStatusMessage("Unable to save");
		}

		return BasePageRedirect("DeprecateMovieFormats");
	}
}
