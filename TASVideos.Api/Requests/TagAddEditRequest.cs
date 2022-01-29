namespace TASVideos.Api.Requests;

/// <summary>
/// Represents a tag to add or update.
/// </summary>
public class TagAddEditRequest
{
	/// <summary>
	/// Gets the tag code. This value must be unique.
	/// </summary>
	public string Code { get; init; } = "";

	/// <summary>
	/// Gets the display name of the tag.
	/// </summary>
	public string DisplayName { get; init; } = "";
}
