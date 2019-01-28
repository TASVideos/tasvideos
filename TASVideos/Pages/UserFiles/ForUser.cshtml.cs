using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using TASVideos.Models;
using TASVideos.Tasks;

namespace TASVideos.Pages.UserFiles
{
	[AllowAnonymous]
	public class ForUserModel : BasePageModel
	{
		private readonly UserFileTasks _userFileTasks;

		public ForUserModel(UserFileTasks userFileTasks)
		{
			_userFileTasks = userFileTasks;
		}

		[FromRoute]
		public string UserName { get; set; }

		public IEnumerable<UserFileModel> Files { get; set; } = new List<UserFileModel>();

		public async Task OnGet()
		{
			Files = await _userFileTasks.GetUserIndex(UserName, includeHidden: false);
		}
	}
}
