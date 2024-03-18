namespace TASVideos.Core.Services;

public class MovieTokens : IPublicationTokens
{
	public IEnumerable<string> SystemCodes { get; init; } = [];
	public IEnumerable<string> Classes { get; init; } = [];
	public IEnumerable<int> Years { get; init; } = Enumerable.Range(2000, DateTime.UtcNow.AddYears(1).Year - 2000 + 1).ToList();
	public IEnumerable<string> Tags { get; init; } = [];
	public IEnumerable<string> Genres { get; init; } = [];
	public IEnumerable<string> Flags { get; init; } = [];

	public bool ShowObsoleted { get; set; }
	public bool OnlyObsoleted { get; set; }
	public string SortBy { get; set; } = "";
	public int? Limit { get; set; }

	public IEnumerable<int> Authors { get; init; } = [];

	public IEnumerable<int> MovieIds { get; init; } = [];

	public IEnumerable<int> Games { get; init; } = [];
	public IEnumerable<int> GameGroups { get; init; } = [];
}
