using Microsoft.AspNetCore.Authorization;
using TASVideos.Core.Services;
namespace TASVideos.Pages.Systems;

[AllowAnonymous]
public class IndexModel : BasePageModel
{
	private readonly IGameSystemService _systemService;

	public IndexModel(IGameSystemService systemService)
	{
		_systemService = systemService;
	}

	public IEnumerable<SystemsResponse> Systems { get; set; } = new List<SystemsResponse>();

	public async Task OnGet()
	{
		Systems = (await _systemService.GetAll())
			.OrderBy(s => s.Id)
			.ToList();
	}
}
