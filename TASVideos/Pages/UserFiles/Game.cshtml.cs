using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Models;
using TASVideos.Services;
using TASVideos.Tasks;

namespace TASVideos.Pages.UserFiles
{
	[AllowAnonymous]
	public class GameModel : BasePageModel
	{
		private readonly UserFileTasks _userFileTasks;

		public GameModel(
			UserFileTasks userFileTasks,
			UserManager userManager) 
			: base(userManager)
		{
			_userFileTasks = userFileTasks;
		}

		public GameFileModel Game { get; set; }

		[FromRoute]
		public int Id { get; set; }

		public async Task<IActionResult> OnGet()
		{
			Game = await _userFileTasks.GetFilesForGame(Id);
			if (Game == null)
			{
				return NotFound();
			}

			return Page();
		}
	}
}
