using System.ComponentModel.DataAnnotations;
using TASVideos.Data.Entity.Exhibition;
using TASVideos.Models;

namespace TASVideos.Pages.Exhibitions.Drafts.Models;

public class ExhibitionDraftCreateModel
{
	public string Title { get; set; } = "";
	public DateTime ExhibitionTimestamp { get; set; }
	public List<int> Games { get; set; } = [];
	public List<int> Contributors { get; set; } = [];

	public IFormFile? Screenshot { get; set; }
	public string? ScreenshotDescription { get; set; }
	public IFormFile? MovieFile { get; set; }
	public string? MovieFileDescription { get; set; }

	public List<ExhibitionDraftCreateUrlModel> StreamingUrls { get; set; } = [];

	[DoNotTrim]
	public string Markup { get; set; } = "";

	public class ExhibitionDraftCreateUrlModel
	{
		public ExhibitionUrlType Type { get; set; }
		public string DisplayName { get; set; } = "";
		public string Url { get; set; } = "";
	}
}
