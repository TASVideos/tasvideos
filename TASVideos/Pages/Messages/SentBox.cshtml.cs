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
	public class SentboxModel : BasePageModel
	{
		private readonly UserManager<User> _userManager;
		private readonly PrivateMessageTasks _pmTasks;

		public SentboxModel(
			UserManager<User> userManager,
			PrivateMessageTasks privateMessageTasks,
			UserTasks userTasks)
			: base(userTasks)
		{
			_userManager = userManager;
			_pmTasks = privateMessageTasks;
		}

		// TODO: rename this model
		[BindProperty]
		public IEnumerable<Models.SentboxModel> SentBox { get; set; } = new List<Models.SentboxModel>();

		public async Task OnGet()
		{
			var user = await _userManager.GetUserAsync(User);
			SentBox = await _pmTasks.GetUserSentBox(user);
		}
	}
}
