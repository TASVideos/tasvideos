namespace TASVideos.Core.Services;

public interface ILanguages
{
	Task<bool> IsLanguagePage(string? pageName);
	Task<IEnumerable<LanguagePage>> GetTranslations(string pageName);
}

internal class Languages : ILanguages
{
	private readonly IWikiPages _wikiPages;

	public Languages(IWikiPages wikiPages)
	{
		_wikiPages = wikiPages;
	}

	public async Task<bool> IsLanguagePage(string? pageName)
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

	public async Task<IEnumerable<LanguagePage>> GetTranslations(string pageName)
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

		var existingLanguages = new List<LanguagePage>();
		foreach (var lang in languages)
		{
			if (await _wikiPages.Exists(lang.Path)
				&& !pageName.StartsWith(lang.Code + "/"))
			{
				existingLanguages.Add(lang);
			}
		}

		return existingLanguages;
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