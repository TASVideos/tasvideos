using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Constants;
using TASVideos.Data.Entity;
using TASVideos.Data.Helpers;
using TASVideos.Legacy.Data.Site;
using TASVideos.WikiEngine;

namespace TASVideos.Legacy.Imports
{
	public static class WikiImporter
	{
		public static void Import(ApplicationDbContext context, NesVideosSiteContext legacySiteContext)
		{
			var siteTexts = legacySiteContext.SiteText
				.Include(s => s.User)
				.Where(s => s.PageName != "DeletedPages/Bizhawk/ReleaseHistory") // Not worth preserving history here, revisions were mistakes and revision history is too large
				.ToList();

			var usernames = context.Users.Select(u => u.UserName).ToList();

			var pages = new List<WikiPage>();

			var siteTextWithUser = (from s in siteTexts
					join u in usernames on s.PageName equals u into uu
					from u in uu.DefaultIfEmpty()
					select new { Site = s, User = u })
					.ToList();

			foreach (var legacyPage in siteTextWithUser)
			{
				string markup = ImportHelper.FixString(legacyPage.Site.Description);
				int revision = legacyPage.Site.Revision;
				string pageName = legacyPage.Site.PageName;

				if (legacyPage.Site.PageName.StartsWith("System"))
				{
					pageName = legacyPage.Site.PageName.Replace("System", "System/");
				}
				else if (legacyPage.Site.PageName == "FrontPage")
				{
					pageName = "System/FrontPage";
					markup = markup.Replace("[module:welcome]", "");
				}

				// ******** Deleted pages that were recreated *************/
				else if (legacyPage.Site.PageName == "GameResources/N64/Kirby64TheCrystalShards"
					|| legacyPage.Site.PageName == "DeletedPages/GameResources/N64/Kirby64TheCrystalShards")
				{
					revision = CrystalShardsLookup[(legacyPage.Site.PageName, legacyPage.Site.Revision)];
				}

				// This page had 2 deleted pages that came first, so we can just add to the revision number
				else if (legacyPage.Site.PageName == "GameResources/DS/MetroidPrimeHunters")
				{
					revision += 2;
				}

				// ******** END Deleted pages that were recreated *************/

				// Shenanigans
				else if (legacyPage.Site.PageName == "Phil" && legacyPage.Site.Revision >= 7 && legacyPage.Site.Revision <= 11)
				{
					markup = markup.Replace(":[", ":|");
				}
				else if (legacyPage.Site.PageName == "971S" && legacyPage.Site.Revision == 3)
				{
					markup = markup.Replace("[Phi:", "[Phil]:");
				}
				else if (legacyPage.Site.PageName == "2884M")
				{
					markup = markup.Replace("][", "II");
				}

				var pubId = SubmissionHelper.IsPublicationLink(legacyPage.Site.PageName);
				var subId = SubmissionHelper.IsSubmissionLink(legacyPage.Site.PageName);
				var gamId = SubmissionHelper.IsGamePageLink(legacyPage.Site.PageName);

				bool isDeleted = false;
				if (pubId.HasValue)
				{
					pageName = LinkConstants.PublicationWikiPage + pubId.Value;
				}
				else if (subId.HasValue)
				{
					pageName = LinkConstants.SubmissionWikiPage + subId.Value;
				}
				else if (gamId.HasValue)
				{
					pageName = LinkConstants.GameWikiPage + gamId.Value;
				}
				else if (pageName.StartsWith("DeletedPages/"))
				{
					pageName = pageName.Replace("DeletedPages/", "");
					isDeleted = true;
				}

				if (legacyPage.User != null)
				{
					pageName = "HomePages/" + pageName;
				}

				markup = markup.Replace("=css/vaulttier.png", "=images/vaulttier.png");
				markup = markup.Replace("=css/moontier.png", "=images/moontier.png");
				markup = markup.Replace("=css/favourite.png", "=images/startier.png");
				markup = markup.Replace("=/css/vaulttier.png", "=images/vaulttier.png");
				markup = markup.Replace("=/css/moontier.png", "=images/moontier.png");
				markup = markup.Replace("=/css/favourite.png", "=images/startier.png");

				pages.Add(new WikiPage
				{
					Id = legacyPage.Site.Id,
					PageName = pageName,
					Markup = markup,
					Revision = revision,
					MinorEdit = legacyPage.Site.MinorEdit == "Y",
					RevisionMessage = legacyPage.Site.WhyEdit,
					IsDeleted = isDeleted,
					CreateTimeStamp = ImportHelper.UnixTimeStampToDateTime(legacyPage.Site.CreateTimeStamp),
					CreateUserName = legacyPage.Site.User.Name,
					LastUpdateTimeStamp = ImportHelper.UnixTimeStampToDateTime(legacyPage.Site.CreateTimeStamp)
				});
			}

			var dic = pages.ToDictionary(
				tkey => $"{tkey.PageName}__ImportKey__{tkey.Revision}",
				tvalue => tvalue);

			// Set child references
			foreach (var wikiPage in pages)
			{
				var result = dic.TryGetValue($"{wikiPage.PageName}__ImportKey__{wikiPage.Revision + 1}", out WikiPage nextWiki);

				if (result)
				{
					wikiPage.ChildId = nextWiki.Id;
				}
			}

			// Referrals (only need latest revisions)
			var referralList = pages
				.Where(p => p.ChildId == null)
				.SelectMany(p => Util.GetAllWikiLinks(p.Markup).Select(referral => new WikiPageReferral
				{
					Referrer = p.PageName,
					Referral = referral.Link?.Split('|').FirstOrDefault(),
					Excerpt = referral.Excerpt
				}))
				.ToList();

			var wikiColumns = new[]
			{
				nameof(WikiPage.Id),
				nameof(WikiPage.ChildId),
				nameof(WikiPage.CreateTimeStamp),
				nameof(WikiPage.CreateUserName),
				nameof(WikiPage.IsDeleted),
				nameof(WikiPage.LastUpdateTimeStamp),
				nameof(WikiPage.LastUpdateUserName),
				nameof(WikiPage.Markup),
				nameof(WikiPage.MinorEdit),
				nameof(WikiPage.PageName),
				nameof(WikiPage.Revision),
				nameof(WikiPage.RevisionMessage)
			};

			pages.BulkInsert(context, wikiColumns, nameof(ApplicationDbContext.WikiPages));

			var referralColumns = new[]
			{
				nameof(WikiPageReferral.Excerpt),
				nameof(WikiPageReferral.Referral),
				nameof(WikiPageReferral.Referrer)
			};

			referralList.BulkInsert(context, referralColumns, nameof(ApplicationDbContext.WikiReferrals), SqlBulkCopyOptions.Default, 100000);
		}

