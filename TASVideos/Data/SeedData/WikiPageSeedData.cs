using TASVideos.Data.Entity;

namespace TASVideos.Data.SeedData
{
    public class WikiPageSeedData
	{
		public const string PageNotFound = "System/PageNotFound";
		public static readonly WikiPage[] SeedPages =
		{
			new WikiPage
			{
				Markup = @"
!!! This page does not yet exist.
%%%
Want to create it?

TODO: finish this page when all the markup is supported
",
				PageName = PageNotFound,
				MinorEdit = false,
				RevisionMessage = "Initial Creation"
			}
		};
	}
}
