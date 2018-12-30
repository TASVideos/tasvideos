using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using TASVideos.Data;
using TASVideos.Models;
using TASVideos.Tasks;

namespace TASVideos.Pages.Users
{
	[AllowAnonymous]
	public class ListModel : BasePageModel
	{
		public ListModel(UserTasks userTasks) : base(userTasks)
		{
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
	}
}
