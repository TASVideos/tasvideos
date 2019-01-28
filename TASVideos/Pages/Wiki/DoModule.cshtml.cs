using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TASVideos.Pages.Wiki
{
	[AllowAnonymous]
	public class DoModuleModel : BasePageModel
	{
		[FromQuery]
		public string Name { get; set; }

		[FromQuery]
		public string Params { get; set; }
	}
}
