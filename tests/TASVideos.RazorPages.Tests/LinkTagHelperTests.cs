using System.Collections.Immutable;

using Microsoft.AspNetCore.Razor.TagHelpers;

using TASVideos.TagHelpers;

namespace TASVideos.RazorPages.Tests;

[TestClass]
public sealed class LinkTagHelperTests
{
	private static TagHelperContext GetHelperContext()
		=> new(
			[],
			ImmutableDictionary<object, object>.Empty,
			Guid.NewGuid().ToString("N"));

	private static TagHelperOutput GetOutputObj(string contentsUnencoded)
	{
		TagHelperOutput output = new(string.Empty, [], (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));
		output.Content.SetContent(contentsUnencoded);
		return output;
	}

	[DataRow(123, "some game", """<a href="/123G">some game</a>""")]
	[TestMethod]
	public void TestGameLinkHelper(int id, string label, string expected)
	{
		var generator = TestableHtmlGenerator.Create(out var viewCtx, "{Id:int}G");
		GameLinkTagHelper gameLinkHelper = new(generator) { Id = id, ViewContext = viewCtx };
		var output = GetOutputObj(contentsUnencoded: label);
		gameLinkHelper.Process(GetHelperContext(), output);
		Assert.AreEqual(expected, output.GetString());
	}

	[DataRow("YoshiRulz", "unused", """<a href="/Users/Profile/YoshiRulz">YoshiRulz</a>""")]
	[TestMethod]
	public void TestProfileLinkHelper(string username, string label, string expected)
	{
		var generator = TestableHtmlGenerator.Create(out var viewCtx, "Users/Profile/{Username}");
		ProfileLinkTagHelper profileLinkHelper = new(generator) { Username = username, ViewContext = viewCtx };
		var output = GetOutputObj(contentsUnencoded: label);
		profileLinkHelper.Process(GetHelperContext(), output);
		Assert.AreEqual(expected, output.GetString());
	}

	[DataRow(1234, "some movie", """<a href="/1234M">some movie</a>""")]
	[TestMethod]
	public void TestPubLinkHelper(int id, string label, string expected)
	{
		var generator = TestableHtmlGenerator.Create(out var viewCtx, "{Id:int}M");
		PubLinkTagHelper pubLinkHelper = new(generator) { Id = id, ViewContext = viewCtx };
		var output = GetOutputObj(contentsUnencoded: label);
		pubLinkHelper.Process(GetHelperContext(), output);
		Assert.AreEqual(expected, output.GetString());
	}

	[DataRow(1234, "some movie", """<a href="/1234S">some movie</a>""")]
	[TestMethod]
	public void TestSubLinkHelper(int id, string label, string expected)
	{
		var generator = TestableHtmlGenerator.Create(out var viewCtx, "{Id:int}S");
		SubLinkTagHelper subLinkHelper = new(generator) { Id = id, ViewContext = viewCtx };
		var output = GetOutputObj(contentsUnencoded: label);
		subLinkHelper.Process(GetHelperContext(), output);
		Assert.AreEqual(expected, output.GetString());
	}

	[DataRow("WelcomeToTASVideos", "unused", """<a href="/WelcomeToTASVideos">WelcomeToTASVideos</a>""")]
	[TestMethod]
	public void TestWikiLinkHelper(string wikiPageName, string label, string expected)
	{
		var generator = TestableHtmlGenerator.Create(out var viewCtx, "{PageName}");
		WikiLinkTagHelper wikiLinkHelper = new(generator) { PageName = wikiPageName, ViewContext = viewCtx };
		var output = GetOutputObj(contentsUnencoded: label);
		wikiLinkHelper.Process(GetHelperContext(), output);
		Assert.AreEqual(expected, output.GetString());
	}
}
