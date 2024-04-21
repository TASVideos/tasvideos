namespace TASVideos.Api.Requests;

internal class GamesRequest : ApiRequest
{
	[Description("The system codes to filter by")]
	public string? Systems { get; init; }

	internal IEnumerable<string> SystemCodes => Systems.CsvToStrings();
}
