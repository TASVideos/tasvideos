namespace TASVideos.ForumEngineTests;

[TestClass]
public class BbParserTests
{
	private readonly TestWriterHelper _testWriterHelper = new();

	private async Task<string> ParseBbcodeString(string input)
	{
		var elem = BbParser.Parse(input, false, true);
		await using var writer = new StringWriter();
		var htmlWriter = new HtmlWriter(writer);
		await elem.WriteHtml(htmlWriter, _testWriterHelper);
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
		const string expected = """<a href="http://www.example.com" rel="noopener external">Example</a>""";

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

	[TestMethod]
	public async Task Thread_UsesTopicTitle()
	{
		const string topicTitle = "Discussion about Unit Testing";
		_testWriterHelper.SetTopicTitle(topicTitle);
		const string input = "[thread]23745[/thread]";
		const string expected = $"<a href=\"/Forum/Topics/23745\">Thread #23745: {topicTitle}</a>";

		var actual = await ParseBbcodeString(input);
		Assert.AreEqual(expected, actual);
	}

	[TestMethod]
	public async Task Submission_UsesSubmissionTitle()
	{
		const string submission1Title = "Test Sub in 0:04:20.00";
		_testWriterHelper.SetSubmissionTitle(submission1Title);
		const string input = "[submission]1[/submission]";
		const string expected = $"<a href=\"/1S\">{submission1Title}</a>";

		var actual = await ParseBbcodeString(input);
		Assert.AreEqual(expected, actual);
	}

	[TestMethod]
	public async Task Movie_UsesPublicationTitle()
	{
		const string publication1Title = "Test Pub in 0:04:20.00";
		_testWriterHelper.SetPublicationTitle(publication1Title);
		const string input = "[movie]1[/movie]";
		const string expected = $"<a href=\"/1M\">{publication1Title}</a>";

		var actual = await ParseBbcodeString(input);
		Assert.AreEqual(expected, actual);
	}

	[TestMethod]
	public async Task Game_UsesGameTitle()
	{
		const string gameTitle = "Test Game";
		_testWriterHelper.SetGameTitle(gameTitle);
		const string input = "[game]1[/game]";
		const string expected = $"<a href=\"/1G\">{gameTitle}</a>";

		var actual = await ParseBbcodeString(input);
		Assert.AreEqual(expected, actual);
	}

	[TestMethod]
	public async Task GameGroup_UsesGameGroupTitle()
	{
		const string gameGroupTitle = "Test Game Group";
		_testWriterHelper.SetGameGroupTitle(gameGroupTitle);
		const string input = "[gamegroup]1[/gamegroup]";
		const string expected = $"<a href=\"/GameGroups/1\">{gameGroupTitle}</a>";

		var actual = await ParseBbcodeString(input);
		Assert.AreEqual(expected, actual);
	}
}
