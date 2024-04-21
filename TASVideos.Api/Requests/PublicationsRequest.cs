namespace TASVideos.Api.Requests;

internal class PublicationsRequest : ApiRequest, IPublicationTokens
{
	public string? Systems { get; set; }
	public string? ClassNames { get; set; }
	public int? StartYear { get; set; }
	public int? EndYear { get; set; }
	public string? GenreNames { get; set; }
	public string? TagNames { get; set; }
	public string? FlagNames { get; set; }
	public string? AuthorIds { get; set; }
	public bool ShowObsoleted { get; set; }
	public bool OnlyObsoleted { get; set; }
	public string? GameIds { get; set; }
	public string? GameGroupIds { get; set; }

	ICollection<string> IPublicationTokens.SystemCodes => Systems.CsvToStrings();
	ICollection<string> IPublicationTokens.Classes => ClassNames.CsvToStrings();
	ICollection<int> IPublicationTokens.Years => StartYear.YearRange(EndYear).ToList();
	ICollection<string> IPublicationTokens.Genres => GenreNames.CsvToStrings();
	ICollection<string> IPublicationTokens.Tags => TagNames.CsvToStrings();
	ICollection<string> IPublicationTokens.Flags => FlagNames.CsvToStrings();
	ICollection<int> IPublicationTokens.Authors => AuthorIds.CsvToInts();
	ICollection<int> IPublicationTokens.MovieIds => [];
	ICollection<int> IPublicationTokens.Games => GameIds.CsvToInts();
	ICollection<int> IPublicationTokens.GameGroups => GameGroupIds.CsvToInts();
	string IPublicationTokens.SortBy => "";
	int? IPublicationTokens.Limit => null;
}
