namespace TASVideos.Pages.Flags;

[RequirePermission(PermissionTo.FlagMaintenance)]
public class IndexModel(IFlagService flagService) : BasePageModel
{
	public ICollection<Flag> Flags { get; set; } = [];

	public async Task OnGet()
	{
		Flags = await flagService.GetAll();
	}
}
