using TASVideos.Extensions;
using TASVideos.TagHelpers;

namespace TASVideos.RazorPages.Tests;

[TestClass]
public class MovieLinkTagHelperTests : LinkTagHelperTestsBase
{
	[DataRow(1234, "some movie", """<a href="/1234M">some movie</a>""")]
	[TestMethod]
	public async Task TestPubLinkHelper(int id, string label, string expected)
	{
		var generator = TestableHtmlGenerator.Create(out var viewCtx, ServiceCollectionExtensions.Aliases.First(kvp => kvp.Key is "/Publications/View"));
		PubLinkTagHelper pubLinkHelper = new(generator) { Id = id, ViewContext = viewCtx };
		var output = GetOutputObj(contentsUnencoded: label, tagName: "pub-link");
		await pubLinkHelper.ProcessAsync(GetHelperContext(), output);
		Assert.AreEqual(expected, GetHtmlString(output));
	}

	[DataRow(1234, "some movie", """<a href="/1234S">some movie</a>""")]
	[TestMethod]
	public async Task TestSubLinkHelper(int id, string label, string expected)
	{
		var generator = TestableHtmlGenerator.Create(out var viewCtx, ServiceCollectionExtensions.Aliases.First(kvp => kvp.Key is "/Submissions/View"));
		SubLinkTagHelper subLinkHelper = new(generator) { Id = id, ViewContext = viewCtx };
		var output = GetOutputObj(contentsUnencoded: label, tagName: "sub-link");
		await subLinkHelper.ProcessAsync(GetHelperContext(), output);
		Assert.AreEqual(expected, GetHtmlString(output));
	}
}
