namespace TASVideos.MovieParsers.Tests;

[TestClass]
[TestCategory("CtmParsers")]
public class CtmTests : BaseParserTests
{
	private readonly Ctm _ctmParser = new();

	protected override string ResourcesPath => "TASVideos.MovieParsers.Tests.CtmSampleFiles.";

	[TestMethod]
	public async Task InvalidHeader()
	{
		var result = await _ctmParser.Parse(Embedded("wrongheader.ctm"), EmbeddedLength("wrongheader.ctm"));
		Assert.IsFalse(result.Success);
		AssertNoWarnings(result);
		Assert.AreEqual(1, result.Errors.Count());
	}

	[TestMethod]
	public async Task ValidHeader()
	{
		var result = await _ctmParser.Parse(Embedded("2frames.ctm"), EmbeddedLength("2frames.ctm"));
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
	}

	[TestMethod]
	public async Task System()
	{
		var result = await _ctmParser.Parse(Embedded("2frames.ctm"), EmbeddedLength("2frames.ctm"));
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
		Assert.AreEqual(SystemCodes.N3ds, result.SystemCode);
	}

	[TestMethod]
	public async Task RerecordCount()
	{
		var result = await _ctmParser.Parse(Embedded("2frames.ctm"), EmbeddedLength("2frames.ctm"));
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
		Assert.AreEqual(1, result.RerecordCount);
	}

	[TestMethod]
	public async Task Ntsc()
	{
		var result = await _ctmParser.Parse(Embedded("2frames.ctm"), EmbeddedLength("2frames.ctm"));
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
		Assert.AreEqual(RegionType.Ntsc, result.Region);
	}

	[TestMethod]
	public async Task Length()
	{
		var result = await _ctmParser.Parse(Embedded("2frames.ctm"), EmbeddedLength("2frames.ctm"));
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
		Assert.AreEqual(2, result.Frames);
	}
}
