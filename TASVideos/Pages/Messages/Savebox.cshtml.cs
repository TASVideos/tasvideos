using Microsoft.AspNetCore.Authorization;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Pages.Messages.Models;

namespace TASVideos.Pages.Messages;

[Authorize]
public class SaveboxModel(ApplicationDbContext db) : BasePageModel
{
	public IEnumerable<SaveboxEntry> SaveBox { get; set; } = [];

	public async Task OnGet()
	{
		var userId = User.GetUserId();
		SaveBox = await db.PrivateMessages
			.ThatAreSavedByUser(userId)
			.Select(pm => new SaveboxEntry
			{
				Id = pm.Id,
				Subject = pm.Subject,
				FromUser = pm.FromUser!.UserName,
				ToUser = pm.ToUser!.UserName,
				SendDate = pm.CreateTimestamp
			})
			.ToListAsync();
	}
}
