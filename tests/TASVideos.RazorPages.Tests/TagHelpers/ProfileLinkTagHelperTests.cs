using TASVideos.TagHelpers;

namespace TASVideos.RazorPages.Tests;

[TestClass]
public class ProfileLinkTagHelperTests : LinkTagHelperTestsBase
{
	[DataRow("YoshiRulz", "unused", """<a href="/Users/Profile/YoshiRulz">YoshiRulz</a>""")]
	[TestMethod]
	public async Task TestProfileLinkHelper(string username, string label, string expected)
	{
		var generator = TestableHtmlGenerator.Create(out var viewCtx, [new("/Users/Profile", "Users/Profile/{Username}")]);
		ProfileLinkTagHelper profileLinkHelper = new(generator) { Username = username, ViewContext = viewCtx };
		var output = GetOutputObj(contentsUnencoded: label, tagName: "profile-link");
		await profileLinkHelper.ProcessAsync(GetHelperContext(), output);
		Assert.AreEqual(expected, GetHtmlString(output));
	}
}
