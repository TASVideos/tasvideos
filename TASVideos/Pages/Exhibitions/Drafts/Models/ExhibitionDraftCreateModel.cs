using TASVideos.Data.Entity.Exhibition;

namespace TASVideos.Pages.Exhibitions.Drafts.Models;

public class ExhibitionDraftCreateModel
{
	public string Title { get; set; } = "";
	public DateTime ExhibitionTimestamp { get; set; } = DateTime.UtcNow;
	public List<int> Games { get; set; } = [];
	public List<int> Contributors { get; set; } = [];

	public IFormFile? Screenshot { get; set; }
	public string? ScreenshotDescription { get; set; }
	public IFormFile? MovieFile { get; set; }
	public string? MovieFileDescription { get; set; }

	public List<ExhibitionDraftCreateUrlModel> Urls { get; set; } = [new()];

	[DoNotTrim]
	public string Markup { get; set; } = "";

	[MustBeTrue]
	public bool AgreeToLicense { get; set; }

	public class ExhibitionDraftCreateUrlModel
	{
		public ExhibitionUrlType Type { get; set; }
		public string DisplayName { get; set; } = "";
		public string Url { get; set; } = "";
	}
}
