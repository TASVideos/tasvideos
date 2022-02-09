using TASVideos.Core.Services;
using TASVideos.Data.Entity;

namespace TASVideos.Core.Tests.Services;

[TestClass]
public class LanguagesTests
{
	private const string SystemLanguageMarkup = "EN:English,ES:Español";

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
	[DataRow("ENFrontPage", false)]
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
		Assert.IsTrue(list.Any(l => l.Code == "EN" && l.DisplayName == "English"));
		Assert.IsTrue(list.Any(l => l.Code == "ES" && l.DisplayName == "Español"));
	}

	[TestMethod]
	public async Task AvailableLanguages_IgnoresTrailingDelimitersAndWhiteSpace()
	{
		var systemLanguageMarkup = @"
				EN : English : ,
				ES : Español , : ";
		_wikiPages
			.Setup(w => w.Page("System/Languages", It.IsAny<int?>()))
			.ReturnsAsync(new WikiPage { Markup = systemLanguageMarkup });

		var actual = await _languages.AvailableLanguages();
		Assert.IsNotNull(actual);
		var list = actual.ToList();
		Assert.AreEqual(2, list.Count);
		Assert.IsTrue(list.Any(l => l.Code == "EN" && l.DisplayName == "English"));
		Assert.IsTrue(list.Any(l => l.Code == "ES" && l.DisplayName == "Español"));
	}

	private void MockStandardMarkup()
	{
		_wikiPages
			.Setup(w => w.Page("System/Languages", It.IsAny<int?>()))
			.ReturnsAsync(new WikiPage { Markup = SystemLanguageMarkup });
	}
}
