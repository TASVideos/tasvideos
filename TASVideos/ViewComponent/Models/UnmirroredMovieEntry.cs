using System.Collections.Generic;

namespace TASVideos.RazorPages.ViewComponents
{
	public class UnmirroredMovieEntry
	{
		public int Id { get; init; }
		public string Title { get; init; } = "";
		public IEnumerable<string> EncodePaths { get; init; } = new List<string>();
	}
}
