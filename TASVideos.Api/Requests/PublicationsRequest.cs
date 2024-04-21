namespace TASVideos.Api.Requests;

public class PublicationsRequest : ApiRequest, IPublicationTokens
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

	public static new ValueTask<PublicationsRequest> BindAsync(HttpContext context)
	{
		return ValueTask.FromResult(new PublicationsRequest
		{
			Sort = context.Request.Query["Sort"],
			Fields = context.Request.Query["Fields"],
			PageSize = context.Request.GetInt(nameof(PageSize)) ?? 100,
			CurrentPage = context.Request.GetInt(nameof(CurrentPage)) ?? 1,
			StartYear = context.Request.GetInt(nameof(StartYear)),
			EndYear = context.Request.GetInt(nameof(EndYear)),
			Systems = context.Request.Query["Systems"],
			ClassNames = context.Request.Query["ClassNames"],
			GenreNames = context.Request.Query["GenreNames"],
			TagNames = context.Request.Query["TagNames"],
			FlagNames = context.Request.Query["FlagNames"],
			GameIds = context.Request.Query["GameIds"],
			GameGroupIds = context.Request.Query["GameGroupIds"],
			ShowObsoleted = context.Request.GetBool(nameof(ShowObsoleted)) ?? false,
			OnlyObsoleted = context.Request.GetBool(nameof(OnlyObsoleted)) ?? false
		});
	}
}
