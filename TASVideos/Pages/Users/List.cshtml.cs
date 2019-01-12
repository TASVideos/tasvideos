using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Models;
using TASVideos.Tasks;

namespace TASVideos.Pages.Users
{
	[AllowAnonymous]
	public class ListModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;

		public ListModel(
			ApplicationDbContext db,
			UserTasks userTasks) 
			: base(userTasks)
		{
			_db = db;
		}

		[FromQuery]
		public PagedModel Search { get; set; } = new PagedModel();

		public PageOf<UserListModel> Users { get; set; }

		public void OnGet()
		{
			Users = UserTasks.GetPageOfUsers(Search);
		}

		public async Task<IActionResult> OnGetSearch(string partial)
		{
			if (!string.IsNullOrWhiteSpace(partial) && partial.Length > 2)
			{
				var matches = await UserTasks.GetUsersByPartial(partial);
				return new JsonResult(matches);
			}

			return new JsonResult(new List<string>());
		}

		public async Task<IActionResult> OnGetVerifyUniqueUserName(string userName)
		{
			if (string.IsNullOrWhiteSpace(userName))
			{
				return new JsonResult(false);
			}

			var exists = await _db.Users.Exists(userName);
			return new JsonResult(exists);
		}
	}
}
