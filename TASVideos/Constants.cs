using Microsoft.AspNetCore.Mvc.Rendering;

namespace TASVideos.RazorPages
{
	public static class UiDefaults
	{
		public const string DefaultDropdownText = "---";

		public static SelectListItem[] DefaultEntry =
		{
			new () { Text = DefaultDropdownText, Value = "" }
		};
	}
}
