namespace TASVideos.MovieParsers.Tests;

[TestClass]
[TestCategory("BK2Parsers")]
public class Fm2ParserTests : BaseParserTests
{
	private readonly Fm2 _fm2Parser = new();
	protected override string ResourcesPath => "TASVideos.MovieParsers.Tests.Fm2SampleFiles.";

	[TestMethod]
	public async Task Ntsc()
	{
		var result = await _fm2Parser.Parse(Embedded("ntsc.fm2"), EmbeddedLength("ntsc.fm2"));
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
		var result = await _fm2Parser.Parse(Embedded("pal.fm2"), EmbeddedLength("pal.fm2"));
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
		var result = await _fm2Parser.Parse(Embedded("fds.fm2"), EmbeddedLength("fds.fm2"));
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
		var result = await _fm2Parser.Parse(Embedded("savestate.fm2"), EmbeddedLength("savestate.fm2"));
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
		var result = await _fm2Parser.Parse(Embedded("norerecords.fm2"), EmbeddedLength("norerecords.fm2"));
		Assert.IsTrue(result.Success);
		Assert.AreEqual(0, result.RerecordCount, "Rerecord count is assumed to be 0");
		Assert.AreEqual(1, result.Warnings.Count());
		AssertNoErrors(result);
	}

	[TestMethod]
	public async Task NegativeRerecords()
	{
		var result = await _fm2Parser.Parse(Embedded("negativererecords.fm2"), EmbeddedLength("negativererecords.fm2"));
		Assert.IsTrue(result.Success);
		Assert.AreEqual(0, result.RerecordCount, "Rerecord count assumed to be 0");
		AssertNoErrors(result);
		Assert.AreEqual(1, result.Warnings.Count());
	}

	[TestMethod]
	public async Task Binary()
	{
		var result = await _fm2Parser.Parse(Embedded("binary.fm2"), EmbeddedLength("binary.fm2"));
		Assert.IsTrue(result.Success, "Result is successful");
		Assert.AreEqual(2, result.Frames, "Frame count should be 2");
		AssertNoWarningsOrErrors(result);
	}

	[TestMethod]
	public async Task BinaryWithoutFrameCount()
	{
		var result = await _fm2Parser.Parse(Embedded("binarynolength.fm2"), EmbeddedLength("binarynolength.fm2"));
		Assert.IsFalse(result.Success);
		AssertNoWarnings(result);
		Assert.AreEqual(1, result.Errors.Count());
	}

	[TestMethod]
	public async Task Hash()
	{
		var result = await _fm2Parser.Parse(Embedded("hash.fm2"), EmbeddedLength("hash.fm2"));
		Assert.AreEqual(1, result.Hashes.Count);
		Assert.AreEqual(HashType.Md5, result.Hashes.First().Key);
		Assert.AreEqual("e9d82f825725c616b0be66ac85dc1b7a", result.Hashes.First().Value);
	}

	[TestMethod]
	public async Task InvalidHash()
	{
		var result = await _fm2Parser.Parse(Embedded("hash-invalid.fm2"), EmbeddedLength("hash-invalid.fm2"));
		Assert.AreEqual(0, result.Hashes.Count);
	}

	[TestMethod]
	public async Task MissingHash()
	{
		var result = await _fm2Parser.Parse(Embedded("hash-missing.fm2"), EmbeddedLength("hash-missing.fm2"));
		Assert.AreEqual(0, result.Hashes.Count);
	}
}
