using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;

using TASVideos.Models;
using TASVideos.Services;
using TASVideos.Tasks;

namespace TASVideos.Pages.UserFiles
{
	[AllowAnonymous]
	public class IndexModel : BasePageModel
	{
		private readonly UserFileTasks _userFileTasks;

		public IndexModel(
			UserFileTasks userFileTasks,
			UserManager userManager)
			: base(userManager)
		{
			_userFileTasks = userFileTasks;
		}

		public UserFileIndexModel Data { get; set; } = new UserFileIndexModel();

		public async Task OnGet()
		{
			Data = await _userFileTasks.GetIndex();
		}
	}
}
