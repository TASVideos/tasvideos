using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.SupportedMovieTypes)]
public class SupportedMovieTypes(IMovieFormatDeprecator deprecator) : WikiViewComponent
{
	public IReadOnlyDictionary<string, DeprecatedMovieFormat?> Formats { get; set; } = null!;

	public async Task<IViewComponentResult> InvokeAsync()
	{
		Formats = await deprecator.GetAll();
		return View();
	}
}
