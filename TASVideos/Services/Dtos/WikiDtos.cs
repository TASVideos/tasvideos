namespace TASVideos.Services.Dtos
{
	public class WikiCreateDto
	{
		public string PageName { get; set; }
		public string Markup { get; set; }
		public bool MinorEdit { get; set; }
		public string RevisionMessage { get; set; }
	}
}
