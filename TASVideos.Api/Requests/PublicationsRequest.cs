namespace TASVideos.Api.Requests;

internal class PublicationsRequest : ApiRequest, IPublicationTokens
{
	[SwaggerParameter("The system codes to filter by")]
	public string? Systems { get; init; }

	[SwaggerParameter("The publication class names to filter by")]
	public string? ClassNames { get; init; }

	[SwaggerParameter("The start year to filter by")]
	public int? StartYear { get; init; }

	[SwaggerParameter("The end year to filter by")]
	public int? EndYear { get; init; }

	[SwaggerParameter("The genre names to filter by")]
	public string? GenreNames { get; init; }

	[SwaggerParameter("The names of the publication tags to filter by")]
	public string? TagNames { get; init; }

	[SwaggerParameter("The names of the publication flags to filter by")]
	public string? FlagNames { get; init; }

	[SwaggerParameter("The ids of the authors to filter by")]
	public string? AuthorIds { get; init; }

	[SwaggerParameter("Indicates whether or not to return obsoleted publications")]
	public bool? ShowObsoleted { get; init; }

	[SwaggerParameter("Indicates whether or not to only return obsoleted publications")]
	public bool? OnlyObsoleted { get; init; }

	[SwaggerParameter("The ids of the games to filter by")]
	public string? GameIds { get; init; }

	[SwaggerParameter("The ids of the game groups to filter by")]
	public string? GameGroupIds { get; init; }

	bool IPublicationTokens.ShowObsoleted => ShowObsoleted ?? false;
	bool IPublicationTokens.OnlyObsoleted => OnlyObsoleted ?? false;

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
