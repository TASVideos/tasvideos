using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using TASVideos.Core.Services;
using TASVideos.WikiEngine;

namespace TASVideos.RazorPages.ViewComponents
{
	[WikiModule(WikiModules.Awards)]
	public class Awards : ViewComponent
	{
		private readonly IAwards _awards;

		public Awards(IAwards awards)
		{
			_awards = awards;
		}

		public async Task<IViewComponentResult> InvokeAsync(string? clear, int year)
		{
			if (!string.IsNullOrWhiteSpace(clear))
			{
				return new ContentViewComponentResult("Error: clear parameter no longer supported");
			}

			var model = await _awards.ForYear(year);

			return View(model);
		}
	}
}
