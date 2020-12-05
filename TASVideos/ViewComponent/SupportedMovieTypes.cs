using Microsoft.AspNetCore.Mvc;
using TASVideos.MovieParsers;

namespace TASVideos.ViewComponents
{
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
