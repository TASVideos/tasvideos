namespace TASVideos.Core.Services;

public class MovieTokens : IPublicationTokens
{
	public ICollection<string> SystemCodes { get; init; } = [];
	public ICollection<string> Classes { get; init; } = [];
	public ICollection<int> Years { get; init; } = Enumerable.Range(2000, DateTime.UtcNow.AddYears(1).Year - 2000 + 1).ToList();
	public ICollection<string> Tags { get; init; } = [];
	public ICollection<string> Genres { get; init; } = [];
	public ICollection<string> Flags { get; init; } = [];

	public bool ShowObsoleted { get; set; }
	public bool OnlyObsoleted { get; set; }
	public string SortBy { get; set; } = "";
	public int? Limit { get; set; }

	public ICollection<int> Authors { get; init; } = [];

	public ICollection<int> MovieIds { get; init; } = [];

	public ICollection<int> Games { get; init; } = [];
	public ICollection<int> GameGroups { get; init; } = [];
}
