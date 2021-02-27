using System.Linq;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.SeedData;
using TASVideos.WikiEngine;

namespace TASVideos.Legacy.Imports
{
	public static class WikiReferralGenerator
	{
		public static void Generate(ApplicationDbContext context)
		{
			// Don't generate referrals for these, since they will be wiped and replaced after import anyway
			var overrides = WikiPageSeedData.NewRevisions
				.Select(wp => wp.PageName)
				.ToList();

			var pagesForReferral = context.WikiPages
				.Where(p => p.ChildId == null)
				.ThatAreNotDeleted()
				.Where(wp => !overrides.Contains(wp.PageName))
				.ToList();

			// Referrals (only need latest revisions)
			var referralList = pagesForReferral
				.SelectMany(p => Util.GetReferrals(p.Markup).Select(referral => new WikiPageReferral
				{
					Referrer = p.PageName,
					Referral = referral.Link,
					Excerpt = referral.Excerpt
				}))
				.ToList();

			var referralColumns = new[]
			{
				nameof(WikiPageReferral.Excerpt),
				nameof(WikiPageReferral.Referral),
				nameof(WikiPageReferral.Referrer)
			};

			referralList.BulkInsert(referralColumns, nameof(ApplicationDbContext.WikiReferrals));
		}
	}
}
