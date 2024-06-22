using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.PublicationHistory)]
public class PublicationHistory(IPublicationHistory history) : WikiViewComponent
{
	public PublicationHistoryGroup History { get; set; } = new();

	public async Task<IViewComponentResult> InvokeAsync(int publicationId)
	{
		var gameHistory = await history.ForGameByPublication(publicationId);
		if (gameHistory is null)
		{
			return Error($"Invalid publication id: {publicationId}");
		}

		History = gameHistory;
		ViewData["Highlight"] = publicationId;
		return View();
	}
}
