using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Services;

namespace TASVideos.Pages.Wiki
{
	[AllowAnonymous]
	public class DoIfModuleModel : BasePageModel
	{
		[FromQuery]
		public string Condition { get; set; }
	}
}
