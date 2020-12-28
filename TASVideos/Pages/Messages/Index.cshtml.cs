using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using TASVideos.Models;
using TASVideos.Services;

namespace TASVideos.Pages.Messages
{
	[Authorize]
	public class IndexModel : BasePageModel
	{
		private readonly UserManager _userManager;

		public IndexModel(UserManager userManager)
		{
			_userManager = userManager;
		}

		[FromRoute]
		public int Id { get; set; }

		public PrivateMessageModel Message { get; set; } = new ();

		public async Task<IActionResult> OnGet()
		{
			var message = await _userManager.GetMessage(User.GetUserId(), Id);

			if (message == null)
			{
				return NotFound();
			}

			Message = message;
			Message.RenderedText = RenderPost(Message.Text, Message.EnableBbCode, Message.EnableHtml);
			return Page();
		}
	}
}
