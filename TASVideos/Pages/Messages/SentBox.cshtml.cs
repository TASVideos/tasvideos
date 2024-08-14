namespace TASVideos.Pages.Messages;

[Authorize]
public class SentboxModel(IPrivateMessageService privateMessageService) : BasePageModel
{
	[FromQuery]
	public PagingModel Paging { get; set; } = new();

	[FromRoute]
	public int? Id { get; set; }

	public PageOf<SentboxEntry> SentBox { get; set; } = new([], new());

	public async Task OnGet()
	{
		SentBox = await privateMessageService.GetSentInbox(User.GetUserId(), Paging);
	}
}
