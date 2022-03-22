namespace TASVideos.MovieParsers.Tests;

[TestClass]
[TestCategory("DtmParsers")]
public class DtmParserTests : BaseParserTests
{
	private readonly Dtm _dtmParser;

	public override string ResourcesPath { get; } = "TASVideos.MovieParsers.Tests.DtmSampleFiles.";

	public DtmParserTests()
	{
		_dtmParser = new Dtm();
	}

	[TestMethod]
	public async Task InvalidHeader()
	{
		var result = await _dtmParser.Parse(Embedded("wrongheader.dtm"), EmbeddedLength("wrongheader.dtm"));
		Assert.IsFalse(result.Success);
		AssertNoWarnings(result);
		Assert.IsNotNull(result.Errors);
		Assert.AreEqual(1, result.Errors.Count());
	}

	[TestMethod]
	public async Task ValidHeader()
	{
		var result = await _dtmParser.Parse(Embedded("2frames-gc.dtm"), EmbeddedLength("2frames-gc.dtm"));
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
	}

	[TestMethod]
	public async Task SystemGc()
	{
		var result = await _dtmParser.Parse(Embedded("2frames-gc.dtm"), EmbeddedLength("2frames-gc.dtm"));
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
		Assert.AreEqual(SystemCodes.GameCube, result.SystemCode);
	}

	[TestMethod]
	public async Task SystemWii()
	{
		var result = await _dtmParser.Parse(Embedded("2frames-wii.dtm"), EmbeddedLength("2frames-wii.dtm"));
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
		Assert.AreEqual(SystemCodes.Wii, result.SystemCode);
	}

	[TestMethod]
	public async Task PowerOn()
	{
		var result = await _dtmParser.Parse(Embedded("2frames-gc.dtm"), EmbeddedLength("2frames-gc.dtm"));
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
		Assert.AreEqual(MovieStartType.PowerOn, result.StartType);
	}

	[TestMethod]
	public async Task Savestate()
	{
		var result = await _dtmParser.Parse(Embedded("savestate.dtm"), EmbeddedLength("savestate.dtm"));
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
		Assert.AreEqual(MovieStartType.Savestate, result.StartType);
	}

	[TestMethod]
	public async Task Sram()
	{
		var result = await _dtmParser.Parse(Embedded("sram.dtm"), EmbeddedLength("sram.dtm"));
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
		Assert.AreEqual(MovieStartType.Sram, result.StartType);
	}

	[TestMethod]
	public async Task RerecordCount()
	{
		var result = await _dtmParser.Parse(Embedded("2frames-gc.dtm"), EmbeddedLength("2frames-gc.dtm"));
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
		Assert.AreEqual(347, result.RerecordCount);
	}

	[TestMethod]
	public async Task NoTicks_FallbackAndWarn()
	{
		var result = await _dtmParser.Parse(Embedded("2frames-legacy.dtm"), EmbeddedLength("2frames-legacy.dtm"));
		Assert.IsTrue(result.Success);
		AssertNoErrors(result);
		Assert.IsNotNull(result.Warnings);
		Assert.AreEqual(1, result.Warnings.Count());
		Assert.AreEqual(ParseWarnings.LengthInferred, result.Warnings.Single());
		Assert.AreEqual(2, result.Frames);
	}

	[TestMethod]
	public async Task GcFrames()
	{
		var result = await _dtmParser.Parse(Embedded("2frames-gc.dtm"), EmbeddedLength("2frames-gc.dtm"));
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
		Assert.AreEqual(283, result.Frames);
	}

	[TestMethod]
	public async Task WiiFrames()
	{
		var result = await _dtmParser.Parse(Embedded("2frames-wii.dtm"), EmbeddedLength("2frames-wii.dtm"));
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
		Assert.AreEqual(189, result.Frames);
	}
}
