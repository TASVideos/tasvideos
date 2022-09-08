namespace TASVideos.MovieParsers.Tests;

[TestClass]
[TestCategory("TasprojParsers")]
public class TasprojParserTests : BaseParserTests
{
	private readonly Tasproj _tasprojParser;
	public override string ResourcesPath { get; } = "TASVideos.MovieParsers.Tests.TasprojSampleFiles.";

	public TasprojParserTests()
	{
		_tasprojParser = new Tasproj();
	}

	[TestMethod]
	public async Task ContainsBranch_NoError()
	{
		var result = await _tasprojParser.Parse(Embedded("branch.tasproj"), EmbeddedLength("branch.tasproj"));

		Assert.AreEqual(true, result.Success);
		AssertNoWarningsOrErrors(result);
	}
}
