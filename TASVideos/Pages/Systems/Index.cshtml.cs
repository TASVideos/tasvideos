using Microsoft.AspNetCore.Authorization;
using TASVideos.Core.Services;
namespace TASVideos.Pages.Systems;

[AllowAnonymous]
public class IndexModel(IGameSystemService systemService) : BasePageModel
{
	public IEnumerable<SystemsResponse> Systems { get; set; } = new List<SystemsResponse>();

	public async Task OnGet()
	{
		Systems = (await systemService.GetAll())
			.OrderBy(s => s.DisplayName)
			.ToList();
	}
}
