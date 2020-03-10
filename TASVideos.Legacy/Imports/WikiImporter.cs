using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Helpers;
using TASVideos.Data.SeedData;
using TASVideos.Legacy.Data.Site;
using TASVideos.Legacy.Data.Site.Entity;
using TASVideos.WikiEngine;

// ReSharper disable StyleCop.SA1201
// ReSharper disable StyleCop.SA1503
namespace TASVideos.Legacy.Imports
{
	public static class WikiImporter
	{
		private class UserDto
		{
			public string Name { get; set; } = "";
			public string HomePage { get; set; } = "";
		}

		public static void Import(string connectionStr, ApplicationDbContext context, NesVideosSiteContext legacySiteContext)
		{
			var blacklist = ObsoletePages.Concat(ObsoletePages.Select(p => "DeletedPages/" + p));

			var siteTexts = legacySiteContext.SiteText
				.Include(s => s.User)
				.Where(w => !blacklist.Contains(w.PageName))
				.ToList();

			var legUsers = legacySiteContext.Users
				.Select(u => new UserDto { Name = u.Name, HomePage = u.HomePage })
				.ToList();

			var pages = new List<WikiPage>(siteTexts.Count);

			var siteTextWithUser = (from s in siteTexts
					join u in legUsers on s.PageName.Split("/").First().ToLower() equals u.Name == "TASVideos Grue" ? "tasvideosgrue" : u.HomePage.ToLower() into uu
					from u in uu.DefaultIfEmpty()
					select new { Site = s, User = u })
					.ToList();

			foreach (var legacyPage in siteTextWithUser)
			{
				string pageName = PageNameShenanigans(legacyPage.Site, legacyPage.User?.Name);
				string markup = MarkupShenanigans(legacyPage.Site, legUsers);
				int revision = RevisionShenanigans(legacyPage.Site);

				pages.Add(new WikiPage
				{
					Id = legacyPage.Site.Id,
					PageName = pageName,
					Markup = markup,
					Revision = revision,
					MinorEdit = legacyPage.Site.MinorEdit == "Y",
					RevisionMessage = legacyPage.Site.WhyEdit.Cap(1000),
					IsDeleted = legacyPage.Site.PageName.StartsWith("DeletedPages/"),
					CreateTimeStamp = ImportHelper.UnixTimeStampToDateTime(legacyPage.Site.CreateTimeStamp),
					CreateUserName = legacyPage.Site.User?.Name,
					LastUpdateTimeStamp = ImportHelper.UnixTimeStampToDateTime(legacyPage.Site.CreateTimeStamp),
					LastUpdateUserName = legacyPage.Site.User?.Name
				});
			}

			var dic = pages.ToDictionary(
				tkey => $"{tkey.PageName}__ImportKey__{tkey.Revision}",
				tvalue => tvalue);

			// Set child references
			foreach (var wikiPage in pages)
			{
				var result = dic.TryGetValue($"{wikiPage.PageName}__ImportKey__{wikiPage.Revision + 1}", out WikiPage? nextWiki);

				if (result)
				{
					wikiPage.ChildId = nextWiki!.Id;
				}
			}

			// Don't generate referrals for these, since they will be wiped and replaced after import anyway
			var overrides = WikiPageSeedData.NewRevisions
				.Select(wp => wp.PageName)
				.ToList();

			var pagesForReferral = pages
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

			pages.BulkInsert(connectionStr, wikiColumns, nameof(ApplicationDbContext.WikiPages), bulkCopyTimeout: 1200);

			var referralColumns = new[]
			{
				nameof(WikiPageReferral.Excerpt),
				nameof(WikiPageReferral.Referral),
				nameof(WikiPageReferral.Referrer)
			};

			referralList.BulkInsert(connectionStr, referralColumns, nameof(ApplicationDbContext.WikiReferrals), SqlBulkCopyOptions.Default, 100000, 300);
		}

