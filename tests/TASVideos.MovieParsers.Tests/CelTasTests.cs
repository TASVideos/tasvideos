namespace TASVideos.MovieParsers.Tests;

[TestClass]
[TestCategory("CelTasParsers")]
public class CelTasTests : BaseParserTests
{
	private readonly CelTas _celtasParser = new();

	protected override string ResourcesPath => "TASVideos.MovieParsers.Tests.CelTasSampleFiles.";

	[TestMethod]
	[DataRow("missingfiletime.celtas", DisplayName = "Missing FileTime header creates error")]
	[DataRow("improperfiletime.celtas", DisplayName = "Improper FileTime header creates error")]
	public async Task Errors(string filename)
	{
		var result = await _celtasParser.Parse(Embedded(filename, out var length), length);
		Assert.IsFalse(result.Success);
		AssertNoWarnings(result);
		Assert.AreEqual(1, result.Errors.Count());
	}

	[TestMethod]
	public async Task ValidHeader()
	{
		var result = await _celtasParser.Parse(Embedded("2465ms.celtas", out var length), length);
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
	}

	[TestMethod]
	public async Task System()
	{
		var result = await _celtasParser.Parse(Embedded("2465ms.celtas", out var length), length);
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
		Assert.AreEqual(SystemCodes.Celeste, result.SystemCode);
	}

	[TestMethod]
	public async Task RerecordCount()
	{
		var result = await _celtasParser.Parse(Embedded("2465ms.celtas", out var length), length);
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
		Assert.AreEqual(12, result.RerecordCount);
	}

	[TestMethod]
	public async Task TotalRerecordCount()
	{
		var result = await _celtasParser.Parse(Embedded("2465ms_Integrated.celtas", out var length), length);
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
		Assert.AreEqual(15, result.RerecordCount);
	}

	[TestMethod]
	public async Task Length()
	{
		var result = await _celtasParser.Parse(Embedded("2465ms.celtas", out var length), length);
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
		Assert.AreEqual(2465, result.Frames);
	}
}
