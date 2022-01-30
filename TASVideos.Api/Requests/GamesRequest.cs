using System.ComponentModel.DataAnnotations;

namespace TASVideos.Api.Requests;

/// <summary>
/// Represents the filtering criteria for the games endpoint.
/// </summary>
public class GamesRequest : ApiRequest
{
	/// <summary>
	/// Gets the system codes to filter by.
	/// </summary>
	[StringLength(200)]
	public string? Systems { get; init; }

	internal IEnumerable<string> SystemCodes => Systems.CsvToStrings();
}
