using System.Reflection;
using Microsoft.AspNetCore.Http;

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

	public static async ValueTask<PublicationsRequest> BindAsync(HttpContext context, ParameterInfo parameter)
	{
		var baseResult = await ApiRequest.BindAsync(context, parameter);

		// TODO: ughhhhhhhhhhhhhhhhhhhhhhhhh
		var result = new PublicationsRequest
		{
			PageSize = baseResult.PageSize,
			CurrentPage = baseResult.CurrentPage,
			Sort = baseResult.Sort,
			Fields = baseResult.Fields
		};

		result.Systems = context.Request.Query["Systems"];
		result.ClassNames = context.Request.Query["ClassNames"];
		if (int.TryParse(context.Request.Query["StartYear"], out var startYear))
		{
			result.StartYear = startYear;
		}

		if (int.TryParse(context.Request.Query["EndYear"], out var endYear))
		{
			result.EndYear = endYear;
		}

		result.GenreNames = context.Request.Query["GenreNames"];
		result.TagNames = context.Request.Query["TagNames"];
		result.FlagNames = context.Request.Query["FlagNames"];
		result.FlagNames = context.Request.Query["FlagNames"];

		if (bool.TryParse(context.Request.Query["ShowObsoleted"], out var showObsoleted))
		{
			result.ShowObsoleted = showObsoleted;
		}

		if (bool.TryParse(context.Request.Query["OnlyObsoleted"], out var onlyObsoleted))
		{
			result.OnlyObsoleted = onlyObsoleted;
		}

		result.GameIds = context.Request.Query["GameIds"];
		result.GameGroupIds = context.Request.Query["GameGroupIds"];

		return result;
	}
}
