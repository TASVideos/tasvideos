using System;

namespace TASVideos.ViewComponents
{
	public class WikiTextChangelogModel
	{
		public DateTime CreateTimestamp { get; set; }
		public string Author { get; set; }
		public string PageName { get; set; }
		public int Revision { get; set; }
		public bool MinorEdit { get; set; }
		public string RevisionMessage { get; set; }
	}
}
