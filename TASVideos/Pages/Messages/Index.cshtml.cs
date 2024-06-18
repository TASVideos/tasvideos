namespace TASVideos.Pages.Messages;

[Authorize]
public class IndexModel(IPrivateMessageService privateMessageService) : BasePageModel
{
	[FromRoute]
	public int Id { get; set; }

	public PrivateMessageDto PrivateMessage { get; set; } = new();

	public async Task<IActionResult> OnGet()
	{
		var message = await privateMessageService.GetMessage(User.GetUserId(), Id);

		if (message is null)
		{
			return NotFound();
		}

		PrivateMessage = message;
		return Page();
	}
}
