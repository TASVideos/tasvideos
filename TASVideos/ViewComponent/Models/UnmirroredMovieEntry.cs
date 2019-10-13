using System.Collections.Generic;

namespace TASVideos.ViewComponents
{
	public class UnmirroredMovieEntry
	{
		public int Id { get; set; }
		public string Title { get; set; } = "";
		public IEnumerable<string> EncodePaths { get; set; } = new List<string>();
	}
}
