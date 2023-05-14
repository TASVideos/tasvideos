using Microsoft.VisualStudio.TestTools.UnitTesting;
using TASVideos.Common;
using TASVideos.ForumEngine;

namespace TASVideos.ForumEngineTests;

[TestClass]
public class BbParserTests
{
	private static async Task<string> ParseBbcodeString(string input)
	{
		var elem = BbParser.Parse(input, false, true);
		await using var writer = new StringWriter();
		var htmlWriter = new HtmlWriter(writer);
		await elem.WriteHtml(htmlWriter, new TestWriterHelper());
		return writer.ToString();
	}

	[TestMethod]
	public async Task Bold()
	{
		const string input = "[b]Hello, world![/b]";
		const string expected = "<b>Hello, world!</b>";

		var actual = await ParseBbcodeString(input);
		Assert.AreEqual(expected, actual);
	}

	[TestMethod]
	public async Task ItalicText()
	{
		const string input = "[i]Hello, world![/i]";
		const string expected = "<i>Hello, world!</i>";

		var actual = await ParseBbcodeString(input);
		Assert.AreEqual(expected, actual);
	}

	[TestMethod]
	public async Task Underline()
	{
		const string input = "[u]Hello, world![/u]";
		const string expected = "<u>Hello, world!</u>";

		var actual = await ParseBbcodeString(input);
		Assert.AreEqual(expected, actual);
	}

	[TestMethod]
	public async Task Url()
	{
		const string input = "[url=http://www.example.com]Example[/url]";
		const string expected = "<a href=\"http://www.example.com\">Example</a>";

		var actual = await ParseBbcodeString(input);
		Assert.AreEqual(expected, actual);
	}

	[TestMethod]
	public async Task Wiki_ReturnsRelativeLink()
	{
		const string input = "[wiki]ArticleIndex[/wiki]";
		const string expected = "<a href=\"/ArticleIndex\">Wiki: ArticleIndex</a>";

		var actual = await ParseBbcodeString(input);
		Assert.AreEqual(expected, actual);
	}
}
