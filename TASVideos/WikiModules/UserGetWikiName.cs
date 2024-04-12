using Microsoft.AspNetCore.Mvc.ViewComponents;
using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.UserGetWikiName)]
public class UserGetWikiName : WikiViewComponent
{
	public IViewComponentResult Invoke()
	{
		return new ContentViewComponentResult(UserClaimsPrincipal.Name());
	}
}
