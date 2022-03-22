namespace TASVideos.MovieParsers.Tests;

[TestClass]
[TestCategory("LtmParsers")]
public class LtmTests : BaseParserTests
{
	private readonly Ltm _ltmParser;

	public override string ResourcesPath { get; } = "TASVideos.MovieParsers.Tests.LtmSampleFiles.";

	public LtmTests()
	{
		_ltmParser = new Ltm();
	}

	[TestMethod]
	[DataRow("linux.ltm", SystemCodes.Linux)]
	[DataRow("flash.ltm", SystemCodes.Flash)]
	[DataRow("flash-extrachars-linuxfallback.ltm", SystemCodes.Linux)]
	[DataRow("unknown-linuxfallback.ltm", SystemCodes.Linux)]
	[DataRow("windows.ltm", SystemCodes.Windows)]
	[DataRow("dos.ltm", SystemCodes.Dos)]
	public async Task SystemId(string filename, string expectedSystemCode)
	{
		var actual = await _ltmParser.Parse(Embedded(filename), EmbeddedLength(filename));
		Assert.IsNotNull(actual);
		Assert.AreEqual(expectedSystemCode, actual.SystemCode);
	}

	[TestMethod]
	public async Task Region()
	{
		var result = await _ltmParser.Parse(Embedded("2frames.ltm"), EmbeddedLength("2frames.ltm"));

		Assert.IsTrue(result.Success);
		Assert.AreEqual(RegionType.Ntsc, result.Region);
	}

	[TestMethod]
	public async Task FrameCount()
	{
		var result = await _ltmParser.Parse(Embedded("2frames.ltm"), EmbeddedLength("2frames.ltm"));

		Assert.IsTrue(result.Success);
		Assert.AreEqual(SystemCodes.Linux, result.SystemCode);
		Assert.AreEqual(2, result.Frames);
	}

	[TestMethod]
	public async Task RerecordCount()
	{
		var result = await _ltmParser.Parse(Embedded("2frames.ltm"), EmbeddedLength("2frames.ltm"));

		Assert.IsTrue(result.Success);
		Assert.AreEqual(SystemCodes.Linux, result.SystemCode);
		Assert.AreEqual(7, result.RerecordCount);
		AssertNoWarningsOrErrors(result);
	}

	[TestMethod]
	public async Task FrameRate()
	{
		var result = await _ltmParser.Parse(Embedded("2frames.ltm"), EmbeddedLength("2frames.ltm"));

		Assert.IsTrue(result.Success);
		Assert.AreEqual(SystemCodes.Linux, result.SystemCode);
		Assert.AreEqual(120, result.FrameRateOverride);
		AssertNoWarningsOrErrors(result);
	}

	[TestMethod]
	public async Task MissingFrameRate_Defaults()
	{
		var result = await _ltmParser.Parse(Embedded("noframerate.ltm"), EmbeddedLength("noframerate.ltm"));

		Assert.IsTrue(result.Success);
		Assert.AreEqual(Ltm.DefaultFrameRate, result.FrameRateOverride);
		AssertNoErrors(result);
		Assert.IsNotNull(result.Warnings);
		Assert.AreEqual(1, result.Warnings.Count());
	}

	[TestMethod]
	public async Task PowerOn()
	{
		var result = await _ltmParser.Parse(Embedded("2frames.ltm"), EmbeddedLength("2frames.ltm"));

		Assert.IsTrue(result.Success);
		Assert.AreEqual(SystemCodes.Linux, result.SystemCode);
		Assert.AreEqual(MovieStartType.PowerOn, result.StartType);
		AssertNoWarningsOrErrors(result);
	}

	[TestMethod]
	public async Task Savestate()
	{
		var result = await _ltmParser.Parse(Embedded("savestate.ltm"), EmbeddedLength("savestate.ltm"));

		Assert.IsTrue(result.Success);
		Assert.AreEqual(SystemCodes.Linux, result.SystemCode);
		Assert.AreEqual(MovieStartType.Savestate, result.StartType);
		AssertNoWarningsOrErrors(result);
	}

	[TestMethod]
	public async Task VariableFramerate()
	{
		var result = await _ltmParser.Parse(Embedded("variableframerate.ltm"), EmbeddedLength("variableframerate.ltm"));

		Assert.IsTrue(result.Success);
		Assert.AreEqual(SystemCodes.Linux, result.SystemCode);
		Assert.AreEqual(30.002721239119342, result.FrameRateOverride);
		AssertNoWarningsOrErrors(result);
	}
}
