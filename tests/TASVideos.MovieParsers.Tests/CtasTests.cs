namespace TASVideos.MovieParsers.Tests;

[TestClass]
[TestCategory("CtasParsers")]
public class CtasTests : BaseParserTests
{
	private readonly Ctas _ctasParser = new();

	protected override string ResourcesPath => "TASVideos.MovieParsers.Tests.CtasSampleFiles.";

	[TestMethod]
	public async Task ValidFile()
	{
		var result = await _ctasParser.Parse(Embedded("camptest.ctas", out var length), length);
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
	}

	[TestMethod]
	[DataRow("toofewframes.ctas", DisplayName="File with less input data than framecount")]
	public async Task ErrorFrameCount(string filename)
	{
		var result = await _ctasParser.Parse(Embedded(filename, out var length), length);
		Assert.IsFalse(result.Success);
		AssertNoWarnings(result);
		Assert.AreEqual(1, result.Errors.Count());
	}

	[TestMethod]
	public async Task IncorrectMagic()
	{
		var result = await _ctasParser.Parse(Embedded("wrongmagic.ctas", out var length), length);
		Assert.IsFalse(result.Success);
		AssertNoWarnings(result);
		Assert.AreEqual(1, result.Errors.Count());
	}

	[TestMethod]
	public async Task ImproperlyPadded()
	{
		var result = await _ctasParser.Parse(Embedded("improperly_padded.ctas", out var length), length);
		Assert.IsFalse(result.Success);
		AssertNoWarnings(result);
		Assert.AreEqual(1, result.Errors.Count());
	}
}
