using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Services;

namespace TASVideos.Pages.Wiki
{
	[AllowAnonymous]
	public class DoModuleModel : BasePageModel
	{
		public DoModuleModel(UserManager userManager) : base(userManager)
		{
		}

		[FromQuery]
		public string Name { get; set; }

		[FromQuery]
		public string Params { get; set; }
	}
}
