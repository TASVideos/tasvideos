namespace TASVideos.MovieParsers.Tests;

[TestClass]
[TestCategory("P2m2Parsers")]
public class P2m2ParserTests : BaseParserTests
{
	private readonly P2m2 _p2m2Parser = new();

	protected override string ResourcesPath => "TASVideos.MovieParsers.Tests.P2m2SampleFiles.";

	[TestMethod]
	public async Task InvalidHeader()
	{
		var result = await _p2m2Parser.Parse(Embedded("wrongheader.p2m2", out var length), length);
		Assert.IsFalse(result.Success);
		AssertNoWarnings(result);
		Assert.AreEqual(1, result.Errors.Count());
	}

	[TestMethod]
	public async Task ValidHeader()
	{
		var result = await _p2m2Parser.Parse(Embedded("2frames.p2m2", out var length), length);
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
	}

	[TestMethod]
	public async Task System()
	{
		var result = await _p2m2Parser.Parse(Embedded("2frames.p2m2", out var length), length);
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
		Assert.AreEqual(SystemCodes.Ps2, result.SystemCode);
	}

	[TestMethod]
	public async Task PowerOn()
	{
		var result = await _p2m2Parser.Parse(Embedded("2frames.p2m2", out var length), length);
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
		Assert.AreEqual(MovieStartType.PowerOn, result.StartType);
	}

	[TestMethod]
	public async Task Savestate()
	{
		var result = await _p2m2Parser.Parse(Embedded("savestate.p2m2", out var length), length);
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
		Assert.AreEqual(MovieStartType.Savestate, result.StartType);
	}

	[TestMethod]
	public async Task RerecordCount()
	{
		var result = await _p2m2Parser.Parse(Embedded("2frames.p2m2", out var length), length);
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
		Assert.AreEqual(1, result.RerecordCount);
	}

	[TestMethod]
	public async Task Frames()
	{
		var result = await _p2m2Parser.Parse(Embedded("2frames.p2m2", out var length), length);
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
		Assert.AreEqual(2, result.Frames);
	}
}
