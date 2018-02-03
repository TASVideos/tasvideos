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
					Markup = legacyPage.Description,
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
			// TODO: iterate through all wikipages, find max revision and set child references
		}
	}
}
