using Microsoft.AspNetCore.Mvc.Rendering;

namespace TASVideos
{
	public static class UiDefaults
	{
		// ReSharper disable once StyleCop.SA1401
		public static SelectListItem[] DefaultEntry =
		{
			new SelectListItem { Text = "---", Value = "" }
		};
	}
}
