﻿namespace TASVideos.MovieParsers.Tests;

[TestClass]
[TestCategory("GmvParsers")]
public class GmvTests : BaseParserTests
{
	private readonly Gmv _gmvParser = new();

	protected override string ResourcesPath => "TASVideos.MovieParsers.Tests.GmvSampleFiles.";

	[TestMethod]
	public async Task InvalidHeader()
	{
		var result = await _gmvParser.Parse(Embedded("wrongheader.gmv"), EmbeddedLength("wrongheader.gmv"));
		Assert.IsFalse(result.Success);
		AssertNoWarnings(result);
		Assert.AreEqual(1, result.Errors.Count());
	}

	[TestMethod]
	public async Task ValidHeader()
	{
		var result = await _gmvParser.Parse(Embedded("2frames.gmv"), EmbeddedLength("2frames.gmv"));
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
	}

	[TestMethod]
	public async Task System()
	{
		var result = await _gmvParser.Parse(Embedded("2frames.gmv"), EmbeddedLength("2frames.gmv"));
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
		Assert.AreEqual(SystemCodes.Genesis, result.SystemCode);
	}

	[TestMethod]
	public async Task RerecordCount()
	{
		var result = await _gmvParser.Parse(Embedded("2frames.gmv"), EmbeddedLength("2frames.gmv"));
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
		Assert.AreEqual(10319, result.RerecordCount);
	}

	[TestMethod]
	public async Task Ntsc()
	{
		var result = await _gmvParser.Parse(Embedded("2frames.gmv"), EmbeddedLength("2frames.gmv"));
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
		Assert.AreEqual(RegionType.Ntsc, result.Region);
	}

	[TestMethod]
	public async Task Pal()
	{
		var result = await _gmvParser.Parse(Embedded("pal.gmv"), EmbeddedLength("pal.gmv"));
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
		Assert.AreEqual(RegionType.Pal, result.Region);
	}

	[TestMethod]
	public async Task PowerOn()
	{
		var result = await _gmvParser.Parse(Embedded("2frames.gmv"), EmbeddedLength("2frames.gmv"));
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
		Assert.AreEqual(MovieStartType.PowerOn, result.StartType);
	}

	[TestMethod]
	public async Task Savestate()
	{
		var result = await _gmvParser.Parse(Embedded("savestate.gmv"), EmbeddedLength("savestate.gmv"));
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
		Assert.AreEqual(MovieStartType.Savestate, result.StartType);
	}

	[TestMethod]
	public async Task Length()
	{
		var result = await _gmvParser.Parse(Embedded("2frames.gmv"), EmbeddedLength("2frames.gmv"));
		Assert.IsTrue(result.Success);
		AssertNoWarningsOrErrors(result);
		Assert.AreEqual(2, result.Frames);
	}
}
