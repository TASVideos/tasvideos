namespace TASVideos;

public static class UiDefaults
{
	public const string DefaultDropdownText = "---";

	public static readonly SelectListItem[] DefaultEntry =
	[
		new() { Text = DefaultDropdownText, Value = "" }
	];

	public static readonly SelectListItem[] AnyEntry = [new() { Text = "Any", Value = "" }];

	public static readonly SelectListItem[] CustomEntry = [new() { Text = "Custom", Value = "Custom" }];
}
