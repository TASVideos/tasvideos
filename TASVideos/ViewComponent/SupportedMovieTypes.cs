using Microsoft.AspNetCore.Mvc;
using TASVideos.MovieParsers;
using TASVideos.WikiEngine;

namespace TASVideos.RazorPages.ViewComponents
{
	[WikiModule(WikiModules.SupportedMovieTypes)]
	public class SupportedMovieTypes : ViewComponent
	{
		private readonly IMovieParser _movieParser;

		public SupportedMovieTypes(IMovieParser movieParser)
		{
			_movieParser = movieParser;
		}

		public IViewComponentResult Invoke()
		{
			return View(_movieParser.SupportedMovieExtensions);
		}
	}
}
