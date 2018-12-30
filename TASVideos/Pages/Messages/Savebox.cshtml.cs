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
	public class SaveboxModel : BasePageModel
	{
		private readonly UserManager<User> _userManager;
		private readonly PrivateMessageTasks _pmTasks;

		public SaveboxModel(
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
		public IEnumerable<Models.SaveboxModel> SaveBox { get; set; } = new List<Models.SaveboxModel>();

		public async Task OnGet()
		{
			var user = await _userManager.GetUserAsync(User);
			SaveBox = await _pmTasks.GetUserSaveBox(user);
		}
	}
}
