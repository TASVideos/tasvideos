using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity.Forum;
using TASVideos.Models;

namespace TASVideos.Pages.Messages
{
	[Authorize]
	public class SentboxModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;

		public SentboxModel(ApplicationDbContext db)
		{
			_db = db;
		}

		public IEnumerable<SentboxEntry> SentBox { get; set; } = new List<SentboxEntry>();

		public async Task OnGet()
		{
			var userId = User.GetUserId();
			SentBox = await _db.PrivateMessages
				.ThatAreNotToUserDeleted()
				.Where(pm => pm.FromUserId == userId)
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
