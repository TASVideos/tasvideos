using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using TASVideos.MovieParsers;
using TASVideos.MovieParsers.Parsers;
using TASVideos.MovieParsers.Result;

// ReSharper disable InconsistentNaming
namespace TASVideos.Test.MovieParsers
{
	[TestClass]
	[TestCategory("LtmParsers")]
	public class LtmTests : BaseParserTests
	{
		private Ltm _ltmParser = null!;

		public override string ResourcesPath { get; } = "TASVideos.Test.MovieParsers.LtmSampleFiles.";

		[TestInitialize]
		public void Initialize()
		{
			_ltmParser = new Ltm();
		}

		[TestMethod]
		public void Region()
		{
			var result = _ltmParser.Parse(Embedded("2frames.ltm"));

			Assert.IsNotNull(result);
			Assert.IsTrue(result.Success);
			Assert.AreEqual(RegionType.Ntsc, result.Region);
		}

		[TestMethod]
		public void FrameCount()
		{
			var result = _ltmParser.Parse(Embedded("2frames.ltm"));

			Assert.IsNotNull(result);
			Assert.IsTrue(result.Success);
			Assert.AreEqual(SystemCodes.Linux, result.SystemCode);
			Assert.AreEqual(2, result.Frames);
		}

		[TestMethod]
		public void RerecordCount()
		{
			var result = _ltmParser.Parse(Embedded("2frames.ltm"));

			Assert.IsNotNull(result);
			Assert.IsTrue(result.Success);
			Assert.AreEqual(SystemCodes.Linux, result.SystemCode);
			Assert.AreEqual(7, result.RerecordCount);
			AssertNoWarningsOrErrors(result);
		}

		[TestMethod]
		public void FrameRate()
		{
			var result = _ltmParser.Parse(Embedded("2frames.ltm"));

			Assert.IsNotNull(result);
			Assert.IsTrue(result.Success);
			Assert.AreEqual(SystemCodes.Linux, result.SystemCode);
			Assert.AreEqual(120, result.FrameRateOverride);
			AssertNoWarningsOrErrors(result);
		}

		[TestMethod]
		public void MissingFrameRate_Defaults()
		{
			var result = _ltmParser.Parse(Embedded("noframerate.ltm"));

			Assert.IsNotNull(result);
			Assert.IsTrue(result.Success);
			Assert.AreEqual(Ltm.DefaultFrameRate, result.FrameRateOverride);
			AssertNoErrors(result);
			Assert.IsNotNull(result.Warnings);
			Assert.AreEqual(1, result.Warnings.Count());
		}

		[TestMethod]
		public void PowerOn()
		{
			var result = _ltmParser.Parse(Embedded("2frames.ltm"));

			Assert.IsNotNull(result);
			Assert.IsTrue(result.Success);
			Assert.AreEqual(SystemCodes.Linux, result.SystemCode);
			Assert.AreEqual(MovieStartType.PowerOn, result.StartType);
			AssertNoWarningsOrErrors(result);
		}

		[TestMethod]
		public void Savestate()
		{
			var result = _ltmParser.Parse(Embedded("savestate.ltm"));

			Assert.IsNotNull(result);
			Assert.IsTrue(result.Success);
			Assert.AreEqual(SystemCodes.Linux, result.SystemCode);
			Assert.AreEqual(MovieStartType.Savestate, result.StartType);
			AssertNoWarningsOrErrors(result);
		}
	}
}