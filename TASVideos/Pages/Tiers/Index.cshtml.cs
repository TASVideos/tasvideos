using System.Collections.Generic;
using System.Threading.Tasks;
using TASVideos.Core.Services;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Tiers
{
	[RequirePermission(PermissionTo.TierMaintenance)]
	public class IndexModel : BasePageModel
	{
		private readonly ITierService _tierService;

		public IndexModel(ITierService tierService)
		{
			_tierService = tierService;
		}

		public IEnumerable<Tier> Tiers { get; set; } = new List<Tier>();

		public async Task OnGet()
		{
			Tiers = await _tierService.GetAll();
		}
	}
}
