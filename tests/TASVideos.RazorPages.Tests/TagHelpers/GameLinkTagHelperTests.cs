using TASVideos.Extensions;
using TASVideos.TagHelpers;

namespace TASVideos.RazorPages.Tests;

[TestClass]
public sealed class GameLinkTagHelperTests : LinkTagHelperTestsBase
{
	[DataRow(123, "some game", """<a href="/123G">some game</a>""")]
	[TestMethod]
	public async Task TestGameLinkHelper(int id, string label, string expected)
	{
		var generator = TestableHtmlGenerator.Create(out var viewCtx, ServiceCollectionExtensions.Aliases.First(kvp => kvp.Key is "/Games/Index"));
		GameLinkTagHelper gameLinkHelper = new(generator) { Id = id, ViewContext = viewCtx };
		var output = GetOutputObj(contentsUnencoded: label, tagName: "game-link");
		await gameLinkHelper.ProcessAsync(GetHelperContext(), output);
		Assert.AreEqual(expected, GetHtmlString(output));
	}
}
