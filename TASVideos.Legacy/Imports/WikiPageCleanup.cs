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
	internal static class WikiPageCleanup
	{
		private record UserDto(string Name, string HomePage);

		public static void Fix(
			ApplicationDbContext context,
			NesVideosSiteContext legacySiteContext)
		{
			var currentPages = context.WikiPages
				.ThatAreNotDeleted()
				.Where(wp => wp.ChildId == null)
				.ToList();

			var legUsers = legacySiteContext.Users
				.Select(u => new UserDto(u.Name, u.HomePage))
				.ToList();

			foreach (var page in currentPages)
			{
				var newRevision = MarkupShenanigans(page, legUsers);
				if (newRevision is not null)
				{
					page.Child = newRevision;
					context.WikiPages.Add(newRevision);
				}
			}

			context.SaveChanges();
		}

		private static string ReplaceIfContains(string markup, string orig, string newStr)
		{
			if (markup.Contains(orig))
			{
				return markup.Replace(orig, newStr);
			}

			return markup;
		}

		private static WikiPage? MarkupShenanigans(WikiPage page, IEnumerable<UserDto> users)
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

			markup = ReplaceIfContains(markup, "=css/fastest-completion.png", "=images/fastest-completion.png");
			markup = ReplaceIfContains(markup, "=css/vaulttier.png", "=images/vaulttier.png");
			markup = ReplaceIfContains(markup, "=css/moontier.png", "=images/moontier.png");
			markup = ReplaceIfContains(markup, "=css/favourite.png", "=images/startier.png");
			markup = ReplaceIfContains(markup, "=/css/vaulttier.png", "=images/vaulttier.png");
			markup = ReplaceIfContains(markup, "=/css/moontier.png", "=images/moontier.png");
			markup = ReplaceIfContains(markup, "=/css/favourite.png", "=images/startier.png");
			markup = ReplaceIfContains(markup, "=/css/newbierec.gif", "=images/newbierec.gif");
			markup = ReplaceIfContains(markup, "=/css/bolt.png", "=images/notable.png");
			markup = ReplaceIfContains(markup, "=/css/verified.png", "=images/verified.png");

			// These are automatic now
			markup = Regex.Replace(markup, "\\[module:gameheader\\]", "", RegexOptions.IgnoreCase);
			markup = Regex.Replace(markup, "\\[module:gamefooter\\]", "", RegexOptions.IgnoreCase);

			markup = ReplaceIfContains(markup, "http://files.tasvideos.org/1853/site_images", "=/images");
			markup = ReplaceIfContains(markup, "http://files.tasvideos.org/common/Wiki", "=/images");

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
			// Non-escaped Rom names, shenanigans to avoid turning proper markup: [[!]] into [[[!]]]
			if (markup.Contains(" [!]"))
			{
				markup = markup.Replace(" [!]", " [[!]]");
			}

			// Non-escaped Rom names
			if (markup.Contains(")[!]"))
			{
				markup = markup.Replace(")[!]", "[[!]]");
			}

			if (markup.Contains("[''''!'''']"))
			{
				markup = markup.Replace("[''''!'''']", "[[!]]");
			}

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
					CreateTimestamp = DateTime.UtcNow,
					LastUpdateTimestamp = DateTime.UtcNow,
					ChildId = null
				};

				return newRevision;
			}

			return null;
		}
	}
}