		private static string PageNameShenanigans(SiteText st, string? userName)
		{
			string pageName = st.PageName;
			if (pageName.StartsWith("System") && pageName != "SystemPages")
			{
				pageName = pageName.Replace("System", "System/");
			}
			else if (pageName == "FrontPage")
			{
				pageName = "System/FrontPage";
			}
			else if (pageName.StartsWith("DeletedPages/"))
			{
				pageName = pageName.Replace("DeletedPages/", "");
			}
			else if (!string.IsNullOrEmpty(userName))
			{
				// Use the user's name for the page name instead of the actual page,
				// We want Homepages to match usernames exactly
				var slashIndex = pageName.IndexOf("/");
				if (slashIndex > 0)
				{
					pageName = userName + pageName.Substring(slashIndex, pageName.Length - slashIndex);
				}
				else
				{
					pageName = userName;
				}

				pageName = "HomePages/" + pageName;
			}
			else
			{
				var pubId = SubmissionHelper.IsPublicationLink(pageName);
				var subId = SubmissionHelper.IsSubmissionLink(pageName);
				var gamId = SubmissionHelper.IsGamePageLink(pageName);

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
			}

			return pageName;
		}

		private static string MarkupShenanigans(SiteText st, IEnumerable<UserDto> users)
		{
			string? markup = ImportHelper.ConvertLatin1String(st.Description) ?? "";

			// TODO: this page has a listparents that needs to be removed
			// However, we need better shenanigans to handle escaped module text
			// Ex: [[module:listparents]] since this page is full of these, obviously
			if (st.PageName == "TextFormattingRules/ListOfModules")
			{
				return markup;
			}

			if (st.PageName == "FrontPage")
			{
				markup = markup.Replace("!! Featured Movie", "");
			}
			else if (st.PageName == "Awards")
			{
				markup = markup.Replace("[module:listsubpages]", "");
				markup = markup.Replace("/images/awards/", "/awards/");
			}

			// Any shenanigans after this aren't worth fixing on old revisions
			if (st.ObsoletedBy.HasValue && st.ObsoletedBy != -1)
			{
				return markup;
			}
			if (markup.Contains("=css/fastest-completion.png")) markup = markup.Replace("=css/fastest-completion.png", "=images/fastest-completion.png");
			if (markup.Contains("=css/vaulttier.png")) markup = markup.Replace("=css/vaulttier.png", "=images/vaulttier.png");
			if (markup.Contains("=css/moontier.png")) markup = markup.Replace("=css/moontier.png", "=images/moontier.png");
			if (markup.Contains("=css/favourite.png")) markup = markup.Replace("=css/favourite.png", "=images/startier.png");
			if (markup.Contains("=/css/vaulttier.png")) markup = markup.Replace("=/css/vaulttier.png", "=images/vaulttier.png");
			if (markup.Contains("=/css/moontier.png")) markup = markup.Replace("=/css/moontier.png", "=images/moontier.png");
			if (markup.Contains("=/css/favourite.png")) markup = markup.Replace("=/css/favourite.png", "=images/startier.png");
			if (markup.Contains("=/css/newbierec.gif")) markup = markup.Replace("=/css/newbierec.gif", "=images/newbierec.gif");
			if (markup.Contains("=/css/bolt.png")) markup = markup.Replace("=/css/bolt.png", "=images/notable.png");
			if (markup.Contains("=/css/verified.png")) markup = markup.Replace("=/css/verified.png", "=images/verified.png");

			// These are automatic now
			markup = Regex.Replace(markup, "\\[module:gameheader\\]", "", RegexOptions.IgnoreCase);
			markup = Regex.Replace(markup, "\\[module:gamefooter\\]", "", RegexOptions.IgnoreCase);

			// Mitigate unnecessary ListParent module calls, if they are at the beginning, wipe them.
			// We can't remove all instances because of pages like Interviews/Phil/GEE2005
			// Where below the title there is Back To: %%%[module:ListParents]
			if (markup.StartsWith("[module:listparents]", StringComparison.InvariantCultureIgnoreCase))
			{
				markup = Regex.Replace(markup, "\\[module:listparents\\]", "", RegexOptions.IgnoreCase);
			}

			// Ditto for pages that end with ListSubPages
			if (markup.EndsWith("[module:listsubpages]", StringComparison.InvariantCultureIgnoreCase))
			{
				markup = Regex.Replace(markup, "\\[module:listsubpages\\]", "", RegexOptions.IgnoreCase);
			}

			// Common markup mistakes
			if (markup.Contains(" [!]")) markup = markup.Replace(" [!]", " [[!]]"); // Non-escaped Rom names, shenanigans to avoid turning proper markup: [[!]] into [[[!]]]
			if (markup.Contains(")[!]")) markup = markup.Replace(")[!]", "[[!]]"); // Non-escaped Rom names
			if (markup.Contains("[''''!'''']")) markup = markup.Replace("[''''!'''']", "[[!]]");

			// Fix improperly linked homepages
			var usersWithPages = users.Where(u => u.HomePage != "").ToList();
			foreach (var user in usersWithPages)
			{
				// TODO: some regex would really help here
				var bareLink = $"[{user.HomePage}]";
				var trailingSlashLink = $"[{user.HomePage}/]";
				var aliasedLink = $"[{user.HomePage}|";

				// Note: it is important to do the trailing slash replace first
				var subPage = $"[{user.HomePage}/";

				if (markup.Contains(bareLink, StringComparison.OrdinalIgnoreCase))
				{
					markup = markup.ReplaceInsensitive(bareLink, $"[user:{user.Name}]");
				}

				if (markup.Contains(trailingSlashLink, StringComparison.OrdinalIgnoreCase))
				{
					markup = markup.ReplaceInsensitive(trailingSlashLink, $"[user:{user.Name}]");
				}

				if (markup.Contains(aliasedLink, StringComparison.OrdinalIgnoreCase))
				{
					markup = markup.ReplaceInsensitive(aliasedLink, $"[user:{user.Name}|");
				}

				if (markup.Contains(subPage, StringComparison.OrdinalIgnoreCase))
				{
					markup = markup.ReplaceInsensitive(subPage, $"[/HomePages/{user.Name}/");
				}
			}

			return markup;
		}

