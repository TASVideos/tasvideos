using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TASVideos.Services
{
	public interface ILanguage
	{
		Task<IEnumerable<Language>> AvailableLanguages();

		Task<bool> IsLanguagePage(string pageName);
	}

	public class Languages : ILanguage
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

			return languagesMarkup
				.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
				.Select(l =>
				{
					var split = l.Split(":");
					return new Language
					{
						Code = split.FirstOrDefault(),
						DisplayName = split.LastOrDefault()
					};
				})
				.ToList();
		}

		public async Task<bool> IsLanguagePage(string pageName)
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

			return languages.Any(l => trimmed.StartsWith(l + "/"));
		}
	}
}
