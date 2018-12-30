using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using TASVideos.Data.Entity;
using TASVideos.Models;
using TASVideos.Tasks;

namespace TASVideos.Pages.Messages
{
	[Authorize]
	public class IndexModel : BasePageModel
	{
		private readonly UserManager<User> _userManager;
		private readonly PrivateMessageTasks _pmTasks;

		public IndexModel(
			UserManager<User> userManager,
			PrivateMessageTasks privateMessageTasks,
			UserTasks userTasks)
			: base(userTasks)
		{
			_userManager = userManager;
			_pmTasks = privateMessageTasks;
		}

		[FromRoute]
		public int Id { get; set; }

		[BindProperty]
		public PrivateMessageModel Message { get; set; }  = new PrivateMessageModel();

		public async Task<IActionResult> OnGet()
		{
			var user = await _userManager.GetUserAsync(User);
			Message = await _pmTasks.GetMessage(user, Id);

			if (Message == null)
			{
				return NotFound();
			}

			Message.RenderedText = RenderPost(Message.Text, Message.EnableBbCode, Message.EnableHtml);
			return Page();
		}
	}
}
