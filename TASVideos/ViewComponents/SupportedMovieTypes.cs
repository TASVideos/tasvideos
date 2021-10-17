using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Core.Services;
using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents
{
	[WikiModule(WikiModules.SupportedMovieTypes)]
	public class SupportedMovieTypes : ViewComponent
	{
		private readonly IMovieFormatDepcrecator _depcrecator;

		public SupportedMovieTypes(IMovieFormatDepcrecator depcrecator)
		{
			_depcrecator = depcrecator;
		}

		public async Task<IViewComponentResult> InvokeAsync()
		{
			return View(await _depcrecator.GetAll());
		}
	}
}
