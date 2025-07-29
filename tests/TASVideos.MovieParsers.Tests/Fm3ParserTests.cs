namespace TASVideos.MovieParsers.Tests;

[TestClass]
[TestCategory("Fm3Parsers")]
public class Fm3ParserTests : BaseParserTests
{
	private readonly Fm3 _fm3Parser = new();
	protected override string ResourcesPath => "TASVideos.MovieParsers.Tests.Fm3SampleFiles.";

	[TestMethod]
	public async Task Ntsc()
	{
		var result = await _fm3Parser.Parse(Embedded("ntsc.fm3", out var length), length);
		Assert.IsTrue(result.Success, "Result is successful");
		Assert.AreEqual(2, result.Frames, "Frame count should be 2");
		Assert.AreEqual(RegionType.Ntsc, result.Region);
		Assert.AreEqual(21, result.RerecordCount);
		Assert.AreEqual(SystemCodes.Nes, result.SystemCode, "System should be NES");
		Assert.AreEqual(MovieStartType.PowerOn, result.StartType);
		AssertNoWarningsOrErrors(result);
	}

	[TestMethod]
	public async Task Pal()
	{
		var result = await _fm3Parser.Parse(Embedded("pal.fm3", out var length), length);
		Assert.IsTrue(result.Success, "Result is successful");
		Assert.AreEqual(2, result.Frames, "Frame count should be 2");
		Assert.AreEqual(RegionType.Pal, result.Region);
		Assert.AreEqual(21, result.RerecordCount);
		Assert.AreEqual(SystemCodes.Nes, result.SystemCode, "System should be NES");
		Assert.AreEqual(MovieStartType.PowerOn, result.StartType);
		AssertNoWarningsOrErrors(result);
	}

	[TestMethod]
	public async Task Fds()
	{
		var result = await _fm3Parser.Parse(Embedded("fds.fm3", out var length), length);
		Assert.IsTrue(result.Success, "Result is successful");
		Assert.AreEqual(2, result.Frames, "Frame count should be 2");
		Assert.AreEqual(RegionType.Ntsc, result.Region);
		Assert.AreEqual(21, result.RerecordCount);
		Assert.AreEqual(SystemCodes.Fds, result.SystemCode, "System should be FDS");
		Assert.AreEqual(MovieStartType.PowerOn, result.StartType);
		AssertNoWarningsOrErrors(result);
	}

	[TestMethod]
	public async Task Savestate()
	{
		var result = await _fm3Parser.Parse(Embedded("savestate.fm3", out var length), length);
		Assert.IsTrue(result.Success, "Result is successful");
		Assert.AreEqual(2, result.Frames, "Frame count should be 2");
		Assert.AreEqual(RegionType.Ntsc, result.Region);
		Assert.AreEqual(21, result.RerecordCount);
		Assert.AreEqual(SystemCodes.Nes, result.SystemCode, "System should be NES");
		Assert.AreEqual(MovieStartType.Savestate, result.StartType);
		AssertNoWarningsOrErrors(result);
	}

	[TestMethod]
	public async Task NoRerecords()
	{
		var result = await _fm3Parser.Parse(Embedded("norerecords.fm3", out var length), length);
		Assert.IsTrue(result.Success);
		Assert.AreEqual(0, result.RerecordCount, "Rerecord count is assumed to be 0");
		Assert.AreEqual(1, result.Warnings.Count());
		AssertNoErrors(result);
	}

	[TestMethod]
	public async Task Binary()
	{
		var result = await _fm3Parser.Parse(Embedded("binary.fm3", out var length), length);
		Assert.IsTrue(result.Success, "Result is successful");
		Assert.AreEqual(2, result.Frames, "Frame count should be 2");
		AssertNoWarningsOrErrors(result);
	}

	[TestMethod]
	public async Task InvalidVersion()
	{
		var result = await _fm3Parser.Parse(Embedded("invalidversion.fm3", out var length), length);
		Assert.IsFalse(result.Success);
		AssertNoWarnings(result);
		Assert.AreEqual(1, result.Errors.Count());
		Assert.IsTrue(result.Errors.First().Contains("Invalid FM3 version"));
	}

	[TestMethod]
	public async Task NoRomFilename()
	{
		var result = await _fm3Parser.Parse(Embedded("noromfilename.fm3", out var length), length);
		Assert.IsFalse(result.Success);
		AssertNoWarnings(result);
		Assert.AreEqual(1, result.Errors.Count());
		Assert.IsTrue(result.Errors.First().Contains("Missing required romFilename field"));
	}

	[TestMethod]
	public async Task NoRomChecksum()
	{
		var result = await _fm3Parser.Parse(Embedded("noromchecksum.fm3", out var length), length);
		Assert.IsFalse(result.Success);
		AssertNoWarnings(result);
		Assert.AreEqual(1, result.Errors.Count());
		Assert.IsTrue(result.Errors.First().Contains("Missing required romChecksum field"));
	}

	[TestMethod]
	public async Task NoGuid()
	{
		var result = await _fm3Parser.Parse(Embedded("noguid.fm3", out var length), length);
		Assert.IsFalse(result.Success);
		AssertNoWarnings(result);
		Assert.AreEqual(1, result.Errors.Count());
		Assert.IsTrue(result.Errors.First().Contains("Missing required guid field"));
	}

	[TestMethod]
	public async Task Hash()
	{
		var result = await _fm3Parser.Parse(Embedded("hash.fm3", out var length), length);
		Assert.IsTrue(result.Success);
		Assert.AreEqual(1, result.Hashes.Count);
		Assert.AreEqual(HashType.Md5, result.Hashes.First().Key);
		Assert.AreEqual("e9d838feee6596dea4b6bb6a7ebb2176", result.Hashes.First().Value);
		AssertNoWarningsOrErrors(result);
	}
}
