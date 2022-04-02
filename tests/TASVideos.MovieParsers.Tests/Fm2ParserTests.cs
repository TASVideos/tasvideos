namespace TASVideos.MovieParsers.Tests;

[TestClass]
[TestCategory("BK2Parsers")]
public class Fm2ParserTests : BaseParserTests
{
	private readonly Fm2 _fm2Parser;
	public override string ResourcesPath { get; } = "TASVideos.MovieParsers.Tests.Fm2SampleFiles.";

	public Fm2ParserTests()
	{
		_fm2Parser = new Fm2();
	}

	[TestMethod]
	public async Task Ntsc()
	{
		var result = await _fm2Parser.Parse(Embedded("ntsc.fm2"), EmbeddedLength("ntsc.fm2"));
		Assert.IsTrue(result.Success, "Result is successful");
		Assert.AreEqual(2, result.Frames, "Frame count should be 2");
		Assert.AreEqual(RegionType.Ntsc, result.Region);
		Assert.AreEqual(21, result.RerecordCount);
		Assert.AreEqual(SystemCodes.Nes, result.SystemCode, "System chould be NES");
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
		Assert.AreEqual(SystemCodes.Nes, result.SystemCode, "System chould be NES");
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
		Assert.AreEqual(SystemCodes.Nes, result.SystemCode, "System chould be NES");
		Assert.AreEqual(MovieStartType.Savestate, result.StartType);
		AssertNoWarningsOrErrors(result);
	}

	[TestMethod]
	public async Task NoRerecords()
	{
		var result = await _fm2Parser.Parse(Embedded("norerecords.fm2"), EmbeddedLength("norerecords.fm2"));
		Assert.IsTrue(result.Success);
		Assert.AreEqual(0, result.RerecordCount, "Rerecord count is assumed to be 0");
		Assert.IsNotNull(result.Warnings);
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
}
