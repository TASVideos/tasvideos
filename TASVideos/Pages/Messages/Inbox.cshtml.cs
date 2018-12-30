using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using TASVideos.Data.Entity;
using TASVideos.Tasks;

namespace TASVideos.Pages.Messages
{
	[Authorize]
	public class InboxModel : BasePageModel
	{
		private readonly UserManager<User> _userManager;
		private readonly PrivateMessageTasks _pmTasks;

		public InboxModel(
			UserManager<User> userManager,
			PrivateMessageTasks privateMessageTasks,
			UserTasks userTasks)
			: base(userTasks)
		{
			_userManager = userManager;
			_pmTasks = privateMessageTasks;
		}

		[FromRoute]
		public int? Id { get; set; }

		// TODO: rename this model
		[BindProperty]
		public IEnumerable<Models.InboxModel> Messages { get; set; } = new List<Models.InboxModel>();

		public async Task OnGet()
		{
			var user = await _userManager.GetUserAsync(User);
			Messages = await _pmTasks.GetUserInBox(user);
		}

		// TODO: make this a post
		public async Task<IActionResult> OnGetSave()
		{
			if (!Id.HasValue)
			{
				return NotFound();
			}

			var user = await _userManager.GetUserAsync(User);
			await _pmTasks.SaveMessageToUser(user, Id.Value);

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
			await _pmTasks.DeleteMessageToUser(user, Id.Value);

			return RedirectToPage("Inbox");
		}
	}
}
