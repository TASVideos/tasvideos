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
	public double RatingMin => Ratings.Any() ? Ratings.Min(r => r.Value) : 0;
	public double RatingMax => Ratings.Any() ? Ratings.Max(r => r.Value) : 0;
	public double RatingAverage
	{
		get
		{
			var ent = Ratings
				.Where(r => r.Type == PublicationRatingType.Entertainment)
				.Select(r => r.Value)
				.ToList();

			var tech = Ratings
				.Where(r => r.Type == PublicationRatingType.TechQuality)
				.Select(r => r.Value);

			var all = ent.Concat(ent).Concat(tech).ToList();
			if (all.Any())
			{
				return Math.Round(all.Average(), 2);
			}

			return 0;
		}
	}

	public ICollection<string> StreamingUrls { get; init; } = new List<string>();

	internal ICollection<File> Files { get; init; } = new List<File>();

	internal ICollection<Rating> Ratings { get; init; } = new List<Rating>();

	internal class File
	{
		public string Path { get; init; } = "";
		public FileType Type { get; init; }
	}

	internal class Rating
	{
		public double Value { get; init; }
		public PublicationRatingType Type { get; init; }
	}
}
