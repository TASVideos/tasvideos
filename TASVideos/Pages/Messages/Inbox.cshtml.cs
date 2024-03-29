﻿namespace TASVideos.Pages.Messages;

[Authorize]
[IgnoreAntiforgeryToken]
public class InboxModel(ApplicationDbContext db) : BasePageModel
{
	[FromRoute]
	public int? Id { get; set; }

	[BindProperty]
	public List<InboxEntry> Messages { get; set; } = [];

	public async Task OnGet()
	{
		Messages = await db.PrivateMessages
			.ToUser(User.GetUserId())
			.ThatAreNotToUserDeleted()
			.ThatAreNotToUserSaved()
			.Select(pm => new InboxEntry(
				pm.Id,
				pm.Subject,
				pm.FromUser!.UserName,
				pm.CreateTimestamp,
				pm.ReadOn.HasValue))
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

	public record InboxEntry(int Id, string? Subject, string From, DateTime Date, bool IsRead);
}
