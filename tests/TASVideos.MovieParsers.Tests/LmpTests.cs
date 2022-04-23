namespace TASVideos.MovieParsers.Tests;

[TestClass]
[TestCategory("LmpParsers")]
public class LmpTests : BaseParserTests
{
	private readonly Lmp _lmpParser;

	public override string ResourcesPath { get; } = "TASVideos.MovieParsers.Tests.LmpSampleFiles.";

	public LmpTests()
	{
		_lmpParser = new Lmp();
	}

	[TestMethod]
	public async Task FileDoesNotEndIn0x80_ReturnsError()
	{
		var embedded = Embedded("doesnotendin80.lmp");
		var length = EmbeddedLength("doesnotendin80.lmp");
		var result = await _lmpParser.Parse(embedded, length);
		Assert.IsNotNull(result);
		Assert.IsFalse(result.Success);
		AssertNoWarnings(result);
		Assert.IsNotNull(result.Errors);
		Assert.AreEqual(1, result.Errors.Count());
	}

	[TestMethod]
	public async Task NewDoom_Success()
	{
		var embedded = Embedded("doom.lmp");
		var length = EmbeddedLength("doom.lmp");
		var result = await _lmpParser.Parse(embedded, length);
		Assert.IsNotNull(result);
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
		Assert.AreEqual(SystemCodes.Doom, result.SystemCode);
		Assert.AreEqual(7071, result.Frames);
		Assert.AreEqual(0, result.RerecordCount, "Lmp does not track rerecords");
	}
}
