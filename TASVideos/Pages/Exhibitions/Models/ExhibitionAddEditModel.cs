using System.ComponentModel;
using TASVideos.Data.Entity.Exhibition;

namespace TASVideos.Pages.Exhibitions.Models;

public class ExhibitionAddEditModel
{
	public int ExhibitionId { get; set; }
	public string Title { get; set; } = "";
	[DisplayName("Exhibition Timestamp (UTC)")]
	public DateTime ExhibitionTimestamp { get; set; } = DateTime.UtcNow;
	public List<int> Games { get; set; } = [];
	public List<int> Contributors { get; set; } = [];

	[DisplayName("Screenshot File")]
	public IFormFile? Screenshot { get; set; }
	[DisplayName("Screenshot Description")]
	public string? ScreenshotDescription { get; set; }

	public ExhibitionAddEditMovieModel MoviesDefault { get; set; } = new();
	[DisplayName("Movie Files")]
	public List<ExhibitionAddEditMovieModel> Movies { get; set; } = [];

	public ExhibitionAddEditUrlModel UrlsDefault { get; set; } = new();
	[DisplayName("URLs")]
	public List<ExhibitionAddEditUrlModel> Urls { get; set; } = [];

	[DisplayName("Exhibition Description")]
	[DoNotTrim]
	public string Markup { get; set; } = "";

	[StringLength(1000)]
	[Display(Name = "Revision Message")]
	public string? RevisionMessage { get; set; } = "";

	public bool MinorEdit { get; set; }

	public class ExhibitionAddEditUrlModel
	{
		public int UrlId { get; set; }
		public ExhibitionUrlType Type { get; set; }
		[DisplayName("Display Name")]
		public string DisplayName { get; set; } = "";
		[DisplayName("URL")]
		public string Url { get; set; } = "";
	}

	public class ExhibitionAddEditMovieModel
	{
		public int FileId { get; set; }
		[DisplayName("Movie File")]
		public IFormFile? MovieFile { get; set; }
		[DisplayName("Description")]
		public string? MovieFileDescription { get; set; }
	}
}
