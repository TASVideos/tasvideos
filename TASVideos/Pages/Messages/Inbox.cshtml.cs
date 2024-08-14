namespace TASVideos.Pages.Messages;

[Authorize]
[IgnoreAntiforgeryToken]
public class InboxModel(IPrivateMessageService privateMessageService) : BasePageModel
{
	[FromQuery]
	public PagingModel Paging { get; set; } = new();

	[FromRoute]
	public int? Id { get; set; }

	public PageOf<InboxEntry> Messages { get; set; } = new([], new());

	public async Task OnGet()
	{
		Messages = await privateMessageService.GetInbox(User.GetUserId(), Paging);
	}

	public async Task<IActionResult> OnPostSave()
	{
		if (!Id.HasValue)
		{
			return NotFound();
		}

		var result = await privateMessageService.SaveMessage(User.GetUserId(), Id.Value);
		SetMessage(result, "Message successfully saved", "Unable to save message");

		return BasePageRedirect("Savebox");
	}

	public async Task<IActionResult> OnPostDelete()
	{
		if (!Id.HasValue)
		{
			return NotFound();
		}

		var result = await privateMessageService.DeleteMessage(User.GetUserId(), Id.Value);
		SetMessage(result, "Message successfully deleted", "Unable to delete message");

		return BasePageRedirect("Inbox");
	}
}
