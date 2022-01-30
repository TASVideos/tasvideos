namespace TASVideos.Core.Services;

public interface ILanguages
{
	Task<IEnumerable<Language>> AvailableLanguages();

	Task<bool> IsLanguagePage(string pageName);
}

internal class Languages : ILanguages
{
	private readonly IWikiPages _wikiPages;

	public Languages(IWikiPages wikiPages)
	{
		_wikiPages = wikiPages;
	}

	public async Task<IEnumerable<Language>> AvailableLanguages()
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
}

public record Language(string Code, string DisplayName);
