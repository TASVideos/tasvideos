using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Pages.Messages.Models;

namespace TASVideos.Pages.Messages
{
	[Authorize]
	[IgnoreAntiforgeryToken]
	public class InboxModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;

		public InboxModel(ApplicationDbContext db)
		{
			_db = db;
		}

		[FromRoute]
		public int? Id { get; set; }

		[BindProperty]
		public IEnumerable<InboxEntry> Messages { get; set; } = new List<InboxEntry>();

		public async Task OnGet()
		{
			Messages = await _db.PrivateMessages
				.ToUser(User.GetUserId())
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

		public async Task<IActionResult> OnPostSave()
		{
			if (!Id.HasValue)
			{
				return NotFound();
			}

			var message = await _db.PrivateMessages
				.ToUser(User.GetUserId())
				.ThatAreNotToUserDeleted()
				.SingleOrDefaultAsync(pm => pm.Id == Id);

			if (message != null)
			{
				message.SavedForToUser = true;
				await _db.SaveChangesAsync();
			}

			return RedirectToPage("Savebox");
		}

		public async Task<IActionResult> OnPostDelete()
		{
			if (!Id.HasValue)
			{
				return NotFound();
			}

			var message = await _db.PrivateMessages
				.ToUser(User.GetUserId())
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
