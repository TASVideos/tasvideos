using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Constants;
using TASVideos.Data.Entity;
using TASVideos.Data.Helpers;
using TASVideos.Legacy.Data.Site;
using TASVideos.Legacy.Data.Site.Entity;
using TASVideos.WikiEngine;

// ReSharper disable StyleCop.SA1201
// ReSharper disable StyleCop.SA1503
namespace TASVideos.Legacy.Imports
{
	public static class WikiImporter
	{
		public static void Import(string connectionStr, ApplicationDbContext context, NesVideosSiteContext legacySiteContext)
		{
			var siteTexts = legacySiteContext.SiteText
				.Include(s => s.User)
				.ToList();

			var legUsers = legacySiteContext.Users.Select(u => new { u.Name, u.HomePage }).ToList();

			var pages = new List<WikiPage>(siteTexts.Count);

			var siteTextWithUser = (from s in siteTexts
					join u in legUsers on s.PageName.Split("/").First().ToLower() equals u.Name == "TASVideos Grue" ? "tasvideosgrue" : u.HomePage.ToLower() into uu
					from u in uu.DefaultIfEmpty()
					select new { Site = s, User = u })
					.ToList();

			foreach (var legacyPage in siteTextWithUser)
			{
				string pageName = PageNameShenanigans(legacyPage.Site, legacyPage.User?.Name);
				string markup = MarkupShenanigans(legacyPage.Site);
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

			pages.BulkInsert(connectionStr, wikiColumns, nameof(ApplicationDbContext.WikiPages), bulkCopyTimeout: 1200);

			var referralColumns = new[]
			{
				nameof(WikiPageReferral.Excerpt),
				nameof(WikiPageReferral.Referral),
				nameof(WikiPageReferral.Referrer)
			};

			referralList.BulkInsert(connectionStr, referralColumns, nameof(ApplicationDbContext.WikiReferrals), SqlBulkCopyOptions.Default, 100000, 300);
		}

		private static string PageNameShenanigans(SiteText st, string userName)
		{
			string pageName = st.PageName;
			if (pageName.StartsWith("System"))
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

		private static string MarkupShenanigans(SiteText st)
		{
			string markup = ImportHelper.ConvertLatin1String(st.Description);

			if (st.PageName == "FrontPage")
			{
				markup = markup.Replace("!! Featured Movie", "");
			}
			else if (st.PageName == "Awards")
			{
				markup = markup.Replace("[module:listsubpages]", "");
			}

			if (markup.Contains("=css/vaulttier.png")) markup = markup.Replace("=css/vaulttier.png", "=images/vaulttier.png");
			if (markup.Contains("=css/moontier.png")) markup = markup.Replace("=css/moontier.png", "=images/moontier.png");
			if (markup.Contains("=css/favourite.png")) markup = markup.Replace("=css/favourite.png", "=images/startier.png");
			if (markup.Contains("=/css/vaulttier.png")) markup = markup.Replace("=/css/vaulttier.png", "=images/vaulttier.png");
			if (markup.Contains("=/css/moontier.png")) markup = markup.Replace("=/css/moontier.png", "=images/moontier.png");
			if (markup.Contains("=/css/favourite.png")) markup = markup.Replace("=/css/favourite.png", "=images/startier.png");

			// Fix known links that failed to use the user module
			if (markup.Contains("[Acmlm]")) markup = markup.Replace("[Acmlm]", "[user:Acmlm]");
			if (markup.Contains("[adelikat]")) markup = markup.Replace("[adelikat]", "[user:adelikat]");
			if (markup.Contains("[Adelikat]")) markup = markup.Replace("[Adelikat]", "[user:adelikat]");
			if (markup.Contains("[Aqfaq]")) markup = markup.Replace("[Aqfaq]", "[user:Aqfaq]");
			if (markup.Contains("[Arc]")) markup = markup.Replace("[Arc]", "[user:Arc]");
			if (markup.Contains("[AKheon]")) markup = markup.Replace("[AKheon]", "[user:AKheon]");
			if (markup.Contains("[Aktan]")) markup = markup.Replace("[Aktan]", "[user:Aktan]");
			if (markup.Contains("[Alden]")) markup = markup.Replace("[Alden]", "[user:alden]");
			if (markup.Contains("[alden]")) markup = markup.Replace("[alden]", "[user:alden]");
			if (markup.Contains("[andrewg]")) markup = markup.Replace("[andrewg]", "[user:andrewg]");
			if (markup.Contains("[Anty-Lemon]")) markup = markup.Replace("[Anty-Lemon]", "[user:Anty-Lemon]");
			if (markup.Contains("[AngerFist]")) markup = markup.Replace("[AngerFist]", "[user:AngerFist]");
			if (markup.Contains("[arandomgameTASer]")) markup = markup.Replace("[arandomgameTASer]", "[user:arandomgameTASer]");
			if (markup.Contains("[arukAdo]")) markup = markup.Replace("[arukAdo]", "[user:arukAdo]");
			if (markup.Contains("[Baxter]")) markup = markup.Replace("[Baxter]", "[user:Baxter]");
			if (markup.Contains("[Bisqwit]")) markup = markup.Replace("[Bisqwit]", "[user:Bisqwit]");
			if (markup.Contains("[Blechy]")) markup = markup.Replace("[Blechy]", "[user:Blechy]");
			if (markup.Contains("[blip]")) markup = markup.Replace("[blip]", "[user:blip]");
			if (markup.Contains("[BoltR]")) markup = markup.Replace("[BoltR]", "[user:BoltR]");
			if (markup.Contains("[Brandon]")) markup = markup.Replace("[Brandon]", "[user:Brandon]");
			if (markup.Contains("[Cardboard]")) markup = markup.Replace("[Cardboard]", "[user:Cardboard]");
			if (markup.Contains("[CoolKirby]")) markup = markup.Replace("[CoolKirby]", "[user:CoolKirby]");
			if (markup.Contains("[Comicalflop]")) markup = markup.Replace("[Comicalflop]", "[user:Comicalflop]");
			if (markup.Contains("[Dada]")) markup = markup.Replace("[Dada]", "[user:Dada]");
			if (markup.Contains("[Dan_]")) markup = markup.Replace("[Dan_]", "[user:Dan_]");
			if (markup.Contains("[DarkKobold]")) markup = markup.Replace("[DarkKobold]", "[user:DarkKobold]");
			if (markup.Contains("[DeHackEd]")) markup = markup.Replace("[DeHackEd]", "[user:DeHackEd]");
			if (markup.Contains("[Deign]")) markup = markup.Replace("[Deign]", "[user:Deign]");
			if (markup.Contains("[Dooty]")) markup = markup.Replace("[Dooty]", "[user:Dooty]");
			if (markup.Contains("[FatRatKnight]")) markup = markup.Replace("[FatRatKnight]", "[user:FatRatKnight]");
			if (markup.Contains("[FerretWarlord]")) markup = markup.Replace("[FerretWarlord]", "[user:FerretWarlord]");
			if (markup.Contains("[feos]")) markup = markup.Replace("[feos]", "[user:feos]");
			if (markup.Contains("[FinalFighter]")) markup = markup.Replace("[FinalFighter]", "[user:finalfighter]");
			if (markup.Contains("[Finalfighter]")) markup = markup.Replace("[Finalfighter]", "[user:finalfighter]");
			if (markup.Contains("[Fog]")) markup = markup.Replace("[Fog]", "[user:Fog]");
			if (markup.Contains("[FractalFusion]")) markup = markup.Replace("[FractalFusion]", "[user:FractalFusion]");
			if (markup.Contains("[Flygon]")) markup = markup.Replace("[Flygon]", "[user:Flygon]");
			if (markup.Contains("[fsvgm777]")) markup = markup.Replace("[fsvgm777]", "[user:fsvgm777]");
			if (markup.Contains("[Genisto]")) markup = markup.Replace("[Genisto]", "[user:Genisto]");
			if (markup.Contains("[GoddessMaria]")) markup = markup.Replace("[GoddessMaria]", "[user:GoddessMaria]");
			if (markup.Contains("[Guga]")) markup = markup.Replace("[Guga]", "[user:Guga]");
			if (markup.Contains("[HappyLee]")) markup = markup.Replace("[HappyLee]", "[user:HappyLee]");
			if (markup.Contains("[ilari]")) markup = markup.Replace("[ilari]", "[user:ilari]");
			if (markup.Contains("[Ilari]")) markup = markup.Replace("[Ilari]", "[user:ilari]");
			if (markup.Contains("[Ideamagnate]")) markup = markup.Replace("[Ideamagnate]", "[user:Ideamagnate]");
			if (markup.Contains("[JXQ]")) markup = markup.Replace("[JXQ]", "[user:JXQ]");
			if (markup.Contains("[Kirkq")) markup = markup.Replace("[Kirkq", "[user:Kirkq");
			if (markup.Contains("[klmz]")) markup = markup.Replace("[klmz]", "[user:klmz]");
			if (markup.Contains("[Klmz]")) markup = markup.Replace("[Klmz]", "[user:klmz]");
			if (markup.Contains("[Maza]")) markup = markup.Replace("[Maza]", "[user:Maza]");
			if (markup.Contains("[MESHUGGAH]")) markup = markup.Replace("[MESHUGGAH]", "[user:MESHUGGAH]");
			if (markup.Contains("[Mitjitsu")) markup = markup.Replace("[Mitjitsu", "[user:Mitjitsu");
			if (markup.Contains("[mmbossman]")) markup = markup.Replace("[mmbossman]", "[user:mmbossman]");
			if (markup.Contains("[moozooh]")) markup = markup.Replace("[moozooh]", "[user:moozooh]");
			if (markup.Contains("[Morrison]")) markup = markup.Replace("[Morrison]", "[user:Morrison]");
			if (markup.Contains("[Mothrayas]")) markup = markup.Replace("[Mothrayas]", "[user:Mothrayas]");
			if (markup.Contains("[mugg]")) markup = markup.Replace("[mugg]", "[user:MUGG]");
			if (markup.Contains("[MUGG]")) markup = markup.Replace("[MUGG]", "[user:MUGG]");
			if (markup.Contains("[Mukki]")) markup = markup.Replace("[Mukki]", "[user:Mukki]");
			if (markup.Contains("[Nach]")) markup = markup.Replace("[Nach]", "[user:Nach]");
			if (markup.Contains("[nanogyth]")) markup = markup.Replace("[nanogyth]", "[user:nanogyth]");
			if (markup.Contains("[natt]")) markup = markup.Replace("[natt]", "[user:natt]");
			if (markup.Contains("[nifboy]")) markup = markup.Replace("[nifboy]", "[user:nifboy]");
			if (markup.Contains("[Nifboy]")) markup = markup.Replace("[Nifboy]", "[user:Nifboy]");
			if (markup.Contains("[NitroGenesis]")) markup = markup.Replace("[NitroGenesis]", "[user:NitroGenesis]");
			if (markup.Contains("[nitrogenesis]")) markup = markup.Replace("[nitrogenesis]", "[user:NitroGenesis]");
			if (markup.Contains("[Nitsuja]")) markup = markup.Replace("[Nitsuja]", "[user:nitsuja]");
			if (markup.Contains("[nitsuja]")) markup = markup.Replace("[nitsuja]", "[user:nitsuja]");
			if (markup.Contains("[OmnipotentEntity]")) markup = markup.Replace("[OmnipotentEntity]", "[user:OmnipotentEntity]");
			if (markup.Contains("[Phil]")) markup = markup.Replace("[Phil]", "[user:Phil]");
			if (markup.Contains("[phil]")) markup = markup.Replace("[phil]", "[user:Phil]");
			if (markup.Contains("[Raiscan]")) markup = markup.Replace("[Raiscan]", "[user:Raiscan]");
			if (markup.Contains("[Randil]")) markup = markup.Replace("[Randil]", "[user:Randil]");
			if (markup.Contains("[Patryk1023]")) markup = markup.Replace("[Patryk1023]", "[user:Patryk1023]");
			if (markup.Contains("[RGamma]")) markup = markup.Replace("[RGamma]", "[user:RGamma]");
			if (markup.Contains("[Scepheo]")) markup = markup.Replace("[Scepheo]", "[user:Scepheo]");
			if (markup.Contains("[sgrunt]")) markup = markup.Replace("[sgrunt]", "[user:sgrunt]");
			if (markup.Contains("[sheela901]")) markup = markup.Replace("[sheela901]", "[user:sheela901]");
			if (markup.Contains("[Solarplex]")) markup = markup.Replace("[Solarplex]", "[user:Solarplex]");
			if (markup.Contains("[Spikestuff]")) markup = markup.Replace("[Spikestuff]", "[user:Spikestuff]");
			if (markup.Contains("[TASeditor]")) markup = markup.Replace("[TASeditor]", "[user:TASeditor]");
			if (markup.Contains("[thecoreyburton]")) markup = markup.Replace("[thecoreyburton]", "[user:thecoreyburton]");
			if (markup.Contains("[theenglishman]")) markup = markup.Replace("[theenglishman]", "[user:theenglishman]");
			if (markup.Contains("[ThunderAxe31]")) markup = markup.Replace("[ThunderAxe31]", "[user:ThunderAxe31]");
			if (markup.Contains("[Toothache]")) markup = markup.Replace("[Toothache]", "[user:Toothache]");
			if (markup.Contains("[Truncated]")) markup = markup.Replace("[Truncated]", "[user:Truncated]");
			if (markup.Contains("[turska]")) markup = markup.Replace("[turska]", "[user:turska]");
			if (markup.Contains("[Upthorn]")) markup = markup.Replace("[Upthorn]", "[user:upthorn]");
			if (markup.Contains("[Vatchern]")) markup = markup.Replace("[Vatchern]", "[user:Vatchern]");
			if (markup.Contains("[Velitha]")) markup = markup.Replace("[Velitha]", "[user:Velitha]");
			if (markup.Contains("[Ventuz]")) markup = markup.Replace("[Ventuz]", "[user:ventuz]");
			if (markup.Contains("[Walker Boh]")) markup = markup.Replace("[Walker Boh]", "[user:Walker Boh]");
			if (markup.Contains("[WalkerBoh]")) markup = markup.Replace("[WalkerBoh]", "[user:Walker Boh]");
			if (markup.Contains("[WST]")) markup = markup.Replace("[WST]", "[user:WST]");
			if (markup.Contains("[Zurreco]")) markup = markup.Replace("[Zurreco]", "[user:Zurreco]");
			if (markup.Contains("[Zggzdydp]")) markup = markup.Replace("[Zggzdydp]", "[user:zggzdydp]");
			if (markup.Contains("[zggzdydp]")) markup = markup.Replace("[zggzdydp]", "[user:zggzdydp]");

			// Improperly done user modules
			if (markup.Contains("[Alden|alden]")) markup = markup.Replace("[Alden|alden]", "[user:alden]");
			if (markup.Contains("[Comicalflop|Comicalflop]")) markup = markup.Replace("[Comicalflop|Comicalflop]", "[user:Comicalflop]");
			if (markup.Contains("[user:Dan]")) markup = markup.Replace("[user:Dan]", "[user:Dan_]");
			if (markup.Contains("[DarkKobold|DarkKobold]")) markup = markup.Replace("[DarkKobold|DarkKobold]", "[user:DarkKobold]");
			if (markup.Contains("[Deign|Deign]")) markup = markup.Replace("[Deign|Deign]", "[user:Deign]");
			if (markup.Contains("[FractalFusion|FractalFusion]")) markup = markup.Replace("[FractalFusion|FractalFusion]", "[user:FractalFusion]");
			if (markup.Contains("[Ilari|Ilari]")) markup = markup.Replace("[Ilari|Ilari]", "[user:Ilari]");
			if (markup.Contains("[JXQ|JXQ]")) markup = markup.Replace("[JXQ|JXQ]", "[user:JXQ]");
			if (markup.Contains("[Mmbossman|mmbossman]")) markup = markup.Replace("[Mmbossman|mmbossman]", "[user:mmbossman]");
			if (markup.Contains("[Mugg|MUGG]")) markup = markup.Replace("[Mugg|MUGG]", "[user:MUGG]");
			if (markup.Contains("[Mukki|Mukki]")) markup = markup.Replace("[Mukki|Mukki]", "[user:Mukki]");
			if (markup.Contains("[nitsuja|nitsuja]")) markup = markup.Replace("[nitsuja|nitsuja]", "[user:nitsuja]");
			if (markup.Contains("[Randil|Randil]")) markup = markup.Replace("[Randil|Randil]", "[user:Randil]");
			if (markup.Contains("[upthorn|Upthorn]")) markup = markup.Replace("[upthorn|Upthorn]", "[user:upthorn]");
			if (markup.Contains("[Zggzdydp|zggzdydp]")) markup = markup.Replace("[Zggzdydp|zggzdydp]", "[user:zggzdydp]");

			if (markup.Contains("[Adelikat/AllPlatformsChallenge")) markup = markup.Replace("[Adelikat/AllPlatformsChallenge", "[HomePages/adelikat/AllPlatformsChallenge");
			if (markup.Contains("[adelikat/AVGN")) markup = markup.Replace("[adelikat/AVGN", "[HomePages/adelikat/AVGN");
			if (markup.Contains("[adelikat/EmptyQueue")) markup = markup.Replace("[adelikat/EmptyQueue", "[HomePages/adelikat/EmptyQueue");
			if (markup.Contains("[adelikat/Movies")) markup = markup.Replace("[adelikat/Movies", "[HomePages/adelikat/Movies");
			if (markup.Contains("[adelikat/MoviesCommentary")) markup = markup.Replace("[adelikat/MoviesCommentary", "[HomePages/adelikat/MoviesCommentary");

			if (markup.Contains("[Alden/")) markup = markup.Replace("[Alden/]", "[HomePages/alden/");

			if (st.PageName == "Ais523")
			{
				markup = markup.Replace("[ais523/Categorization]", "[HomePages/ais523/Categorization]");
			}

			if (markup.Contains("[Bisqwit/")) markup = markup.Replace("[Bisqwit/]", "[HomePages/Bisqwit/");
			if (markup.Contains("[FractalFusion/")) markup = markup.Replace("[FractalFusion/]", "[HomePages/FractalFusion/");
			if (markup.Contains("[fsvgm777/")) markup = markup.Replace("[fsvgm777/]", "[HomePages/fsvgm777/");
			if (markup.Contains("[Ilari/")) markup = markup.Replace("[Ilari/]", "[HomePages/Ilari/");
			if (markup.Contains("[Moozooh/")) markup = markup.Replace("[Moozooh/]", "[HomePages/moozooh/");
			if (markup.Contains("[Mothrayas/")) markup = markup.Replace("[Mothrayas/]", "[HomePages/Mothrayas/");
			if (markup.Contains("[Mugg/")) markup = markup.Replace("[Mugg/]", "[HomePages/MUGG/");
			if (markup.Contains("[Nach/")) markup = markup.Replace("[Nach/]", "[HomePages/Nach/");
			if (markup.Contains("[PJBoy/")) markup = markup.Replace("[PJBoy/]", "[HomePages/P.JBoy/");

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
	}
}
