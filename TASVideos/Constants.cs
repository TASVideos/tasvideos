using Microsoft.AspNetCore.Mvc.Rendering;

namespace TASVideos.Constants
{
    public static class LinkConstants
	{
		public const string SubmissionWikiPage = "System/SubmissionContent/S";
	}

	public static class UiDefaults
	{
		public static SelectListItem[] DefaultEntry =
		{
			new SelectListItem { Text = "---", Value = "" }
		};
	}
}
