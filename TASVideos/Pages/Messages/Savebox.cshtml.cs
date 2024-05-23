namespace TASVideos.Pages.Messages;

[Authorize]
public class SaveboxModel(ApplicationDbContext db) : BasePageModel
{
	public List<SaveboxEntry> SaveBox { get; set; } = [];

	public async Task OnGet()
	{
		var userId = User.GetUserId();
		var userName = User.Name();
		SaveBox = await db.PrivateMessages
			.ThatAreSavedByUser(userId)
			.OrderBy(m => m.ToUser!.UserName == userName)
			.ThenByDescending(m => m.CreateTimestamp)
			.Select(pm => new SaveboxEntry(
				pm.Id,
				pm.Subject,
				pm.FromUser!.UserName,
				pm.ToUser!.UserName,
				pm.CreateTimestamp))
			.ToListAsync();
	}

	public record SaveboxEntry(int Id, string? Subject, string From, string To, DateTime SendDate);
}
