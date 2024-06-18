namespace TASVideos.Pages.Messages;

[Authorize]
public class SaveboxModel(IPrivateMessageService privateMessageService) : BasePageModel
{
	public ICollection<SaveboxEntry> SaveBox { get; set; } = [];

	public async Task OnGet()
	{
		SaveBox = await privateMessageService.GetSavebox(User.GetUserId());
	}
}
