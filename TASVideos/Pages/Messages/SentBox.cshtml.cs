using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;
using TASVideos.Models;
using TASVideos.Tasks;
namespace TASVideos.Pages.Messages
{
	[Authorize]
	public class SentboxModel : BasePageModel
	{
		private readonly UserManager<User> _userManager;
		private readonly ApplicationDbContext _db;

		public SentboxModel(
			UserManager<User> userManager,
			ApplicationDbContext db,
			UserTasks userTasks)
			: base(userTasks)
		{
			_userManager = userManager;
			_db = db;
		}

		public IEnumerable<SentboxEntry> SentBox { get; set; } = new List<SentboxEntry>();

		public async Task OnGet()
		{
			var user = await _userManager.GetUserAsync(User);
			SentBox = await _db.PrivateMessages
				.ThatAreNotToUserDeleted()
				.Where(pm => pm.FromUserId == user.Id)
				.Select(pm => new SentboxEntry
				{
					Id = pm.Id,
					Subject = pm.Subject,
					ToUser = pm.ToUser.UserName,
					SendDate = pm.CreateTimeStamp,
					HasBeenRead = pm.ReadOn.HasValue
				})
				.ToListAsync();
		}
	}
}
