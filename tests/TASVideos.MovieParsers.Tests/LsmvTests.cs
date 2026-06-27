namespace TASVideos.MovieParsers.Tests;

[TestClass]
[TestCategory("LsmvParsers")]
public class LsmvTests : BaseParserTests
{
	private readonly Lsmv _lsmvParser = new();

	protected override string ResourcesPath => "TASVideos.MovieParsers.Tests.LsmvSampleFiles.";

	[TestMethod]
	public async Task Errors()
	{
		var result = await _lsmvParser.Parse(Embedded("noinputlog.lsmv", out var length), length);
		Assert.IsFalse(result.Success);
		AssertNoWarnings(result);
		Assert.AreEqual(1, result.Errors.Count());
	}

	[TestMethod]
	public async Task SavestateCheck_Error()
	{
		var result = await _lsmvParser.Parse(Embedded("savestate.lsmv", out var length), length);
		Assert.IsFalse(result.Success);
		AssertNoWarnings(result);
		Assert.AreEqual(1, result.Errors.Count());
	}

	[TestMethod]
	public async Task Frames_WithSubFrames()
	{
		var result = await _lsmvParser.Parse(Embedded("2frameswithsub.lsmv", out var length), length);
		Assert.IsTrue(result.Success);
		Assert.AreEqual(2, result.Frames);
		AssertNoWarningsOrErrors(result);
	}

	[TestMethod]
	public async Task Frames_NoInputFrames_Returns0()
	{
		var result = await _lsmvParser.Parse(Embedded("0frameswithsub.lsmv", out var length), length);
		Assert.IsTrue(result.Success);
		Assert.AreEqual(0, result.Frames);
		AssertNoWarningsOrErrors(result);
	}

	[TestMethod]
	public async Task NoRerecordEntry_Warning()
	{
		var result = await _lsmvParser.Parse(Embedded("norerecordentry.lsmv", out var length), length);
		Assert.IsTrue(result.Success);
		AssertNoErrors(result);
		Assert.AreEqual(1, result.Warnings.Count());
	}

	[TestMethod]
	public async Task EmptyRerecordEntry_Warning()
	{
		var result = await _lsmvParser.Parse(Embedded("emptyrerecordentry.lsmv", out var length), length);
		Assert.IsTrue(result.Success);
		AssertNoErrors(result);
		Assert.AreEqual(1, result.Warnings.Count());
	}

	[TestMethod]
	public async Task InvalidRerecordEntry_Warning()
	{
		var result = await _lsmvParser.Parse(Embedded("invalidrerecordentry.lsmv", out var length), length);
		Assert.IsTrue(result.Success);
		AssertNoErrors(result);
		Assert.AreEqual(1, result.Warnings.Count());
	}

	[TestMethod]
	public async Task ValidRerecordEntry()
	{
		var result = await _lsmvParser.Parse(Embedded("2frameswithsub.lsmv", out var length), length);
		Assert.IsTrue(result.Success);
		Assert.AreEqual(1, result.RerecordCount);
		AssertNoWarningsOrErrors(result);
	}

	[TestMethod]
	public async Task MissingGameType_Error()
	{
		var result = await _lsmvParser.Parse(Embedded("gametype-missing.lsmv", out var length), length);
		Assert.IsFalse(result.Success);
		AssertNoWarnings(result);
		Assert.AreEqual(1, result.Errors.Count());
	}

	[TestMethod]
	public async Task InvalidGameType_DefaultsSnesNtsc()
	{
		var result = await _lsmvParser.Parse(Embedded("gametype-empty.lsmv", out var length), length);
		Assert.IsTrue(result.Success);
		Assert.AreEqual(SystemCodes.Snes, result.SystemCode);
		Assert.AreEqual(RegionType.NTSC, result.Region);
		Assert.AreEqual(2, result.Warnings.Count());
		AssertNoErrors(result);
	}

	[TestMethod]
	[DataRow("gametype-snesntsc.lsmv", SystemCodes.Snes, RegionType.NTSC)]
	[DataRow("gametype-snespal.lsmv", SystemCodes.Snes, RegionType.PAL)]
	[DataRow("gametype-bsx.lsmv", SystemCodes.Snes, RegionType.NTSC)]
	[DataRow("gametype-bsxslotted.lsmv", SystemCodes.Snes, RegionType.NTSC)]
	[DataRow("gametype-sufamiturbo.lsmv", SystemCodes.Snes, RegionType.NTSC)]
	[DataRow("gametype-sgb_ntsc.lsmv", SystemCodes.Sgb, RegionType.NTSC)]
	[DataRow("gametype-sgb_pal.lsmv", SystemCodes.Sgb, RegionType.PAL)]
	[DataRow("gametype-gdmg.lsmv", SystemCodes.GameBoy, RegionType.World)]
	[DataRow("gametype-ggbc.lsmv", SystemCodes.Gbc, RegionType.World)]
	[DataRow("gametype-ggbca.lsmv", SystemCodes.Gbc, RegionType.World)]
	public async Task SystemAndRegion(string file, string expectedSystem, RegionType expectedRegion)
	{
		var result = await _lsmvParser.Parse(Embedded(file, out var length), length);
		Assert.IsTrue(result.Success);
		Assert.AreEqual(expectedSystem, result.SystemCode);
		Assert.AreEqual(expectedRegion, result.Region);
		AssertNoWarningsOrErrors(result);
	}

	[TestMethod]
	[DataRow("2frameswithsub.lsmv", MovieStartType.PowerOn)]
	[DataRow("savestate.anchor.lsmv", MovieStartType.Savestate)]
	[DataRow("moviesram.lsmv", MovieStartType.Sram)]
	[DataRow("moviesram-zerosrm.lsmv", MovieStartType.PowerOn)]
	public async Task StartType(string file, MovieStartType expectedStartType)
	{
		var result = await _lsmvParser.Parse(Embedded(file, out var length), length);
		Assert.IsTrue(result.Success);
		Assert.AreEqual(expectedStartType, result.StartType);
		AssertNoWarningsOrErrors(result);
	}
}
