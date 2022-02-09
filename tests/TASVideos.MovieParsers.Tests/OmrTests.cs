namespace TASVideos.MovieParsers.Tests;

[TestClass]
[TestCategory("OmrParsers")]
public class OmrTests : BaseParserTests
{
	private readonly Omr _omrParser;

	public override string ResourcesPath { get; } = "TASVideos.MovieParsers.Tests.OmrSampleFiles.";

	public OmrTests()
	{
		_omrParser = new Omr();
	}

	[TestMethod]
	public async Task SystemMsx()
	{
		var result = await _omrParser.Parse(Embedded("2seconds.omr"));
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
		Assert.AreEqual(SystemCodes.Msx, result.SystemCode);
	}

	[TestMethod]
	public async Task SystemSvi()
	{
		var result = await _omrParser.Parse(Embedded("svi.omr"));
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
		Assert.AreEqual(SystemCodes.Svi, result.SystemCode);
	}

	[TestMethod]
	public async Task SystemColeco()
	{
		var result = await _omrParser.Parse(Embedded("coleco.omr"));
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
		Assert.AreEqual(SystemCodes.Coleco, result.SystemCode);
	}

	[TestMethod]
	public async Task Rerecords()
	{
		var result = await _omrParser.Parse(Embedded("2seconds.omr"));
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
		Assert.AreEqual(140, result.RerecordCount);
	}

	[TestMethod]
	public async Task PowerOn()
	{
		var result = await _omrParser.Parse(Embedded("2seconds.omr"));
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
		Assert.AreEqual(MovieStartType.PowerOn, result.StartType);
	}

	[TestMethod]
	public async Task Savestate()
	{
		var result = await _omrParser.Parse(Embedded("savestate.omr"));
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
		Assert.AreEqual(MovieStartType.Savestate, result.StartType);
	}

	[TestMethod]
	public async Task Ntsc()
	{
		var result = await _omrParser.Parse(Embedded("2seconds.omr"));
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
		Assert.AreEqual(RegionType.Ntsc, result.Region);
	}

	[TestMethod]
	public async Task Pal()
	{
		var result = await _omrParser.Parse(Embedded("pal.omr"));
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
		Assert.AreEqual(RegionType.Pal, result.Region);
	}

	[TestMethod]
	public async Task Frames()
	{
		var result = await _omrParser.Parse(Embedded("2seconds.omr"));
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
		Assert.AreEqual(120, result.Frames);
	}
}
