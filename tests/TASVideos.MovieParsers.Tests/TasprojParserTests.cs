namespace TASVideos.MovieParsers.Tests;

[TestClass]
[TestCategory("TasprojParsers")]
public class TasprojParserTests : BaseParserTests
{
	private readonly Tasproj _tasprojParser = new();
	protected override string ResourcesPath => "TASVideos.MovieParsers.Tests.TasprojSampleFiles.";

	[TestMethod]
	public async Task ContainsBranch_NoError()
	{
		var result = await _tasprojParser.Parse(Embedded("branch.tasproj", out var length), length);

		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
	}

	[TestMethod]
	public async Task TopLevelDir()
	{
		var result = await _tasprojParser.Parse(Embedded("TopLevelDir.tasproj", out var length), length);
		Assert.IsTrue(result.Success);
		Assert.AreEqual("snes", result.SystemCode);
		AssertNoErrors(result);
	}
}
