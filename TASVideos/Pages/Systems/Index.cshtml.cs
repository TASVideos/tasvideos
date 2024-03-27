namespace TASVideos.Pages.Systems;

[AllowAnonymous]
public class IndexModel(IGameSystemService systemService) : BasePageModel
{
	public ICollection<SystemsResponse> Systems { get; set; } = [];

	public async Task OnGet()
	{
		Systems = await systemService.GetAll();
	}
}
