using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Helpers;
using TASVideos.Extensions;
using TASVideos.Legacy.Data.Site;
using TASVideos.Legacy.Data.Site.Entity;

namespace TASVideos.Legacy.Imports
{
	public static class WikiImporter
	{
		private class UserDto
		{
			public string Name { get; init; } = "";
			public string HomePage { get; init; } = "";
		}

		public static void Import(NesVideosSiteContext legacySiteContext)
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
				int revision = RevisionShenanigans(legacyPage.Site);

				pages.Add(new WikiPage
				{
					Id = legacyPage.Site.Id,
					PageName = pageName,
					Markup = ImportHelper.ConvertLatin1String(legacyPage.Site.Description) ?? "",
					Revision = revision,
					MinorEdit = legacyPage.Site.MinorEdit == "Y",
					RevisionMessage = legacyPage.Site.WhyEdit.Cap(1000),
					IsDeleted = legacyPage.Site.PageName.StartsWith("DeletedPages/"),
					CreateTimestamp = ImportHelper.UnixTimeStampToDateTime(legacyPage.Site.CreateTimestamp),
					CreateUserName = legacyPage.Site.User?.Name,
					LastUpdateTimestamp = ImportHelper.UnixTimeStampToDateTime(legacyPage.Site.CreateTimestamp),
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

			var wikiColumns = new[]
			{
				nameof(WikiPage.Id),
				nameof(WikiPage.ChildId),
				nameof(WikiPage.CreateTimestamp),
				nameof(WikiPage.CreateUserName),
				nameof(WikiPage.IsDeleted),
				nameof(WikiPage.LastUpdateTimestamp),
				nameof(WikiPage.LastUpdateUserName),
				nameof(WikiPage.Markup),
				nameof(WikiPage.MinorEdit),
				nameof(WikiPage.PageName),
				nameof(WikiPage.Revision),
				nameof(WikiPage.RevisionMessage)
			};

			pages.BulkInsert(wikiColumns, nameof(ApplicationDbContext.WikiPages));
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
			else if (pageName == "NoGamename")
			{
				pageName = "NoGameName";
			}
			else if (pageName == "Roles")
			{
				pageName = "Roles/Details";
			}
			else if (!string.IsNullOrEmpty(userName))
			{
				// Use the user's name for the page name instead of the actual page,
				// We want Homepages to match usernames exactly
				var slashIndex = pageName.IndexOf("/");
				if (slashIndex > 0)
				{
					pageName = userName + pageName[slashIndex..];
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
			"ZH/SubmitMovie",
			"PT/SubmitMovie",
			"Privileges",

			"SystemDiffLineLengthNote",
			"SystemUserEditRank",
			"SystemIsDeletedPage",
			"SystemIsPage",
			"SystemLayoutLeftMenu",
			"SystemLayoutMainMenu",
			"SystemLayoutTinyMenu",
			"SystemMovieBittorrentNag",
			"SystemMovieEditingFailure",
			"SystemMovieMoreMovies",
			"SystemMovieNoMovies",
			"SystemMovieRatingAccessDenied",
			"SystemMovieRatingViewAccessDenied",
			"SystemMovieWhyHowReference",
			"SystemNewPageTemplate",
			"SystemNotice10Bit444",
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
