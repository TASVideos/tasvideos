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
	public class SentboxModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;

		public SentboxModel(ApplicationDbContext db)
		{
			_db = db;
		}

		[FromRoute]
		public int? Id { get; set; }

		public IEnumerable<SentboxEntry> SentBox { get; set; } = new List<SentboxEntry>();

		public async Task OnGet()
		{
			var userId = User.GetUserId();
			SentBox = await _db.PrivateMessages
				.ThatAreNotToUserDeleted()
				.FromUser(userId)
				.Select(pm => new SentboxEntry
				{
					Id = pm.Id,
					Subject = pm.Subject,
					ToUser = pm.ToUser!.UserName,
					SendDate = pm.CreateTimeStamp,
					HasBeenRead = pm.ReadOn.HasValue
				})
				.ToListAsync();
		}

		public async Task<IActionResult> OnPostDelete()
		{
			if (!Id.HasValue)
			{
				return NotFound();
			}

			var message = await _db.PrivateMessages
				.FromUser(User.GetUserId())
				.ThatAreNotToUserDeleted()
				.SingleOrDefaultAsync(pm => pm.Id == Id);

			if (message != null)
			{
				_db.PrivateMessages.Remove(message);
				try
				{
					await _db.SaveChangesAsync();
				}
				catch(DbUpdateConcurrencyException)
				{
					// Do nothing, likely the user has read at the same time
				}
			}

			return RedirectToPage("SentBox");
		}
	}
}
