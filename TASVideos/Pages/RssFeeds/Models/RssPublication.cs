using TASVideos.Data.Entity;

namespace TASVideos.Pages.RssFeeds.Models;

public class RssPublication
{
	public WikiPage Wiki { get; init; } = new();

	public int Id { get; init; }
	public DateTime CreateTimestamp { get; init; }
	public string Title { get; init; } = "";

	public IEnumerable<string> TagNames { get; init; } = new List<string>();

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

	public ICollection<string> StreamingUrls { get; init; } = new List<string>();

	internal ICollection<File> Files { get; init; } = new List<File>();

	internal ICollection<double> Ratings { get; init; } = new List<double>();

	internal class File
	{
		public string Path { get; init; } = "";
		public FileType Type { get; init; }
	}
}
