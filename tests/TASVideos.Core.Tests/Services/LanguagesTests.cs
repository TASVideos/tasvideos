using TASVideos.Core.Services.Wiki;
using TASVideos.Data.Entity;

namespace TASVideos.Core.Tests.Services;

[TestClass]
public class LanguagesTests : TestDbBase
{
	private const string SystemLanguageMarkup = "FR:French,ES:Español";

	private readonly IWikiPages _wikiPages;
	private readonly Languages _languages;

	public LanguagesTests()
	{
		_wikiPages = Substitute.For<IWikiPages>();
		_languages = new Languages(_db, _wikiPages, new NoCacheService());
	}

	[TestMethod]
	[DataRow(null, null)]
	[DataRow("", null)]
	[DataRow("\r \n \t", null)]
	[DataRow("/", null)]
	[DataRow("ES", "ES")]
	[DataRow("ES/", "ES")]
	[DataRow("FRFrontPage", null)]
	[DataRow("ES/FrontPage", "ES")]
	[DataRow("FrontPage", null)]
	public async Task IsLanguagePage(string pageName, string? expected)
	{
		MockStandardMarkup();
		var actual = await _languages.IsLanguagePage(pageName);
		Assert.AreEqual(expected, actual?.Code);
	}

	[TestMethod]
	public async Task AvailableLanguages_NoPage_ReturnsEmptyList()
	{
		_wikiPages.Page(Arg.Any<string>()).Returns((IWikiPage?)null);

		var actual = await _languages.AvailableLanguages();

		Assert.IsNotNull(actual);
		Assert.AreEqual(0, actual.Count());
	}

	[TestMethod]
	public async Task AvailableLanguages_NoMarkup_ReturnsEmptyList()
	{
		_wikiPages.Page(Arg.Any<string>()).Returns(new WikiResult { Markup = "" });

		var actual = await _languages.AvailableLanguages();

		Assert.IsNotNull(actual);
		Assert.AreEqual(0, actual.Count());
	}

	[TestMethod]
	public async Task AvailableLanguages_JunkMarkup_ReturnsJunk()
	{
		const string junk = "RandomText";
		_wikiPages.Page(Arg.Any<string>()).Returns(new WikiResult { Markup = junk });

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
		const string systemLanguageMarkup = """
											
											FR : French : ,
											ES : Español , : 
											
											""";
		_wikiPages.Page("System/Languages").Returns(new WikiResult { Markup = systemLanguageMarkup });

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

		var result = await _languages.GetTranslations(page);
		Assert.IsNotNull(result);
		Assert.AreEqual(0, result.TrPageList.Count);
		Assert.AreEqual("EN", result.ThisPageLang.Code);
	}

	[TestMethod]
	public async Task GetTranslations_MainPage_HasTranslations_ReturnsList()
	{
		const string page = "TestPage";
		MockStandardMarkup();
		_db.WikiPages.AddRange(
			new WikiPage { PageName = $"FR/{page}" },
			new WikiPage { PageName = $"ES/{page}" });
		await _db.SaveChangesAsync();

		var result = await _languages.GetTranslations(page);
		Assert.IsNotNull(result);
		Assert.AreEqual(2, result.TrPageList.Count);
		Assert.AreEqual("EN", result.ThisPageLang.Code);
	}

	[TestMethod]
	public async Task GetTranslation_Translation_ReturnsList()
	{
		const string mainPage = "TestPage";
		const string lang = "FR";
		const string translation = lang + "/" + mainPage;
		MockStandardMarkup();

		_db.WikiPages.AddRange(
			new WikiPage { PageName = mainPage },
			new WikiPage { PageName = translation },
			new WikiPage { PageName = $"ES/{mainPage}" });
		await _db.SaveChangesAsync();

		var result = await _languages.GetTranslations(translation);
		Assert.IsNotNull(result);
		var listResult = result.TrPageList;
		Assert.AreEqual(2, listResult.Count);
		Assert.IsTrue(listResult.Any(r => r.Code == "EN"), "Translation must link to original english page");
		Assert.IsFalse(listResult.Any(r => r.Code == "FR"), "Translation must not link to itself");
		Assert.AreEqual(lang, result.ThisPageLang.Code);
	}

	private void MockStandardMarkup()
	{
		_wikiPages.Page("System/Languages").Returns(new WikiResult { Markup = SystemLanguageMarkup });
	}
}
