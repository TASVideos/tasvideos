namespace TASVideos.Pages.Profile;

[Authorize]
public class WatchedTopicsModel(ITopicWatcher topicWatcher) : BasePageModel
{
	[FromQuery]
	public PagingModel Search { get; set; } = new();

	public PageOf<WatchedTopic> Watches { get; set; } = new([], new());

	public async Task OnGet()
	{
		Watches = await topicWatcher.UserWatches(User.GetUserId(), Search);
	}

	public async Task<IActionResult> OnPostStopWatching(int topicId)
	{
		await topicWatcher.UnwatchTopic(topicId, User.GetUserId());
		return BasePageRedirect("WatchedTopics");
	}

	public async Task<IActionResult> OnPostStopAllWatching()
	{
		await topicWatcher.UnwatchAllTopics(User.GetUserId());
		return BasePageRedirect("WatchedTopics");
	}
}
