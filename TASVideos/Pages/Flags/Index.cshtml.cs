using Microsoft.AspNetCore.Mvc.RazorPages;
using TASVideos.Core.Services;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Flags;

[RequirePermission(PermissionTo.FlagMaintenance)]
public class IndexModel : PageModel
{
	private readonly IFlagService _flagService;

	public IndexModel(IFlagService flagService)
	{
		_flagService = flagService;
	}

	public IEnumerable<Flag> Flags { get; set; } = new List<Flag>();

	public async Task OnGet()
	{
		Flags = (await _flagService.GetAll())
			.OrderBy(t => t.Token)
			.ToList();
	}
}
