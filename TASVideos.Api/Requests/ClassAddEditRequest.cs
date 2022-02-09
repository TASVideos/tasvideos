using System.ComponentModel.DataAnnotations;

namespace TASVideos.Api.Requests;

/// <summary>
/// Represents a publication class to add or update
/// </summary>
public class ClassAddEditRequest
{
	/// <summary>
	/// Gets the name of the publication class, must be a unique value
	/// </summary>
	[Required]
	public string Name { get; init; } = "";

	/// <summary>
	/// Gets the weight multiplier to use for player point calculations (1 is no modification, less than 0 will reduce the overall player points, more than 1 will increase it)
	/// </summary>
	[Required]
	public double Weight { get; init; }

	/// <summary>
	/// Gets the relative path of the icon to use (must be an image resource on the server)
	/// </summary>
	[Required]
	[StringLength(100)]
	public string IconPath { get; init; } = "";

	/// <summary>
	/// Gets the wiki page to link to, that will document the publication class
	/// </summary>
	[Required]
	[StringLength(100)]
	public string Link { get; init; } = "";
}
