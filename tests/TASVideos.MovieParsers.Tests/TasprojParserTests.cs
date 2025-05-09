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
		var result = await _tasprojParser.Parse(Embedded("branch.tasproj"), EmbeddedLength("branch.tasproj"));

		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
	}
}
