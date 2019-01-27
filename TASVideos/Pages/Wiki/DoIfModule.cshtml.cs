using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Services;

namespace TASVideos.Pages.Wiki
{
	[AllowAnonymous]
	public class DoIfModuleModel : BasePageModel
	{
		public DoIfModuleModel(UserManager userManager) : base(userManager)
		{
		}

		[FromQuery]
		public string Condition { get; set; }
	}
}
