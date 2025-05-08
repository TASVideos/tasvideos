namespace TASVideos.MovieParsers.Tests;

[TestClass]
[TestCategory("3CTParsers")]
public class ThreeCtParserTests : BaseParserTests
{
	private readonly ThreeCt _threeCtParser = new();
	protected override string ResourcesPath => "TASVideos.MovieParsers.Tests.ThreeCtSampleFiles.";

	[TestMethod]
	public async Task Basic()
	{
		var result = await _threeCtParser.Parse(Embedded("basic.3ct"), EmbeddedLength("basic.3ct"));
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
		Assert.AreEqual(SystemCodes.Nes, result.SystemCode);
		Assert.AreEqual(RegionType.Ntsc, result.Region);
		Assert.AreEqual(0, result.RerecordCount);
		Assert.AreEqual(MovieStartType.PowerOn, result.StartType);
		Assert.AreEqual("3ct", result.FileExtension);
		Assert.AreEqual(30, result.CycleCount);
	}
}
