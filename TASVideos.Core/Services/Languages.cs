using Microsoft.EntityFrameworkCore;
using TASVideos.Data.Entity;

namespace TASVideos.Core.Services;

public interface ILanguages
{
	Task<IEnumerable<LanguagePage>> GetTranslations(string pageName);
}

internal class Languages : ILanguages
{
	internal const string TranslationsCacheKey = "Translations-";
	private readonly IWikiPages _wikiPages;
	private readonly ICacheService _cache;

	public Languages(IWikiPages wikiPages, ICacheService cache)
	{
		_wikiPages = wikiPages;
		_cache = cache;
	}

	public async Task<IEnumerable<LanguagePage>> GetTranslations(string pageName)
	{
		var key = TranslationsCacheKey + pageName;
		if (_cache.TryGetValue(key, out IEnumerable<LanguagePage> languages))
		{
			return languages;
		}

		languages = await GetTranslationsInternal(pageName);
		_cache.Set(key, languages, Durations.FiveMinutesInSeconds);
		return languages;
	}

	private async Task<IEnumerable<LanguagePage>> GetTranslationsInternal(string pageName)
	{
		string subPage = pageName;
		var languages = new List<LanguagePage>();
		bool isTranslation = await IsLanguagePage(pageName);
		if (isTranslation)
		{
			if (!pageName.Contains('/'))
			{
				return Enumerable.Empty<LanguagePage>();
			}

			subPage = string.Join("/", pageName.Split('/').Skip(1));

			// Translations should also include the original link to the English version
			languages.Add(new LanguagePage("EN", "English", subPage));

		}

		languages.AddRange((await AvailableLanguages())
			.Select(l => new LanguagePage(l.Code, l.DisplayName, l.Code + "/" + subPage))
			.ToList());

		if (!languages.Any())
		{
			return Enumerable.Empty<LanguagePage>();
		}

		var existingLanguagePages = languages
			.Where(l => !pageName.StartsWith(l.Code + "/"))
			.Select(l => l.Path)
			.ToList();
		var existingPages = await _wikiPages.Query
			.WithNoChildren()
			.ThatAreNotDeleted()
			.Where(wp => existingLanguagePages.Contains(wp.PageName))
			.Select(wp => wp.PageName)
			.ToListAsync();

		return languages
			.Where(l => existingPages.Contains(l.Path));
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
		var languagesMarkup = (await _wikiPages
				.SystemPage("Languages"))
			?.Markup;

		if (string.IsNullOrWhiteSpace(languagesMarkup))
		{
			return Enumerable.Empty<Language>();
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

internal record Language(string Code, string DisplayName);
public record LanguagePage(string Code, string DisplayName, string Path);