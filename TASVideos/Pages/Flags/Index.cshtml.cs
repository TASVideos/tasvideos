using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TASVideos.Data.Entity;
using TASVideos.Services;

namespace TASVideos.Pages.Flags
{
	[RequirePermission(PermissionTo.FlagMaintenance)]
	public class IndexModel : PageModel
	{
		private readonly IFlagService _flagService;

		public IndexModel(IFlagService flagService)
		{
			_flagService = flagService;
		}

		[TempData]
		public string? Message { get; set; }

		[TempData]
		public string? MessageType { get; set; }

		public bool ShowMessage => !string.IsNullOrWhiteSpace(Message);

		public IEnumerable<Flag> Flags { get; set; } = new List<Flag>();

		public async Task OnGet()
		{
			Flags = (await _flagService.GetAll())
				.OrderBy(t => t.Token)
				.ToList();
		}
	}
}
