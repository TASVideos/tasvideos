using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Pages.Messages.Models;

namespace TASVideos.Pages.Messages;

[Authorize]
[IgnoreAntiforgeryToken]
public class InboxModel(ApplicationDbContext db) : BasePageModel
{
	[FromRoute]
	public int? Id { get; set; }

	[BindProperty]
	public IEnumerable<InboxEntry> Messages { get; set; } = new List<InboxEntry>();

	public async Task OnGet()
	{
		Messages = await db.PrivateMessages
			.ToUser(User.GetUserId())
			.ThatAreNotToUserDeleted()
			.ThatAreNotToUserSaved()
			.Select(pm => new InboxEntry
			{
				Id = pm.Id,
				Subject = pm.Subject,
				SendDate = pm.CreateTimestamp,
				FromUser = pm.FromUser!.UserName,
				IsRead = pm.ReadOn.HasValue
			})
			.ToListAsync();
	}

	public async Task<IActionResult> OnPostSave()
	{
		if (!Id.HasValue)
		{
			return NotFound();
		}

		var message = await db.PrivateMessages
			.ToUser(User.GetUserId())
			.ThatAreNotToUserDeleted()
			.SingleOrDefaultAsync(pm => pm.Id == Id);

		if (message is not null)
		{
			message.SavedForToUser = true;
			await db.SaveChangesAsync();
		}

		return BasePageRedirect("Savebox");
	}

	public async Task<IActionResult> OnPostDelete()
	{
		if (!Id.HasValue)
		{
			return NotFound();
		}

		var message = await db.PrivateMessages
			.ToUser(User.GetUserId())
			.ThatAreNotToUserDeleted()
			.SingleOrDefaultAsync(pm => pm.Id == Id);

		if (message is not null)
		{
			message.DeletedForToUser = true;
			await db.SaveChangesAsync();
		}

		return BasePageRedirect("Inbox");
	}
}
