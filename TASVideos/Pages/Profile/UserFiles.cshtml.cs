using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using TASVideos.Models;
using TASVideos.Tasks;

namespace TASVideos.Pages.Profile
{
	[Authorize]
	public class UserFilesModel : BasePageModel
	{
		private readonly UserFileTasks _fileTasks;

		public UserFilesModel(
			UserFileTasks fileTasks,
			UserTasks userTasks)
			: base(userTasks)
		{
			_fileTasks = fileTasks;
		}

		public string UserName { get; set; }

		public IEnumerable<UserFileModel> Files { get; set; } = new List<UserFileModel>();

		public async Task OnGet()
		{
			UserName = User.Identity.Name;
			Files = await _fileTasks.GetUserIndex(UserName, includeHidden: true);
		}
	}
}
