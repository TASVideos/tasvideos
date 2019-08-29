using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using TASVideos.MovieParsers;
using TASVideos.MovieParsers.Parsers;
using TASVideos.MovieParsers.Result;

// ReSharper disable InconsistentNaming
namespace TASVideos.Test.MovieParsers
{
	[TestClass]
	[TestCategory("DtmParsers")]
	public class DtmParserTests : BaseParserTests
	{
		private Dtm _dtmParser;

		public override string ResourcesPath { get; } = "TASVideos.Test.MovieParsers.DtmSampleFiles.";

		[TestInitialize]
		public void Initialize()
		{
			_dtmParser = new Dtm();
		}

		[TestMethod]
		public void InvalidHeader()
		{
			var result = _dtmParser.Parse(Embedded("wrongheader.dtm"));
			Assert.IsFalse(result.Success);
			AssertNoWarnings(result);
			Assert.IsNotNull(result.Errors);
			Assert.AreEqual(1, result.Errors.Count());
		}

		[TestMethod]
		public void ValidHeader()
		{
			var result = _dtmParser.Parse(Embedded("2frames-gc.dtm"));
			Assert.IsTrue(result.Success);
			AssertNoWarningsOrErrors(result);
		}

		[TestMethod]
		public void SystemGc()
		{
			var result = _dtmParser.Parse(Embedded("2frames-gc.dtm"));
			Assert.IsTrue(result.Success);
			AssertNoWarningsOrErrors(result);
			Assert.AreEqual(SystemCodes.GameCube, result.SystemCode);
		}

		[TestMethod]
		public void SystemWii()
		{
			var result = _dtmParser.Parse(Embedded("2frames-wii.dtm"));
			Assert.IsTrue(result.Success);
			AssertNoWarningsOrErrors(result);
			Assert.AreEqual(SystemCodes.Wii, result.SystemCode);
		}

		[TestMethod]
		public void PowerOn()
		{
			var result = _dtmParser.Parse(Embedded("2frames-gc.dtm"));
			Assert.IsTrue(result.Success);
			AssertNoWarningsOrErrors(result);
			Assert.AreEqual(MovieStartType.PowerOn, result.StartType);
		}

		[TestMethod]
		public void Savestate()
		{
			var result = _dtmParser.Parse(Embedded("savestate.dtm"));
			Assert.IsTrue(result.Success);
			AssertNoWarningsOrErrors(result);
			Assert.AreEqual(MovieStartType.Savestate, result.StartType);
		}

		[TestMethod]
		public void Sram()
		{
			var result = _dtmParser.Parse(Embedded("sram.dtm"));
			Assert.IsTrue(result.Success);
			AssertNoWarningsOrErrors(result);
			Assert.AreEqual(MovieStartType.Sram, result.StartType);
		}

		[TestMethod]
		public void RerecordCount()
		{
			var result = _dtmParser.Parse(Embedded("2frames-gc.dtm"));
			Assert.IsTrue(result.Success);
			AssertNoWarningsOrErrors(result);
			Assert.AreEqual(347, result.RerecordCount);
		}

		[TestMethod]
		public void NoTicks_FallbackAndWarn()
		{
			var result = _dtmParser.Parse(Embedded("2frames-legacy.dtm"));
			Assert.IsTrue(result.Success);
			AssertNoErrors(result);
			Assert.IsNotNull(result.Warnings);
			Assert.AreEqual(1, result.Warnings.Count());
			Assert.AreEqual(ParseWarnings.LengthInferred, result.Warnings.Single());
			Assert.AreEqual(2, result.Frames);
		}

		[TestMethod]
		public void GcFrames()
		{
			var result = _dtmParser.Parse(Embedded("2frames-gc.dtm"));
			Assert.IsTrue(result.Success);
			AssertNoWarningsOrErrors(result);
			Assert.AreEqual(240, result.Frames);
		}

		[TestMethod]
		public void WiiFrames()
		{
			var result = _dtmParser.Parse(Embedded("2frames-wii.dtm"));
			Assert.IsTrue(result.Success);
			AssertNoWarningsOrErrors(result);
			Assert.AreEqual(180, result.Frames);
		}
	}
}
