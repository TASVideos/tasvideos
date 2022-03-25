namespace TASVideos.MovieParsers.Tests;

[TestClass]
[TestCategory("WtfParsers")]
public class WtfTests : BaseParserTests
{
	private readonly Wtf _wtfParser;

	public override string ResourcesPath { get; } = "TASVideos.MovieParsers.Tests.WtfSampleFiles.";

	public WtfTests()
	{
		_wtfParser = new Wtf();
	}

	[TestMethod]
	public async Task InvalidHeader()
	{
		var result = await _wtfParser.Parse(Embedded("wrongheader.wtf"), EmbeddedLength("wrongheader.wtf"));
		Assert.IsFalse(result.Success);
		AssertNoWarnings(result);
		Assert.IsNotNull(result.Errors);
		Assert.AreEqual(1, result.Errors.Count());
	}

	[TestMethod]
	public async Task ValidHeader()
	{
		var result = await _wtfParser.Parse(Embedded("2frames.wtf"), EmbeddedLength("2frames.wtf"));
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
	}

	[TestMethod]
	public async Task System()
	{
		var result = await _wtfParser.Parse(Embedded("2frames.wtf"), EmbeddedLength("2frames.wtf"));
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
		Assert.AreEqual(SystemCodes.Windows, result.SystemCode);
	}

	[TestMethod]
	public async Task RerecordCount()
	{
		var result = await _wtfParser.Parse(Embedded("2frames.wtf"), EmbeddedLength("2frames.wtf"));
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
		Assert.AreEqual(984, result.RerecordCount);
	}

	[TestMethod]
	public async Task Ntsc()
	{
		var result = await _wtfParser.Parse(Embedded("2frames.wtf"), EmbeddedLength("2frames.wtf"));
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
		Assert.AreEqual(RegionType.Ntsc, result.Region);
	}

	[TestMethod]
	public async Task PowerOn()
	{
		var result = await _wtfParser.Parse(Embedded("2frames.wtf"), EmbeddedLength("2frames.wtf"));
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
		Assert.AreEqual(MovieStartType.PowerOn, result.StartType);
	}

	[TestMethod]
	public async Task FrameRate()
	{
		var result = await _wtfParser.Parse(Embedded("2frames.wtf"), EmbeddedLength("2frames.wtf"));
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
		Assert.IsNotNull(result.FrameRateOverride);
		Assert.IsTrue(FrameRatesAreEqual(61, result.FrameRateOverride!.Value));
	}

	[TestMethod]
	public async Task WhenFrameRateIsZero_NoOverride()
	{
		var result = await _wtfParser.Parse(Embedded("noframerate.wtf"), EmbeddedLength("noframerate.wtf"));
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
		Assert.IsNull(result.FrameRateOverride);
	}

	[TestMethod]
	public async Task Length()
	{
		var result = await _wtfParser.Parse(Embedded("2frames.wtf"), EmbeddedLength("2frames.wtf"));
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
		Assert.AreEqual(2, result.Frames);
	}
}
