using TASVideos.Core.Services.Wiki;

namespace TASVideos.Core.Services;

public interface ILanguages
{
	Task<TranslationsData> GetTranslations(string pageName);
}

internal class Languages(ApplicationDbContext db, IWikiPages wikiPages, ICacheService cache) : ILanguages
{
	internal const string TranslationsCacheKey = "Translations-";

	public async Task<TranslationsData> GetTranslations(string pageName)
	{
		var key = TranslationsCacheKey + pageName;
		if (cache.TryGetValue(key, out ICollection<LanguagePage> languages))
		{
			return new(await IsLanguagePage(pageName) ?? Language.English, languages);
		}

		var translationsData = await GetTranslationsInternal(pageName);
		cache.Set(key, translationsData.TrPageList, Durations.FiveMinutes);
		return translationsData;
	}

	private async Task<TranslationsData> GetTranslationsInternal(string pageName)
	{
		string subPage = pageName;
		var languages = new List<LanguagePage>();
		var thisPageLang = await IsLanguagePage(pageName);
		if (thisPageLang is not null)
		{
			if (!pageName.Contains('/'))
			{
				return new(thisPageLang, []);
			}

			subPage = string.Join("/", pageName.Split('/').Skip(1));

			// Translations should also include the original link to the English version
			languages.Add(new(Language.English, subPage));
		}
		else
		{
			thisPageLang = Language.English;
		}

		languages.AddRange((await AvailableLanguages())
			.Select(l => new LanguagePage(l, $"{l.Code}/{subPage}"))
			.ToList());

		if (!languages.Any())
		{
			return new(thisPageLang, []);
		}

		var existingLanguagePages = languages
			.Where(l => !pageName.StartsWith(l.Code + "/"))
			.Select(l => l.Path)
			.ToList();
		var existingPages = await db.WikiPages
			.ThatAreCurrent()
			.ThatAreNotDeleted()
			.Where(wp => existingLanguagePages.Contains(wp.PageName))
			.Select(wp => wp.PageName)
			.ToListAsync();

		return new(thisPageLang, languages.Where(l => existingPages.Contains(l.Path)).ToList());
	}

	internal async Task<Language?> IsLanguagePage(string? pageName)
	{
		if (string.IsNullOrWhiteSpace(pageName))
		{
			return null;
		}

		string trimmed = pageName.Trim('/');

		if (string.IsNullOrEmpty(trimmed))
		{
			return null;
		}

		var languages = await AvailableLanguages();

		return languages.FirstOrDefault(l => trimmed.StartsWith(l.Code + "/")
			|| l.Code == trimmed);
	}

	internal async Task<IEnumerable<Language>> AvailableLanguages()
	{
		var languagesMarkup = (await wikiPages.SystemPage("Languages"))?.Markup;

		if (string.IsNullOrWhiteSpace(languagesMarkup))
		{
			return [];
		}

		var languages = new List<Language>();

		var rawEntries = languagesMarkup
			.SplitWithEmpty(",")
			.Where(s => !string.IsNullOrWhiteSpace(s));

		foreach (var l in rawEntries)
		{
			var split = l
				.SplitWithEmpty(":")
				.Where(s => !string.IsNullOrWhiteSpace(s))
				.Select(s => s.Trim())
				.ToList();

			if (split.Any())
			{
				languages.Add(new Language(
					split.FirstOrDefault() ?? "",
					split.LastOrDefault() ?? ""));
			}
		}

		return languages;
	}
}

public record Language(string Code, string DisplayName)
{
	public static readonly Language English = new("EN", "English");
}

public record LanguagePage(Language Lang, string Path)
{
	public string Code => Lang.Code;
}

public record TranslationsData(Language ThisPageLang, ICollection<LanguagePage> TrPageList);
