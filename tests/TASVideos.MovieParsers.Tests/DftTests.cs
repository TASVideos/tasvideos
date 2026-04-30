namespace TASVideos.MovieParsers.Tests;

[TestClass]
[TestCategory("DftParsers")]
public class DftTests : BaseParserTests
{
	private readonly Dft _dftParser = new();

	protected override string ResourcesPath => "TASVideos.MovieParsers.Tests.DftSampleFiles.";

	[TestMethod]
	public async Task NotGz()
	{
		var result = await _dftParser.Parse(Embedded("notgz.dft", out var length), length);
		Assert.IsFalse(result.Success);
		AssertNoWarnings(result);
		Assert.AreEqual(1, result.Errors.Count());
	}

	[TestMethod]
	public async Task NotTar()
	{
		var result = await _dftParser.Parse(Embedded("nottar.dft", out var length), length);
		Assert.IsFalse(result.Success);
		AssertNoWarnings(result);
		Assert.AreEqual(1, result.Errors.Count());
	}

	[TestMethod]
	public async Task Ntsc()
	{
		var result = await _dftParser.Parse(Embedded("2frames.dft", out var length), length);
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
		Assert.AreEqual(RegionType.Ntsc, result.Region);
	}

	[TestMethod]
	public async Task RerecordCount()
	{
		var result = await _dftParser.Parse(Embedded("2frames.dft", out var length), length);
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
		Assert.AreEqual(0, result.RerecordCount);
	}

	[TestMethod]
	public async Task Length()
	{
		var result = await _dftParser.Parse(Embedded("2frames.dft", out var length), length);
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
		Assert.AreEqual(2, result.Frames);
	}

	[TestMethod]
	public async Task PowerOn()
	{
		var result = await _dftParser.Parse(Embedded("2frames.dft", out var length), length);
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
		Assert.AreEqual(MovieStartType.PowerOn, result.StartType);
	}

	[TestMethod]
	public async Task Windows()
	{
		var result = await _dftParser.Parse(Embedded("2frames.dft", out var length), length);
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
		Assert.AreEqual(SystemCodes.Windows, result.SystemCode);
	}

	[TestMethod]
	public async Task Includes()
	{
		var result = await _dftParser.Parse(Embedded("includes.dft", out var length), length);
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
		Assert.AreEqual(6, result.Frames);
	}

	[TestMethod]
	public async Task Recursive()
	{
		var result = await _dftParser.Parse(Embedded("recursive.dft", out var length), length);
		Assert.IsFalse(result.Success);
		AssertNoWarnings(result);
		Assert.AreEqual(1, result.Errors.Count());
	}
}
