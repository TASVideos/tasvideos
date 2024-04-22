namespace TASVideos.Api.Requests;

internal class PublicationsRequest : ApiRequest, IPublicationTokens
{
	[SwaggerParameter("The system codes to filter by")]
	public string? Systems { get; set; }

	[SwaggerParameter("The publication class names to filter by")]
	public string? ClassNames { get; set; }

	[SwaggerParameter("The start year to filter by")]
	public int? StartYear { get; set; }

	[SwaggerParameter("The end year to filter by")]
	public int? EndYear { get; set; }

	[SwaggerParameter("The genre names to filter by")]
	public string? GenreNames { get; set; }

	[SwaggerParameter("The names of the publication tags to filter by")]
	public string? TagNames { get; set; }

	[SwaggerParameter("The names of the publication flags to filter by")]
	public string? FlagNames { get; set; }

	[SwaggerParameter("The ids of the authors to filter by")]
	public string? AuthorIds { get; set; }

	[SwaggerParameter("Indicates whether or not to return obsoleted publications")]
	public bool? ShowObsoleted { get; set; }

	[SwaggerParameter("Indicates whether or not to only return obsoleted publications")]
	public bool? OnlyObsoleted { get; set; }

	[SwaggerParameter("The ids of the games to filter by")]
	public string? GameIds { get; set; }

	[SwaggerParameter("The ids of the game groups to filter by")]
	public string? GameGroupIds { get; set; }

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
