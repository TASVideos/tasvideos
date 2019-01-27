using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using TASVideos.Extensions;
using TASVideos.Models;
using TASVideos.Services;

namespace TASVideos.Pages.Messages
{
	[Authorize]
	public class IndexModel : BasePageModel
	{
		private readonly UserManager _userManager;

		public IndexModel(
			UserManager userManager)
			: base(userManager)
		{
			_userManager = userManager;
		}

		[FromRoute]
		public int Id { get; set; }

		[BindProperty]
		public PrivateMessageModel Message { get; set; }  = new PrivateMessageModel();

		public async Task<IActionResult> OnGet()
		{
			Message = await _userManager.GetMessage(User.GetUserId(), Id);

			if (Message == null)
			{
				return NotFound();
			}

			Message.RenderedText = RenderPost(Message.Text, Message.EnableBbCode, Message.EnableHtml);
			return Page();
		}
	}
}
