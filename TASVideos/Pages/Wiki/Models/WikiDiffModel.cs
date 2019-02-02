namespace TASVideos.Pages.Wiki.Models
{
	public class WikiDiffModel
	{
		public string PageName { get; set; }

		public int LeftRevision { get; set; }
		public string LeftMarkup { get; set; }

		public int RightRevision { get; set; }
		public string RightMarkup { get; set; }
	}
}
