using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

using FastMember;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Constants;
using TASVideos.Data.Entity;
using TASVideos.Data.Helpers;
using TASVideos.Legacy.Data.Site;
using TASVideos.Legacy.Data.Site.Entity;
using TASVideos.WikiEngine;

namespace TASVideos.Legacy.Imports
{
	public static class WikiImporter
	{
		public static void Import(ApplicationDbContext context, NesVideosSiteContext legacySiteContext)
		{
			List<SiteText> siteTexts = legacySiteContext.SiteText
				//.Where(s => s.Type == "S" && s.ObsoletedBy == -1 && !(s.PageName == "5029S" && s.Revision == 2)) // TODO: fix this record!
				.ToList();

			var usernames = context.Users.Select(u => u.UserName).ToList();
			var legacyUsers = legacySiteContext.Users.ToList();

			var pages = new List<WikiPage>();
			var referralList = new List<WikiPageReferral>();
			foreach (var legacyPage in siteTexts)
			{
				string markup = legacyPage.Description;
				int revision = legacyPage.Revision;
				string pageName = legacyPage.PageName;

				if (legacyPage.PageName.StartsWith("System"))
				{
					pageName = legacyPage.PageName.Replace("System", "System/");
				}
				else if (legacyPage.PageName == "FrontPage")
				{
					pageName = "System/FrontPage";
					markup = markup.Replace("[module:welcome]", "");
				}

				// ******** Deleted pages that were recreated *************/

				// Not worth preserving history here, revisions were mistakes and revision history is too large
				else if (legacyPage.PageName == "DeletedPages/Bizhawk/ReleaseHistory")
				{
					continue;
				}

				else if (legacyPage.PageName == "GameResources/N64/Kirby64TheCrystalShards" || legacyPage.PageName == "DeletedPages/GameResources/N64/Kirby64TheCrystalShards")
				{
					revision = CrystalShardsLookup[(legacyPage.PageName, legacyPage.Revision)];
				}

				// This page had 2 deleted pages that came first, so we can just add to the revision number
				else if (legacyPage.PageName == "GameResources/DS/MetroidPrimeHunters")
				{
					revision += 2;
				}

				// ******** END Deleted pages that were recreated *************/

				// Shenanigans
				else if (legacyPage.PageName == "Phil" && legacyPage.Revision >= 7 && legacyPage.Revision <= 11)
				{
					markup = markup.Replace(":[", ":|");
				}
				else if (legacyPage.PageName == "971S" && legacyPage.Revision == 3)
				{
					markup = markup.Replace("[Phi:", "[Phil]:");
				}


				var pubId = SubmissionHelper.IsPublicationLink(legacyPage.PageName);
				var subId = SubmissionHelper.IsSubmissionLink(legacyPage.PageName);

				bool isDeleted = false;
				if (pubId.HasValue)
				{
					pageName = LinkConstants.PublicationWikiPage + pubId.Value;
				}
				else if (subId.HasValue)
				{
					pageName = LinkConstants.SubmissionWikiPage + subId.Value;
				}
				else if (pageName.StartsWith("DeletedPages/"))
				{
					pageName = pageName.Replace("DeletedPages/", "");
					isDeleted = true;
				}

				if (usernames.Contains(pageName))
				{
					pageName = "HomePages/" + pageName;
				}

				markup = markup.Replace("=css/vaulttier.png", "=images/vaulttier.png");
				markup = markup.Replace("=css/moontier.png", "=images/moontier.png");
				markup = markup.Replace("=css/favourite.png", "=images/startier.png");
				markup = markup.Replace("=/css/vaulttier.png", "=images/vaulttier.png");
				markup = markup.Replace("=/css/moontier.png", "=images/moontier.png");
				markup = markup.Replace("=/css/favourite.png", "=images/startier.png");


				var wikiPage = new WikiPage
				{
					PageName = pageName,
					Markup = markup,
					Revision = revision,
					MinorEdit = legacyPage.MinorEdit == "Y",
					RevisionMessage = legacyPage.WhyEdit,
					IsDeleted = isDeleted,
					CreateTimeStamp = ImportHelpers.UnixTimeStampToDateTime(legacyPage.CreateTimeStamp),
					CreateUserName = legacyUsers.Single(u => u.Id == legacyPage.UserId).Name,
					LastUpdateTimeStamp = ImportHelpers.UnixTimeStampToDateTime(legacyPage.CreateTimeStamp)
				};

				pages.Add(wikiPage);
			}

			// Set child references
			foreach (var wikiPage in pages)
			{
				var nextWiki = pages
					.SingleOrDefault(wp => wp.Revision == wikiPage.Revision + 1
						&& wp.PageName == wikiPage.PageName);

				if (nextWiki != null)
				{
					wikiPage.ChildId = nextWiki.Id;
				}
			}

			// Referrals (only need latest revisions)
			foreach (var currentPage in pages.Where(p => p.ChildId == null))
			{
				var referrals = Util.GetAllWikiLinks(currentPage.Markup);
				foreach (var referral in referrals)
				{
					referralList.Add(new WikiPageReferral
					{
						Referrer = currentPage.PageName,
						Referral = referral.Link?.Split('|').FirstOrDefault(),
						Excerpt = referral.Excerpt
					});
				}
			}

			var wikiCopyParams = new[]
			{
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

			using (var sqlCopy = new SqlBulkCopy(context.Database.GetDbConnection().ConnectionString))
			{
				sqlCopy.DestinationTableName = $"[{nameof(ApplicationDbContext.WikiPages)}]";
				sqlCopy.BatchSize = 10000;

				foreach (var param in wikiCopyParams)
				{
					sqlCopy.ColumnMappings.Add(param, param);
				}

				using (var reader = ObjectReader.Create(pages, wikiCopyParams))
				{
					sqlCopy.WriteToServer(reader);
				}
			}

			var referralCopyParams = new[]
			{
				nameof(WikiPageReferral.Excerpt),
				nameof(WikiPageReferral.Referral),
				nameof(WikiPageReferral.Referrer)
			};

			using (var referralCopy = new SqlBulkCopy(context.Database.GetDbConnection().ConnectionString))
			{
				referralCopy.DestinationTableName = $"[{nameof(ApplicationDbContext.WikiReferrals)}]";
				referralCopy.BatchSize = 100000;

				foreach (var param in referralCopyParams)
				{
					referralCopy.ColumnMappings.Add(param, param);
				}

				using (var reader = ObjectReader.Create(referralList, referralCopyParams))
				{
					referralCopy.WriteToServer(reader);
				}
			}
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
