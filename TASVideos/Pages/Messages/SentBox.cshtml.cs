using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Data;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Messages;

[Authorize]
public class SentboxModel(ApplicationDbContext db) : BasePageModel
{
	[FromRoute]
	public int? Id { get; set; }

	public List<SentboxEntry> SentBox { get; set; } = [];

	public async Task OnGet()
	{
		var userId = User.GetUserId();
		SentBox = await db.PrivateMessages
			.ThatAreNotToUserDeleted()
			.FromUser(userId)
			.Select(pm => new SentboxEntry(
				pm.Id,
				pm.Subject,
				pm.ToUser!.UserName,
				pm.CreateTimestamp,
				pm.ReadOn.HasValue))
			.ToListAsync();
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

			// Do nothing on failure, likely the user has read at the same time
			await ConcurrentSave(db, "", "");
		}

		return BasePageRedirect("SentBox");
	}

	public record SentboxEntry(int Id, string? Subject, string To, DateTime SendDate, bool IsRead);
}
