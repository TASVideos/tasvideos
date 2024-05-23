namespace TASVideos.Pages.Messages;

[Authorize]
[IgnoreAntiforgeryToken]
public class InboxModel(ApplicationDbContext db) : BasePageModel
{
	[FromQuery]
	public PagingModel Paging { get; set; } = new();

	[FromRoute]
	public int? Id { get; set; }

	public PageOf<InboxEntry> Messages { get; set; } = new([]);

	public async Task OnGet()
	{
		Messages = await db.PrivateMessages
			.ToUser(User.GetUserId())
			.ThatAreNotToUserDeleted()
			.ThatAreNotToUserSaved()
			.OrderBy(m => m.ReadOn.HasValue)
			.ThenByDescending(m => m.CreateTimestamp)
			.Select(pm => new InboxEntry(
				pm.Id,
				pm.Subject,
				pm.FromUser!.UserName,
				pm.CreateTimestamp,
				pm.ReadOn.HasValue))
			.PageOf(Paging);
	}

	public async Task<IActionResult> OnPostSave()
	{
		if (!Id.HasValue)
		{
			return NotFound();
		}

		var message = await GetMessage();
		if (message is not null)
		{
			message.SavedForToUser = true;
			await ConcurrentSave(db, "Message successfully saved", "Unable to save message");
		}

		return BasePageRedirect("Savebox");
	}

	public async Task<IActionResult> OnPostDelete()
	{
		if (!Id.HasValue)
		{
			return NotFound();
		}

		var message = await GetMessage();
		if (message is not null)
		{
			message.DeletedForToUser = true;
			await ConcurrentSave(db, "Message successfully deleted", "Unable to deleted message");
		}

		return BasePageRedirect("Inbox");
	}

	private async Task<PrivateMessage?> GetMessage()
		=> await db.PrivateMessages
			.ToUser(User.GetUserId())
			.ThatAreNotToUserDeleted()
			.SingleOrDefaultAsync(pm => pm.Id == Id);

	public record InboxEntry(int Id, string? Subject, string From, DateTime Date, bool IsRead);
}
