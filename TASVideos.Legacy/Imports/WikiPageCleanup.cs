using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.SeedData;
using TASVideos.Legacy.Data.Site;

namespace TASVideos.Legacy.Imports
{
	public static class WikiPageCleanup
	{
		private class UserDto
		{
			public string Name { get; init; } = "";
			public string HomePage { get; init; } = "";
		}

		public static void Fix(
			ApplicationDbContext context,
			NesVideosSiteContext legacySiteContext)
		{
			var currentPages = context.WikiPages
				.ThatAreNotDeleted()
				.Where(wp => wp.ChildId == null)
				.ToList();

			var legUsers = legacySiteContext.Users
				.Select(u => new UserDto { Name = u.Name, HomePage = u.HomePage })
				.ToList();

			foreach (var page in currentPages)
			{
				var newRevision = MarkupShenanigans(page, legUsers);
				if (newRevision != null)
				{
					page.Child = newRevision;
					context.WikiPages.Add(newRevision); 
				}
			}

			context.SaveChanges();
		}

		private static WikiPage? MarkupShenanigans(WikiPage page, List<UserDto> users)
		{
			string markup = page.Markup;

			// TODO: this page has a listparents that needs to be removed
			// However, we need better shenanigans to handle escaped module text
			// Ex: [[module:listparents]] since this page is full of these, obviously
			if (page.PageName == "TextFormattingRules/ListOfModules")
			{
				return null;
			}

			if (page.PageName == "System/FrontPage")
			{
				markup = markup.Replace("!! Featured Movie", "");
			}
			else if (page.PageName == "Awards")
			{
				markup = markup.Replace("[module:listsubpages]", "");
				markup = markup.Replace("/images/awards/", "/awards/");
			}
			else if (page.PageName == "Links")
			{
				markup = markup.Replace("[module:GoogleFlavor]", "");
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

			if (markup != page.Markup)
			{
				var newRevision = new WikiPage
				{
					Id = 0,
					PageName = page.PageName,
					Markup = markup,
					Revision = page.Revision + 1,
					RevisionMessage = WikiPageSeedData.Import,
					CreateTimeStamp = DateTime.UtcNow,
					LastUpdateTimeStamp =  DateTime.UtcNow,
					ChildId = null
				};

				return newRevision;
			}

			return null;
		}
	}
}
