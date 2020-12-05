using Microsoft.AspNetCore.Mvc;
using TASVideos.MovieParsers;

namespace TASVideos.ViewComponents
{
	public class SupportedMovieTypes : ViewComponent
	{
		private readonly MovieParser _movieParser;

		public SupportedMovieTypes(MovieParser movieParser)
		{
			_movieParser = movieParser;
		}

		public IViewComponentResult Invoke()
		{
			return View(_movieParser.SupportedMovieExtensions);
		}
	}
}
