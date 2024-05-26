namespace TASVideos.Pages.Messages;

[Authorize]
public class SentboxModel(ApplicationDbContext db) : BasePageModel
{
	[FromQuery]
	public PagingModel Paging { get; set; } = new();

	[FromRoute]
	public int? Id { get; set; }

	public PageOf<SentboxEntry> SentBox { get; set; } = new([]);

	public async Task OnGet()
	{
		var userId = User.GetUserId();
		SentBox = await db.PrivateMessages
			.ThatAreNotToUserDeleted()
			.FromUser(userId)
			.OrderBy(m => m.ReadOn.HasValue)
			.ThenByDescending(m => m.CreateTimestamp)
			.Select(pm => new SentboxEntry(
				pm.Id,
				pm.Subject,
				pm.ToUser!.UserName,
				pm.CreateTimestamp,
				pm.ReadOn.HasValue))
			.PageOf(Paging);
	}

	public async Task<IActionResult> OnPostDelete()
	{
		if (!Id.HasValue)
		{
			return NotFound();
		}

		var message = await db.PrivateMessages
			.FromUser(User.GetUserId())
			.ThatAreNotToUserDeleted()
			.SingleOrDefaultAsync(pm => pm.Id == Id);

		if (message is not null)
		{
			db.PrivateMessages.Remove(message);
			await db.TrySaveChanges(); // Do nothing on failure, likely the user has read at the same time
		}

		return BasePageRedirect("SentBox");
	}

	public record SentboxEntry(int Id, string? Subject, string To, DateTime SendDate, bool IsRead);
}
