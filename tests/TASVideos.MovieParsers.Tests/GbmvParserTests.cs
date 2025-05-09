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
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
	}

	[TestMethod]
	[DataRow("System-Gbal.gbmv", SystemCodes.Gba)]
	public async Task Systems(string filename, string expectedSystem)
	{
		var result = await _gbmvParser.Parse(Embedded(filename), EmbeddedLength(filename));
		Assert.IsTrue(result.Success);
		Assert.AreEqual(expectedSystem, result.SystemCode);
		AssertNoWarningsOrErrors(result);
	}
}
