using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Pages.Messages.Models;

namespace TASVideos.Pages.Messages;

[Authorize]
public class SentboxModel(ApplicationDbContext db) : BasePageModel
{
	[FromRoute]
	public int? Id { get; set; }

	public IEnumerable<SentboxEntry> SentBox { get; set; } = new List<SentboxEntry>();

	public async Task OnGet()
	{
		var userId = User.GetUserId();
		SentBox = await db.PrivateMessages
			.ThatAreNotToUserDeleted()
			.FromUser(userId)
			.Select(pm => new SentboxEntry
			{
				Id = pm.Id,
				Subject = pm.Subject,
				ToUser = pm.ToUser!.UserName,
				SendDate = pm.CreateTimestamp,
				HasBeenRead = pm.ReadOn.HasValue
			})
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
}
