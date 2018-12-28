using Microsoft.AspNetCore.Authorization;
using TASVideos.Tasks;

namespace TASVideos.Pages
{
	[AllowAnonymous]
	public class IndexModel : BasePageModel
	{
		public IndexModel(UserTasks userTasks)
			: base(userTasks)
		{
		}

		public void OnGet()
		{
		}
	}
}
