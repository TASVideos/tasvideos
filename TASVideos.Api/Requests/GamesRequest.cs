using System.Reflection;
using Microsoft.AspNetCore.Http;

namespace TASVideos.Api.Requests;

public class GamesRequest : ApiRequest
{
	public string? Systems { get; init; }

	internal IEnumerable<string> SystemCodes => Systems.CsvToStrings();

	public static new async ValueTask<GamesRequest> BindAsync(HttpContext context, ParameterInfo parameter)
	{
		var baseResult = await ApiRequest.BindAsync(context, parameter);

		// TODO: ughhhhhhhhhhhhhhhhhhhhhhhhh
		return new GamesRequest
		{
			PageSize = baseResult.PageSize,
			CurrentPage = baseResult.CurrentPage,
			Sort = baseResult.Sort,
			Fields = baseResult.Fields,
			Systems = context.Request.Query["Systems"]
		};
	}
}
