using TASVideos.Core.Services.Wiki;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.RssFeeds.Models;

public class RssPublication
{
	public IWikiPage Wiki { get; set; } = null!;

	public int Id { get; init; }
	public DateTime CreateTimestamp { get; init; }
	public string Title { get; init; } = "";

	public IEnumerable<string> TagNames { get; init; } = [];

	public int MovieFileSize { get; init; }
	public string ScreenshotPath => Files.First(f => f.Type == FileType.Screenshot).Path;

	public double RatingCount => Ratings.Count / 2.0;
	public double RatingMin => Ratings.Any() ? Ratings.Min() : 0;
	public double RatingMax => Ratings.Any() ? Ratings.Max() : 0;
	public double RatingAverage
	{
		get
		{
			if (Ratings.Any())
			{
				return Math.Round(Ratings.Average(), 2);
			}

			return 0;
		}
	}

	public IReadOnlyCollection<string> StreamingUrls { get; init; } = [];

	internal IReadOnlyCollection<File> Files { get; init; } = [];

	internal IReadOnlyCollection<double> Ratings { get; init; } = [];

	internal class File
	{
		public string Path { get; init; } = "";
		public FileType Type { get; init; }
	}
}
