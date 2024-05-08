namespace TASVideos.MovieParsers.Tests;

[TestClass]
[TestCategory("GbmvParsers")]
public class GbmvParserTests : BaseParserTests
{
	private readonly Gbmv _gbmvParser = new();
	protected override string ResourcesPath => "TASVideos.MovieParsers.Tests.GbmvSampleFiles.";

	[TestMethod]
	public async Task GBAHawk_NewHeaderValues()
	{
		var result = await _gbmvParser.Parse(Embedded("GBAHawk_NewHeaderValues.gbmv"), EmbeddedLength("GBAHawk_NewHeaderValues.gbmv"));
		Assert.AreEqual(true, result.Success);
		AssertNoWarningsOrErrors(result);
	}
}
