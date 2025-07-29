﻿namespace TASVideos.MovieParsers.Tests;

[TestClass]
[TestCategory("MarParsers")]
public class MarTests : BaseParserTests
{
	private readonly Mar _marParser = new();

	protected override string ResourcesPath => "TASVideos.MovieParsers.Tests.MarSampleFiles.";

	[TestMethod]
	public async Task InvalidHeader()
	{
		var result = await _marParser.Parse(Embedded("wrongheader.mar", out var length), length);
		Assert.IsFalse(result.Success);
		AssertNoWarnings(result);
		Assert.AreEqual(1, result.Errors.Count());
	}

	[TestMethod]
	public async Task ValidHeader()
	{
		var result = await _marParser.Parse(Embedded("2frames.mar", out var length), length);
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
	}

	[TestMethod]
	public async Task System()
	{
		var result = await _marParser.Parse(Embedded("2frames.mar", out var length), length);
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
		Assert.AreEqual(SystemCodes.Arcade, result.SystemCode);
	}

	[TestMethod]
	public async Task Region()
	{
		var result = await _marParser.Parse(Embedded("2frames.mar", out var length), length);
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
		Assert.AreEqual(RegionType.Ntsc, result.Region);
	}

	[TestMethod]
	public async Task RerecordCount()
	{
		var result = await _marParser.Parse(Embedded("2frames.mar", out var length), length);
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
		Assert.AreEqual(33686018, result.RerecordCount);
	}

	[TestMethod]
	public async Task Length()
	{
		var result = await _marParser.Parse(Embedded("2frames.mar", out var length), length);
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
		Assert.AreEqual(16843009, result.Frames);
	}

	[TestMethod]
	public async Task FrameRate()
	{
		var result = await _marParser.Parse(Embedded("2frames.mar", out var length), length);
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
		Assert.IsNotNull(result.FrameRateOverride);
		Assert.IsTrue(FrameRatesAreEqual(60.606060606308169, result.FrameRateOverride!.Value));
	}

	[TestMethod]
	public async Task WhenFrameRateIsZero_NoOverride()
	{
		var result = await _marParser.Parse(Embedded("noframerate.mar", out var length), length);
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
		Assert.IsNull(result.FrameRateOverride);
	}
}
