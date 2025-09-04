using Microsoft.AspNetCore.Razor.TagHelpers;
using TASVideos.TagHelpers;

namespace TASVideos.RazorPages.Tests.TagHelpers;

[TestClass]
public class WikiLinkTagHelperTests
{
	[TestMethod]
	public void WikiLinkTagHelper_Process_RendersCorrectHtml()
	{
		var tagHelper = new WikiLinkTagHelper { PageName = "GameResources/NES/SuperMarioBros" };
		var context = new TagHelperContext(
			allAttributes: [],
			items: new Dictionary<object, object>(),
			uniqueId: "test");
		var output = new TagHelperOutput(
			tagName: "wiki-link",
			attributes: [],
			getChildContentAsync: (_, _) =>
				Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

		tagHelper.Process(context, output);

		var htmlString = GetHtmlString(output);
		Assert.AreEqual("<a href=\"/GameResources/NES/SuperMarioBros\">GameResources/NES/SuperMarioBros</a>", htmlString);
	}

	private static string GetHtmlString(TagHelperOutput output)
	{
		using var writer = new StringWriter();
		output.WriteTo(writer, NullHtmlEncoder.Default);
		return writer.ToString();
	}
}
