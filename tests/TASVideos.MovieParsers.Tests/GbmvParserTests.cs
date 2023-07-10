namespace TASVideos.MovieParsers.Tests;

[TestClass]
[TestCategory("GbmvParsers")]
public class GbmvParserTests : BaseParserTests
{
	private readonly Gbmv _gbmvParser;
	public override string ResourcesPath { get; } = "TASVideos.MovieParsers.Tests.GbmvSampleFiles.";

	public GbmvParserTests()
	{
		_gbmvParser = new Gbmv();
	}

	[TestMethod]
	public async Task GBAHawk_NewHeaderValues()
	{
		var result = await _gbmvParser.Parse(Embedded("GBAHawk_NewHeaderValues.gbmv"), EmbeddedLength("GBAHawk_NewHeaderValues.gbmv"));
		Assert.AreEqual(true, result.Success);
		AssertNoWarningsOrErrors(result);
	}
}
