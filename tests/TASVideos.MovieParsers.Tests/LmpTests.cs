namespace TASVideos.MovieParsers.Tests;

[TestClass]
[TestCategory("LmpParsers")]
public class LmpTests : BaseParserTests
{
	private readonly Lmp _lmpParser = new();

	protected override string ResourcesPath => "TASVideos.MovieParsers.Tests.LmpSampleFiles.";

	[TestMethod]
	public async Task FileTooShort_ReturnsError()
	{
		var result = await _lmpParser.Parse(Embedded("tooshort.lmp", out var length), length);
		Assert.IsNotNull(result);
		Assert.IsFalse(result.Success);
		AssertNoWarnings(result);
		Assert.AreEqual(1, result.Errors.Count());
	}

	[TestMethod]
	public async Task FileDoesNotEndIn0x80_ReturnsError()
	{
		var result = await _lmpParser.Parse(Embedded("doesnotendin80.lmp", out var length), length);
		Assert.IsNotNull(result);
		Assert.IsFalse(result.Success);
		AssertNoWarnings(result);
		Assert.AreEqual(1, result.Errors.Count());
	}

	[TestMethod]
	public async Task NewDoom_Success()
	{
		var result = await _lmpParser.Parse(Embedded("doom.lmp", out var length), length);
		Assert.IsNotNull(result);
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
		Assert.AreEqual(SystemCodes.Doom, result.SystemCode);
		Assert.AreEqual(7071, result.Frames);
		Assert.AreEqual(0, result.RerecordCount, "Lmp does not track rerecords");
		Assert.IsNull(result.Annotations);
	}

	[TestMethod]
	public async Task Footer_Success()
	{
		var result = await _lmpParser.Parse(Embedded("638-footer.lmp", out var length), length);
		Assert.IsNotNull(result);
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
		Assert.AreEqual(SystemCodes.Doom, result.SystemCode);
		Assert.AreEqual(638, result.Frames);
		Assert.IsNotNull(result.Annotations);
	}
}
