using TASVideos.Core.Services.Wiki;

namespace TASVideos.Core.Services;

public interface ILanguages
{
	Task<ICollection<LanguagePage>> GetTranslations(string pageName);
}

internal class Languages(ApplicationDbContext db, IWikiPages wikiPages, ICacheService cache) : ILanguages
{
	internal const string TranslationsCacheKey = "Translations-";

	public async Task<ICollection<LanguagePage>> GetTranslations(string pageName)
	{
		var key = TranslationsCacheKey + pageName;
		if (cache.TryGetValue(key, out ICollection<LanguagePage> languages))
		{
			return languages;
		}

		languages = await GetTranslationsInternal(pageName);
		cache.Set(key, languages, Durations.FiveMinutes);
		return languages;
	}

	private async Task<ICollection<LanguagePage>> GetTranslationsInternal(string pageName)
	{
		string subPage = pageName;
		var languages = new List<LanguagePage>();
		bool isTranslation = await IsLanguagePage(pageName);
		if (isTranslation)
		{
			if (!pageName.Contains('/'))
			{
				return [];
			}

			subPage = string.Join("/", pageName.Split('/').Skip(1));

			// Translations should also include the original link to the English version
			languages.Add(new(Language.English, subPage));
		}

		languages.AddRange((await AvailableLanguages())
			.Select(l => new LanguagePage(l, $"{l.Code}/{subPage}"))
			.ToList());

		if (!languages.Any())
		{
			return [];
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

		return languages
			.Where(l => existingPages.Contains(l.Path))
			.ToList();
	}

	internal async Task<bool> IsLanguagePage(string? pageName)
	{
		if (string.IsNullOrWhiteSpace(pageName))
		{
			return false;
		}

		string trimmed = pageName.Trim('/');

		if (string.IsNullOrEmpty(trimmed))
		{
			return false;
		}

		var languages = await AvailableLanguages();

		return languages.Any(l => trimmed.StartsWith(l.Code + "/")
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
