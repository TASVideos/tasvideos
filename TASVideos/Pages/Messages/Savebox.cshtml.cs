using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Pages.Messages.Models;

namespace TASVideos.Pages.Messages;

[Authorize]
public class SaveboxModel : BasePageModel
{
	private readonly ApplicationDbContext _db;

	public SaveboxModel(ApplicationDbContext db)
	{
		_db = db;
	}

	public IEnumerable<SaveboxEntry> SaveBox { get; set; } = new List<SaveboxEntry>();

	public async Task OnGet()
	{
		var userId = User.GetUserId();
		SaveBox = await _db.PrivateMessages
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
