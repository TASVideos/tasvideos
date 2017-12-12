namespace TASVideos.Data.Entity
{
	public class WikiPage : BaseEntity
	{
		public int Id { get; set; }
		public string PageName { get; set; }
		public string Markup { get; set; }
		public int Revision { get; set; } = 1;

		public bool MinorEdit { get; set; }
		public string RevisionMessage { get; set; }

		public virtual WikiPage Child { get; set; } // The latest revision of a page is one with Child = null
	}
}
