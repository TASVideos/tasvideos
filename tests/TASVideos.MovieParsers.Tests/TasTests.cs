namespace TASVideos.MovieParsers.Tests;

[TestClass]
[TestCategory("TasParsers")]
public class TasTests : BaseParserTests
{
	private readonly Tas _tasParser = new();

	protected override string ResourcesPath => "TASVideos.MovieParsers.Tests.TasSampleFiles.";

	[TestMethod]
	[DataRow("missingfiletime.tas", DisplayName = "Missing FileTime header creates error")]
	[DataRow("improperfiletime.tas", DisplayName = "Improper FileTime header creates error")]
	public async Task Errors(string filename)
	{
		var result = await _tasParser.Parse(Embedded(filename, out var length), length);
		Assert.IsFalse(result.Success);
		AssertNoWarnings(result);
		Assert.AreEqual(1, result.Errors.Count());
	}

	[TestMethod]
	public async Task ValidHeader()
	{
		var result = await _tasParser.Parse(Embedded("2465ms.tas", out var length), length);
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
	}

	[TestMethod]
	public async Task System()
	{
		var result = await _tasParser.Parse(Embedded("2465ms.tas", out var length), length);
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
		Assert.AreEqual(SystemCodes.Celeste, result.SystemCode);
	}

	[TestMethod]
	public async Task RerecordCount()
	{
		var result = await _tasParser.Parse(Embedded("2465ms.tas", out var length), length);
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
		Assert.AreEqual(12, result.RerecordCount);
	}

	[TestMethod]
	public async Task TotalRerecordCount()
	{
		var result = await _tasParser.Parse(Embedded("2465ms_Integrated.tas", out var length), length);
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
		Assert.AreEqual(15, result.RerecordCount);
	}

	[TestMethod]
	[DataRow("2465ms.tas")]
	[DataRow("2465ms_ChapterTime.tas")]
	public async Task Length(string filename)
	{
		var result = await _tasParser.Parse(Embedded(filename, out var length), length);
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
		Assert.AreEqual(145, result.Frames);
	}
}
