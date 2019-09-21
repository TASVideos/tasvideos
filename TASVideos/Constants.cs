using Microsoft.AspNetCore.Mvc.Rendering;

namespace TASVideos
{
	public static class UiDefaults
	{
		public const string DefaultDropdownText = "---";

		// ReSharper disable once StyleCop.SA1401
		public static SelectListItem[] DefaultEntry =
		{
			new SelectListItem { Text = DefaultDropdownText, Value = "" }
		};
	}
}
