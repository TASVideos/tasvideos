namespace TASVideos.Api.Requests;

internal class GamesRequest : ApiRequest
{
	public string? Systems { get; init; }

	internal IEnumerable<string> SystemCodes => Systems.CsvToStrings();

	public static new ValueTask<GamesRequest> BindAsync(HttpContext context)
	{
		return ValueTask.FromResult(new GamesRequest
		{
			Sort = context.Request.Query["Sort"],
			Fields = context.Request.Query["Fields"],
			PageSize = context.Request.GetInt(nameof(PageSize)) ?? 100,
			CurrentPage = context.Request.GetInt(nameof(CurrentPage)) ?? 1,
			Systems = context.Request.Query["Systems"]
		});
	}
}
