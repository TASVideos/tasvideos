namespace TASVideos.Api.Requests;

internal class GamesRequest : ApiRequest
{
	public string? Systems { get; init; }

	internal IEnumerable<string> SystemCodes => Systems.CsvToStrings();
}