		private static readonly Dictionary<(string, int), int> CrystalShardsLookup = new Dictionary<(string, int), int>
		{
			[("GameResources/N64/Kirby64TheCrystalShards", 1)] = 1,
			[("GameResources/N64/Kirby64TheCrystalShards", 2)] = 2,
			[("DeletedPages/GameResources/N64/Kirby64TheCrystalShards", 1)] = 3,
			[("DeletedPages/GameResources/N64/Kirby64TheCrystalShards", 2)] = 4,
			[("DeletedPages/GameResources/N64/Kirby64TheCrystalShards", 3)] = 5,
			[("DeletedPages/GameResources/N64/Kirby64TheCrystalShards", 4)] = 6,
			[("DeletedPages/GameResources/N64/Kirby64TheCrystalShards", 5)] = 7,
			[("DeletedPages/GameResources/N64/Kirby64TheCrystalShards", 6)] = 8,
			[("DeletedPages/GameResources/N64/Kirby64TheCrystalShards", 7)] = 9,
			[("DeletedPages/GameResources/N64/Kirby64TheCrystalShards", 8)] = 10,
			[("GameResources/N64/Kirby64TheCrystalShards", 3)] = 11,
			[("GameResources/N64/Kirby64TheCrystalShards", 4)] = 12,
			[("GameResources/N64/Kirby64TheCrystalShards", 5)] = 13,
			[("GameResources/N64/Kirby64TheCrystalShards", 6)] = 14,
			[("GameResources/N64/Kirby64TheCrystalShards", 7)] = 15,
			[("GameResources/N64/Kirby64TheCrystalShards", 8)] = 16,
			[("GameResources/N64/Kirby64TheCrystalShards", 9)] = 17,
			[("GameResources/N64/Kirby64TheCrystalShards", 10)] = 18,
			[("GameResources/N64/Kirby64TheCrystalShards", 11)] = 19,
			[("GameResources/N64/Kirby64TheCrystalShards", 12)] = 20,
			[("GameResources/N64/Kirby64TheCrystalShards", 13)] = 21,
			[("GameResources/N64/Kirby64TheCrystalShards", 14)] = 22,
			[("GameResources/N64/Kirby64TheCrystalShards", 15)] = 23,
			[("GameResources/N64/Kirby64TheCrystalShards", 16)] = 24,
			[("GameResources/N64/Kirby64TheCrystalShards", 17)] = 25,
			[("DeletedPages/GameResources/N64/Kirby64TheCrystalShards", 9)] = 26,
			[("DeletedPages/GameResources/N64/Kirby64TheCrystalShards", 10)] = 27,
			[("GameResources/N64/Kirby64TheCrystalShards", 18)] = 28,
			[("GameResources/N64/Kirby64TheCrystalShards", 19)] = 29,
			[("GameResources/N64/Kirby64TheCrystalShards", 20)] = 30,
			[("GameResources/N64/Kirby64TheCrystalShards", 21)] = 31,
			[("DeletedPages/GameResources/N64/Kirby64TheCrystalShards", 11)] = 32,
			[("GameResources/N64/Kirby64TheCrystalShards", 22)] = 33,
			[("GameResources/N64/Kirby64TheCrystalShards", 23)] = 34,
		};
	}
}
