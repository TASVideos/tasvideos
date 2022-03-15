using TASVideos.Core.Services;
using TASVideos.Data.Entity;

namespace TASVideos.Core.Tests.Services;

[TestClass]
public class LanguagesTests
{
	private const string SystemLanguageMarkup = "FR:French,ES:Español";

	private readonly Mock<IWikiPages> _wikiPages;
	private readonly Languages _languages;

	public LanguagesTests()
	{
		_wikiPages = new Mock<IWikiPages>(MockBehavior.Strict);
		_languages = new Languages(_wikiPages.Object);
	}

	[TestMethod]
	[DataRow(null, false)]
	[DataRow("", false)]
	[DataRow("\r \n \t", false)]
	[DataRow("/", false)]
	[DataRow("ES", true)]
	[DataRow("ES/", true)]
	[DataRow("FRFrontPage", false)]
	[DataRow("ES/FrontPage", true)]
	[DataRow("FrontPage", false)]
	public async Task IsLanguagePage(string pageName, bool expected)
	{
		MockStandardMarkup();
		var actual = await _languages.IsLanguagePage(pageName);
		Assert.AreEqual(expected, actual);
	}

	[TestMethod]
	public async Task AvailableLanguages_NoPage_ReturnsEmptyList()
	{
		_wikiPages
			.Setup(w => w.Page(It.IsAny<string>(), It.IsAny<int?>()))
			.ReturnsAsync((WikiPage?)null);

		var actual = await _languages.AvailableLanguages();

		Assert.IsNotNull(actual);
		Assert.AreEqual(0, actual.Count());
	}

	[TestMethod]
	public async Task AvailableLanguages_NoMarkup_ReturnsEmptyList()
	{
		_wikiPages
			.Setup(w => w.Page(It.IsAny<string>(), It.IsAny<int?>()))
			.ReturnsAsync(new WikiPage { Markup = "" });

		var actual = await _languages.AvailableLanguages();

		Assert.IsNotNull(actual);
		Assert.AreEqual(0, actual.Count());
	}

	[TestMethod]
	public async Task AvailableLanguages_JunkMarkup_ReturnsJunk()
	{
		var junk = "RandomText";
		_wikiPages
			.Setup(w => w.Page(It.IsAny<string>(), It.IsAny<int?>()))
			.ReturnsAsync(new WikiPage { Markup = junk });

		var actual = await _languages.AvailableLanguages();

		Assert.IsNotNull(actual);
		var list = actual.ToList();
		Assert.AreEqual(1, list.Count);
		Assert.AreEqual(junk, list.Single().Code);
		Assert.AreEqual(junk, list.Single().DisplayName);
	}

	[TestMethod]
	public async Task AvailableLanguages_ValidMarkup_ReturnsLanguages()
	{
		MockStandardMarkup();

		var actual = await _languages.AvailableLanguages();

		Assert.IsNotNull(actual);
		var list = actual.ToList();
		Assert.AreEqual(2, list.Count);
		Assert.IsTrue(list.Any(l => l.Code == "FR" && l.DisplayName == "French"));
		Assert.IsTrue(list.Any(l => l.Code == "ES" && l.DisplayName == "Español"));
	}

	[TestMethod]
	public async Task AvailableLanguages_IgnoresTrailingDelimitersAndWhiteSpace()
	{
		var systemLanguageMarkup = @"
				FR : French : ,
				ES : Español , : ";
		_wikiPages
			.Setup(w => w.Page("System/Languages", It.IsAny<int?>()))
			.ReturnsAsync(new WikiPage { Markup = systemLanguageMarkup });

		var actual = await _languages.AvailableLanguages();
		Assert.IsNotNull(actual);
		var list = actual.ToList();
		Assert.AreEqual(2, list.Count);
		Assert.IsTrue(list.Any(l => l.Code == "FR" && l.DisplayName == "French"));
		Assert.IsTrue(list.Any(l => l.Code == "ES" && l.DisplayName == "Español"));
	}

	[TestMethod]
	public async Task GetTranslations_MainPage_NoTranslations_ReturnsEmptyList()
	{
		const string page = "TestPage";
		MockStandardMarkup();
		_wikiPages.Setup(m => m.Exists($"FR/{page}", false)).ReturnsAsync(false);
		_wikiPages.Setup(m => m.Exists($"ES/{page}", false)).ReturnsAsync(false);

		var result = await _languages.GetTranslations(page);
		Assert.IsNotNull(result);
		Assert.AreEqual(0, result.Count());
	}

	[TestMethod]
	public async Task GetTranslations_MainPage_HasTranslations_ReturnsList()
	{
		const string page = "TestPage";
		MockStandardMarkup();
		_wikiPages.Setup(m => m.Exists($"FR/{page}", false)).ReturnsAsync(true);
		_wikiPages.Setup(m => m.Exists($"ES/{page}", false)).ReturnsAsync(true);

		var result = await _languages.GetTranslations(page);
		Assert.IsNotNull(result);
		Assert.AreEqual(2, result.Count());
	}

	[TestMethod]
	public async Task GetTranslation_Translation_ReturnsList()
	{
		const string mainPage = "TestPage";
		const string lang = "FR";
		const string translation = lang + "/" + mainPage;
		MockStandardMarkup();
		_wikiPages.Setup(m => m.Exists(mainPage, false)).ReturnsAsync(true);
		_wikiPages.Setup(m => m.Exists(translation, false)).ReturnsAsync(true);
		_wikiPages.Setup(m => m.Exists($"ES/{mainPage}", false)).ReturnsAsync(true);

		var result = await _languages.GetTranslations(translation);
		Assert.IsNotNull(result);
		var listResult = result.ToList();
		Assert.AreEqual(2, listResult.Count);
		Assert.IsTrue(listResult.Any(r => r.Code == "EN"), "Translation must link to original english page");
		Assert.IsFalse(listResult.Any(r => r.Code == "FR"), "Translation must not link to itself");
	}

	private void MockStandardMarkup()
	{
		_wikiPages
			.Setup(w => w.Page("System/Languages", It.IsAny<int?>()))
			.ReturnsAsync(new WikiPage { Markup = SystemLanguageMarkup });
	}
}
