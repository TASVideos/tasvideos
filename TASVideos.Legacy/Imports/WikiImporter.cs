using System;
using System.Collections.Generic;
using System.Linq;
using TASVideos.Data;
using TASVideos.Data.Constants;
using TASVideos.Data.Entity;
using TASVideos.Data.Helpers;
using TASVideos.Legacy.Data;
using TASVideos.Legacy.Data.Entity;
using TASVideos.WikiEngine;

namespace TASVideos.Legacy.Imports
{
	public static class WikiImporter
	{
		public static void ImportWikiPages(ApplicationDbContext context, NesVideosSiteContext legacyContext)
		{
			// TODO: page to keep ram down
			// TODO: Deleted pages
			// TODO: createdby username (look up by userid)
			// TODO: homepages

			List<SiteText> siteTexts = legacyContext.SiteText
				.OrderBy(s => s.Id)
				.ToList();

			foreach (var legacyPage in siteTexts)
			{
				string markup = legacyPage.Description;

				// Shenanigans
				if (legacyPage.PageName == "Phil" && legacyPage.Revision >= 7 && legacyPage.Revision <= 11)
				{
					markup = markup.Replace(":[", ":|");
				}

				var pubId = SubmissionHelper.IsPublicationLink(legacyPage.PageName);
				var subId = SubmissionHelper.IsSubmissionLink(legacyPage.PageName);

				string pageName = legacyPage.PageName;
				if (pubId.HasValue)
				{
					pageName = LinkConstants.PublicationWikiPage + pubId.Value;
				}
				else if (subId.HasValue)
				{
					pageName = LinkConstants.SubmissionWikiPage + subId.Value;
				}

				var wikiPage = new WikiPage
				{
					PageName = pageName,
					Markup = markup,
					Revision = legacyPage.Revision,
					MinorEdit = legacyPage.MinorEdit == "Y",
					RevisionMessage = legacyPage.WhyEdit,
					IsDeleted = false, // TODO
					CreateTimeStamp = ImportHelpers.UnixTimeStampToDateTime(legacyPage.CreateTimeStamp)
				};

				context.WikiPages.Add(wikiPage);

				var referrals = Util.GetAllWikiLinks(wikiPage.Markup);
				foreach (var referral in referrals)
				{
					context.WikiReferrals.Add(new WikiPageReferral
					{
						Referrer = wikiPage.PageName,
						Referral = referral.Link?.Split('|').FirstOrDefault(),
						Excerpt = referral.Excerpt
					});
				}
			}

			context.SaveChanges();

			// Set child references
			foreach (var wikiPage in context.WikiPages)
			{
				var nextWiki = context.WikiPages
					.SingleOrDefault(wp => wp.Revision == wikiPage.Revision + 1
						&& wp.PageName == wikiPage.PageName);

				if (nextWiki != null)
				{
					wikiPage.Child = nextWiki;
				}
			}

			context.SaveChanges();
		}
	}
}
