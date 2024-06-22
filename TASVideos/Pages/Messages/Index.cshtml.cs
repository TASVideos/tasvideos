namespace TASVideos.Pages.Messages;

[Authorize]
public class IndexModel(IPrivateMessageService privateMessageService) : BasePageModel
{
	[FromRoute]
	public int Id { get; set; }

	public Message PrivateMessage { get; set; } = null!;

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
