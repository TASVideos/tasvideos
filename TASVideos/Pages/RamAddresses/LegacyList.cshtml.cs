using Microsoft.AspNetCore.Mvc;

namespace TASVideos.Pages.RamAddresses;

public class LegacyListModel : BasePageModel
{
	public IActionResult OnGet()
	{
		return BasePageRedirect("/RamAddresses/List");
	}
}