		private static int RevisionShenanigans(SiteText st)
		{
			int revision = st.Revision;

			// This page had 2 deleted pages that came first, so we can just add to the revision number
			if (st.PageName == "GameResources/DS/MetroidPrimeHunters")
			{
				revision += 2;
			}

			return revision;
		}

		// ReSharper disable once StyleCop.SA1201
		private static readonly string[] ObsoletePages = 
		{
			"AccessBlocked",
			"CacheControl",
			"Login",
			"OldMovies",
			"SiteTechnology",
			"SiteTechnology/Database",
			"Urmom", // What fool would do this?
			"TASSnapshot",
			"TASSnapshot/Filelist",
			"SiteTechnology/API",
			"SandBox", // History for this page isn't important
			"SubmitMovie",

			"SystemDiffLineLengthNote",
			"SystemUserEditRank",
			"SystemIsDeletedPage",
			"SystemIsPage",
			"SystemLayoutLeftMenu",
			"SystemLayoutMainMenu",
			"SystemLayoutTinyMenu",
			"SystemMovieBittorrentNag",
			"SystemMovieEditingFailure",
			"SystemMovieNoMovies",
			"SystemMovieRatingAccessDenied",
			"SystemMovieRatingViewAccessDenied",
			"SystemMovieWhyHowReference",
			"SystemNewPageTemplate",
			"SystemNotPageEditor",
			"SystemRestrictedPage",
			"SystemPageNotFound",
			"SystemPages",
			"SystemRestrictedPage",
			"SystemSmvVersionTooOld",
			"SystemSubmissionComplete",
			"SystemSubmissionEditingFailure",
			"SystemSubmissionViewingFailur",
			"SystemSubmissionVotingHelp",
			"SystemSubmissionLoginFailure",
			"SystemViewPageSource"
		};
	}
}
