using TASVideos.Extensions;

namespace TASVideos.MovieParsers.Tests;

[TestClass]
[TestCategory("LtmParsers")]
public class LtmTests : BaseParserTests
{
	private readonly Ltm _ltmParser = new();

	protected override string ResourcesPath => "TASVideos.MovieParsers.Tests.LtmSampleFiles.";

	[TestMethod]
	[DataRow("arcade.ltm", SystemCodes.Arcade)]
	[DataRow("dos.ltm", SystemCodes.Dos)]
	[DataRow("flash.ltm", SystemCodes.Flash)]
	[DataRow("flash-extrachars-linuxfallback.ltm", SystemCodes.Linux)]
	[DataRow("linux.ltm", SystemCodes.Linux)]
	[DataRow("macos.ltm", SystemCodes.MacOs)]
	[DataRow("pc98.ltm", SystemCodes.Pc98)]
	[DataRow("pico8.ltm", SystemCodes.Pico8)]
	[DataRow("ruffle.ltm", SystemCodes.Flash)]
	[DataRow("ruffle-windows-override.ltm", SystemCodes.Windows)]
	[DataRow("unknown-linuxfallback.ltm", SystemCodes.Linux)]
	[DataRow("windows.ltm", SystemCodes.Windows)]
	public async Task SystemId(string filename, string expectedSystemCode)
	{
		var actual = await _ltmParser.Parse(Embedded(filename), EmbeddedLength(filename));
		Assert.IsNotNull(actual);
		Assert.AreEqual(expectedSystemCode, actual.SystemCode);
	}

	[TestMethod]
	public async Task Annotations()
	{
		var result = await _ltmParser.Parse(Embedded("annotations.ltm"), EmbeddedLength("annotations.ltm"));
		Assert.IsTrue(result.Success);
		Assert.IsFalse(string.IsNullOrWhiteSpace(result.Annotations));
		var lines = result.Annotations.SplitWithEmpty("\n");
		Assert.AreEqual(2, lines.Length);
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

	[TestMethod]
	public async Task Hash()
	{
		var result = await _ltmParser.Parse(Embedded("hash.ltm"), EmbeddedLength("hash.ltm"));
		Assert.AreEqual(1, result.Hashes.Count);
		Assert.AreEqual(HashType.Md5, result.Hashes.First().Key);
		Assert.AreEqual("7d66e47fdc0807927c40ce1491c68ad3", result.Hashes.First().Value);
	}

	[TestMethod]
	public async Task NoHash()
	{
		var result = await _ltmParser.Parse(Embedded("no-hash.ltm"), EmbeddedLength("no-hash.ltm"));
		Assert.AreEqual(0, result.Hashes.Count);
	}

	[TestMethod]
	public async Task MissingHash()
	{
		var result = await _ltmParser.Parse(Embedded("missing-hash.ltm"), EmbeddedLength("missing-hash.ltm"));
		Assert.AreEqual(0, result.Hashes.Count);
	}

	[TestMethod]
	public async Task InvalidHash()
	{
		var result = await _ltmParser.Parse(Embedded("invalid-hash.ltm"), EmbeddedLength("invalid-hash.ltm"));
		Assert.AreEqual(0, result.Hashes.Count);
	}
}
