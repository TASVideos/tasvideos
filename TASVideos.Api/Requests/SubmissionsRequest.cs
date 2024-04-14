using System.Reflection;
using Microsoft.AspNetCore.Http;

namespace TASVideos.Api.Requests;

public class SubmissionsRequest : ApiRequest, ISubmissionFilter
{
	public string? Statuses { get; set; }
	public string? User { get; set; }
	public int? StartYear { get; set; }
	public int? EndYear { get; set; }
	public string? Systems { get; set; }
	public string? Games { get; set; }
	public int? StartType { get; set; }

	ICollection<int> ISubmissionFilter.Years => StartYear.YearRange(EndYear).ToList();

	ICollection<SubmissionStatus> ISubmissionFilter.StatusFilter => !string.IsNullOrWhiteSpace(Statuses)
		? Statuses
			.SplitWithEmpty(",")
			.Where(s => Enum.TryParse(s, out SubmissionStatus _))
			.Select(s =>
			{
				Enum.TryParse(s, out SubmissionStatus x);
				return x;
			})
			.ToList()
		: [];

	ICollection<string> ISubmissionFilter.Systems => Systems.CsvToStrings();
	ICollection<int> ISubmissionFilter.GameIds => Games.CsvToInts();

	public static new async ValueTask<SubmissionsRequest> BindAsync(HttpContext context, ParameterInfo parameter)
	{
		var baseResult = await ApiRequest.BindAsync(context, parameter);

		// TODO: ughhhhhhhhhhhhhhhhhhhhhhhhh
		var result = new SubmissionsRequest
		{
			PageSize = baseResult.PageSize,
			CurrentPage = baseResult.CurrentPage,
			Sort = baseResult.Sort,
			Fields = baseResult.Fields
		};

		if (int.TryParse(context.Request.Query["StartYear"], out var startYear))
		{
			result.StartYear = startYear;
		}

		if (int.TryParse(context.Request.Query["EndYear"], out var endYear))
		{
			result.EndYear = endYear;
		}

		if (int.TryParse(context.Request.Query["StartType"], out var startType))
		{
			result.StartType = startType;
		}

		result.Statuses = context.Request.Query["Statuses"];
		result.User = context.Request.Query["User"];
		result.Systems = context.Request.Query["Systems"];
		result.Games = context.Request.Query["Games"];

		return result;
	}
}
