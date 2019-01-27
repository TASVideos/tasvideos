using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Extensions;
using TASVideos.Models;
using TASVideos.Services;

namespace TASVideos.Pages.Messages
{
	[Authorize]
	public class SaveboxModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;

		public SaveboxModel(
			ApplicationDbContext db,
			UserManager userManager)
			: base(userManager)
		{
			_db = db;
		}

		public IEnumerable<SaveboxEntry> SaveBox { get; set; } = new List<SaveboxEntry>();

		public async Task OnGet()
		{
			var userId = User.GetUserId();
			SaveBox = await _db.PrivateMessages
				.Where(pm => (pm.SavedForFromUser && !pm.DeletedForFromUser && pm.FromUserId == userId)
					|| (pm.SavedForToUser && !pm.DeletedForToUser && pm.ToUserId == userId))
				.Select(pm => new SaveboxEntry
				{
					Id = pm.Id,
					Subject = pm.Subject,
					FromUser = pm.FromUser.UserName,
					ToUser = pm.ToUser.UserName,
					SendDate = pm.CreateTimeStamp
				})
				.ToListAsync();
		}
	}
}
