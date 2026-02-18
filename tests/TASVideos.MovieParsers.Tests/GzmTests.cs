using System.ComponentModel;

namespace TASVideos.MovieParsers.Tests;

[TestClass]
[TestCategory("GzmParsers")]
public class GzmTests : BaseParserTests
{
	private readonly Gzm _gzmParser = new();

	protected override string ResourcesPath => "TASVideos.MovieParsers.Tests.GzmSampleFiles.";

	[TestMethod]
	public async Task ValidFile()
	{
		var result = await _gzmParser.Parse(Embedded("test.gzm", out var length), length);
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
	}

	[TestMethod]
	[DataRow("toofewframes.gzm", DisplayName="File with less input data than nInputs")]
	[DataRow("toomanyframes.gzm", DisplayName="File with more input data than nInputs")]
	public async Task ErrorFrameCount(string filename)
	{
		var result = await _gzmParser.Parse(Embedded(filename, out var length), length);
		Assert.IsFalse(result.Success);
		AssertNoWarnings(result);
		Assert.AreEqual(1, result.Errors.Count());
	}

	[TestMethod]
	public async Task ExtraDataFile()
	{
		var result = await _gzmParser.Parse(Embedded("ocadata.gzm", out var length), length);
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
	}

	[TestMethod]
	public async Task MismatchSeedFile()
	{
		var result = await _gzmParser.Parse(Embedded("mismatchedseed.gzm", out var length), length);
		Assert.IsFalse(result.Success);
		AssertNoWarnings(result);
		Assert.AreEqual(1, result.Errors.Count());
	}
}
