using TASVideos.Data.Entity.Exhibition;

namespace TASVideos.Pages.Exhibitions.Drafts.Models;

public class ExhibitionDraftEditModel
{
	public string Title { get; set; } = "";
	public DateTime ExhibitionTimestamp { get; set; }
	public List<int> Games { get; set; } = [];
	public List<int> Contributors { get; set; } = [];

	public IFormFile? Screenshot { get; set; }
	public string? ScreenshotDescription { get; set; }
	public IFormFile? MovieFile { get; set; }
	public string? MovieFileDescription { get; set; }

	public ExhibitionDraftEditUrlModel UrlsDefault { get; set; } = new();
	public List<ExhibitionDraftEditUrlModel> Urls { get; set; } = [];

	[DoNotTrim]
	public string Markup { get; set; } = "";

	public class ExhibitionDraftEditUrlModel
	{
		public int UrlId { get; set; }
		public ExhibitionUrlType Type { get; set; }
		public string DisplayName { get; set; } = "";
		public string Url { get; set; } = "";
	}
}