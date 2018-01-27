using Microsoft.AspNetCore.Mvc.Rendering;

namespace TASVideos.Constants
{
    public static class LinkConstants
	{
		public const string SubmissionWikiPage = "InternalSystem/SubmissionContent/S";
		public const string PublicationWikiPage = "InternalSystem/PublicationContent/M";
	}

	public static class UiDefaults
	{
		// ReSharper disable once StyleCop.SA1401
		public static SelectListItem[] DefaultEntry =
		{
			new SelectListItem { Text = "---", Value = "" }
		};
	}
}
