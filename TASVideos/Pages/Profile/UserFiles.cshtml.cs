using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

using TASVideos.Data.Entity;
using TASVideos.Models;
using TASVideos.Tasks;

namespace TASVideos.Pages.Profile
{
	[Authorize]
	public class UserFilesModel : BasePageModel
	{
		private readonly UserManager<User> _userManager;
		private readonly UserFileTasks _fileTasks;

		public UserFilesModel(
			UserManager<User> userManager,
			UserFileTasks fileTasks,
			UserTasks userTasks)
			: base(userTasks)
		{
			_userManager = userManager;
			_fileTasks = fileTasks;
		}

		public string UserName { get; set; }

		public IEnumerable<UserFileModel> Files { get; set; } = new List<UserFileModel>();

		public async Task OnGet()
		{
			var user = await _userManager.GetUserAsync(User);
			UserName = user.UserName;
			Files = await _fileTasks.GetUserIndex(user.Id, includeHidden: true);
		}
	}
}
