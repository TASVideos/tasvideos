namespace TASVideos.MovieParsers.Tests;

[TestClass]
[TestCategory("M64Parsers")]
public class M64Tests : BaseParserTests
{
	private readonly M64 _m64Parser;

	public override string ResourcesPath { get; } = "TASVideos.MovieParsers.Tests.M64SampleFiles.";

	public M64Tests()
	{
		_m64Parser = new M64();
	}

	[TestMethod]
	public async Task InvalidHeader()
	{
		var result = await _m64Parser.Parse(Embedded("wrongheader.m64"), EmbeddedLength("wrongheader.m64"));
		Assert.IsFalse(result.Success);
		AssertNoWarnings(result);
		Assert.IsNotNull(result.Errors);
		Assert.AreEqual(1, result.Errors.Count());
	}

	[TestMethod]
	public async Task ValidHeader()
	{
		var result = await _m64Parser.Parse(Embedded("2frames.m64"), EmbeddedLength("2frames.m64"));
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
	}

	[TestMethod]
	public async Task System()
	{
		var result = await _m64Parser.Parse(Embedded("2frames.m64"), EmbeddedLength("2frames.m64"));
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
		Assert.AreEqual(SystemCodes.N64, result.SystemCode);
	}

	[TestMethod]
	public async Task RerecordCount()
	{
		var result = await _m64Parser.Parse(Embedded("2frames.m64"), EmbeddedLength("2frames.m64"));
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
		Assert.AreEqual(1, result.RerecordCount);
	}

	[TestMethod]
	public async Task Ntsc()
	{
		var result = await _m64Parser.Parse(Embedded("2frames.m64"), EmbeddedLength("2frames.m64"));
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
		Assert.AreEqual(RegionType.Ntsc, result.Region);
	}

	[TestMethod]
	public async Task Pal()
	{
		var result = await _m64Parser.Parse(Embedded("pal.m64"), EmbeddedLength("pal.m64"));
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
		Assert.AreEqual(RegionType.Pal, result.Region);
	}

	[TestMethod]
	public async Task Length()
	{
		var result = await _m64Parser.Parse(Embedded("2frames.m64"), EmbeddedLength("2frames.m64"));
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
		Assert.AreEqual(2, result.Frames);
	}

	[TestMethod]
	public async Task PowerOn()
	{
		var result = await _m64Parser.Parse(Embedded("2frames.m64"), EmbeddedLength("2frames.m64"));
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
		Assert.AreEqual(MovieStartType.PowerOn, result.StartType);
	}

	[TestMethod]
	public async Task Sram()
	{
		var result = await _m64Parser.Parse(Embedded("sram.m64"), EmbeddedLength("sram.m64"));
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
		Assert.AreEqual(MovieStartType.Sram, result.StartType);
	}

	[TestMethod]
	public async Task Savestate()
	{
		var result = await _m64Parser.Parse(Embedded("savestate.m64"), EmbeddedLength("savestate.m64"));
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
		Assert.AreEqual(MovieStartType.Savestate, result.StartType);
	}
}
