using Microsoft.AspNetCore.Authorization;
using TASVideos.Core.Services;
using TASVideos.Data.Entity.Game;

namespace TASVideos.Pages.Systems;

[AllowAnonymous]
public class IndexModel : BasePageModel
{
	private readonly IGameSystemService _systemService;

	public IndexModel(IGameSystemService systemService)
	{
		_systemService = systemService;
	}

	public IEnumerable<GameSystem> Systems { get; set; } = new List<GameSystem>();

	public async Task OnGet()
	{
		Systems = (await _systemService.GetAll())
			.OrderBy(s => s.Id)
			.ToList();
	}
}
