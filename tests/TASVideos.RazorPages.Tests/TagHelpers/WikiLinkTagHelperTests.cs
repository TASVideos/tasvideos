using TASVideos.TagHelpers;

namespace TASVideos.RazorPages.Tests;

[TestClass]
public sealed class WikiLinkTagHelperTests : LinkTagHelperTestsBase
{
	[DataRow("GameResources/NES/SuperMarioBros", "unused", """<a href="/GameResources/NES/SuperMarioBros">GameResources/NES/SuperMarioBros</a>""")]
	[DataRow("WelcomeToTASVideos", "unused", """<a href="/WelcomeToTASVideos">WelcomeToTASVideos</a>""")]
	[TestMethod]
	public async Task WikiLinkTagHelper_Process_RendersCorrectHtml(string wikiPageName, string label, string expected)
	{
		var tagHelper = new WikiLinkTagHelper { PageName = wikiPageName };
		var context = GetHelperContext();
		var output = GetOutputObj(contentsUnencoded: label, tagName: "wiki-link");

		await tagHelper.ProcessAsync(context, output);

		var htmlString = GetHtmlString(output);
		Assert.AreEqual(expected, htmlString);
	}
}
