using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Models;
using TASVideos.Tasks;

namespace TASVideos.Pages.Messages
{
	[Authorize]
	public class SaveboxModel : BasePageModel
	{
		private readonly UserManager<User> _userManager;
		private readonly ApplicationDbContext _db;

		public SaveboxModel(
			UserManager<User> userManager,
			ApplicationDbContext db,
			UserTasks userTasks)
			: base(userTasks)
		{
			_userManager = userManager;
			_db = db;
		}

		public IEnumerable<SaveboxEntry> SaveBox { get; set; } = new List<SaveboxEntry>();

		public async Task OnGet()
		{
			var user = await _userManager.GetUserAsync(User);
			SaveBox = await _db.PrivateMessages
				.Where(pm => (pm.SavedForFromUser && !pm.DeletedForFromUser && pm.FromUserId == user.Id)
					|| (pm.SavedForToUser && !pm.DeletedForToUser && pm.ToUserId == user.Id))
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
