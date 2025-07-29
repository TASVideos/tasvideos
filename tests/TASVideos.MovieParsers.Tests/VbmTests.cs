﻿namespace TASVideos.MovieParsers.Tests;

[TestClass]
[TestCategory("VbmParsers")]
public class VbmTests : BaseParserTests
{
	private readonly Vbm _vbmParser = new();

	protected override string ResourcesPath => "TASVideos.MovieParsers.Tests.VbmSampleFiles.";

	[TestMethod]
	public async Task InvalidHeader()
	{
		var result = await _vbmParser.Parse(Embedded("wrongheader.vbm", out var length), length);
		Assert.IsFalse(result.Success);
		AssertNoWarnings(result);
		Assert.AreEqual(1, result.Errors.Count());
	}

	[TestMethod]
	public async Task ValidHeader()
	{
		var result = await _vbmParser.Parse(Embedded("2frames.vbm", out var length), length);
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
	}

	[TestMethod]
	public async Task Ntsc()
	{
		var result = await _vbmParser.Parse(Embedded("2frames.vbm", out var length), length);
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
		Assert.AreEqual(RegionType.Ntsc, result.Region);
	}

	[TestMethod]
	public async Task RerecordCount()
	{
		var result = await _vbmParser.Parse(Embedded("2frames.vbm", out var length), length);
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
		Assert.AreEqual(39098, result.RerecordCount);
	}

	[TestMethod]
	public async Task Length()
	{
		var result = await _vbmParser.Parse(Embedded("2frames.vbm", out var length), length);
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
		Assert.AreEqual(95490, result.Frames);
	}

	[TestMethod]
	public async Task PowerOn()
	{
		var result = await _vbmParser.Parse(Embedded("2frames.vbm", out var length), length);
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
		Assert.AreEqual(MovieStartType.PowerOn, result.StartType);
	}

	[TestMethod]
	public async Task Savestate()
	{
		var result = await _vbmParser.Parse(Embedded("savestate.vbm", out var length), length);
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
		Assert.AreEqual(MovieStartType.Savestate, result.StartType);
	}

	[TestMethod]
	public async Task Sram()
	{
		var result = await _vbmParser.Parse(Embedded("sram.vbm", out var length), length);
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
		Assert.AreEqual(MovieStartType.Sram, result.StartType);
	}

	[TestMethod]
	public async Task Gba()
	{
		var result = await _vbmParser.Parse(Embedded("2frames.vbm", out var length), length);
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
		Assert.AreEqual(SystemCodes.Gba, result.SystemCode);
	}
}
