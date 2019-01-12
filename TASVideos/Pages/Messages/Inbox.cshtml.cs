using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;
using TASVideos.Models;
using TASVideos.Tasks;

namespace TASVideos.Pages.Messages
{
	[Authorize]
	public class InboxModel : BasePageModel
	{
		private readonly UserManager<User> _userManager;
		private readonly ApplicationDbContext _db;

		public InboxModel(
			UserManager<User> userManager,
			ApplicationDbContext db,
			UserTasks userTasks)
			: base(userTasks)
		{
			_userManager = userManager;
			_db = db;
		}

		[FromRoute]
		public int? Id { get; set; }

		// TODO: rename this model
		[BindProperty]
		public IEnumerable<InboxEntry> Messages { get; set; } = new List<InboxEntry>();

		public async Task OnGet()
		{
			var user = await _userManager.GetUserAsync(User);
			Messages = await _db.PrivateMessages
				.ToUser(user)
				.ThatAreNotToUserDeleted()
				.ThatAreNotToUserSaved()
				.Select(pm => new InboxEntry
				{
					Id = pm.Id,
					Subject = pm.Subject,
					SendDate = pm.CreateTimeStamp,
					FromUser = pm.FromUser.UserName,
					IsRead = pm.ReadOn.HasValue
				})
				.ToListAsync();
		}

		// TODO: make this a post
		public async Task<IActionResult> OnGetSave()
		{
			if (!Id.HasValue)
			{
				return NotFound();
			}

			var user = await _userManager.GetUserAsync(User);

			var message = await _db.PrivateMessages
				.ToUser(user)
				.ThatAreNotToUserDeleted()
				.SingleOrDefaultAsync(pm => pm.Id == Id);

			if (message != null)
			{
				message.SavedForToUser = true;
				await _db.SaveChangesAsync();
			}

			return RedirectToPage("Inbox");
		}

		// TODO: make this a post
		public async Task<IActionResult> OnGetDelete()
		{
			if (!Id.HasValue)
			{
				return NotFound();
			}

			var user = await _userManager.GetUserAsync(User);

			var message = await _db.PrivateMessages
				.ToUser(user)
				.ThatAreNotToUserDeleted()
				.SingleOrDefaultAsync(pm => pm.Id == Id);

			if (message != null)
			{
				message.DeletedForToUser = true;
				await _db.SaveChangesAsync();
			}

			return RedirectToPage("Inbox");
		}
	}
}
