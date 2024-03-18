using TASVideos.Core.Services;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Flags;

[RequirePermission(PermissionTo.FlagMaintenance)]
public class IndexModel(IFlagService flagService) : BasePageModel
{
	public IEnumerable<Flag> Flags { get; set; } = new List<Flag>();

	public async Task OnGet()
	{
		Flags = (await flagService.GetAll())
			.OrderBy(t => t.Token)
			.ToList();
	}
}
