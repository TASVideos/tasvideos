using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using TASVideos.MovieParsers;
using TASVideos.MovieParsers.Parsers;
using TASVideos.MovieParsers.Result;

// ReSharper disable InconsistentNaming
namespace TASVideos.Test.MovieParsers
{
	[TestClass]
	[TestCategory("MarParsers")]
	public class MarTests : BaseParserTests
	{
		private Mar _marParser = null!;

		public override string ResourcesPath { get; } = "TASVideos.Test.MovieParsers.MarSampleFiles.";

		[TestInitialize]
		public void Initialize()
		{
			_marParser = new Mar();
		}

		[TestMethod]
		public void InvalidHeader()
		{
			var result = _marParser.Parse(Embedded("wrongheader.mar"));
			Assert.IsFalse(result.Success);
			AssertNoWarnings(result);
			Assert.IsNotNull(result.Errors);
			Assert.AreEqual(1, result.Errors.Count());
		}

		[TestMethod]
		public void ValidHeader()
		{
			var result = _marParser.Parse(Embedded("2frames.mar"));
			Assert.IsTrue(result.Success);
			AssertNoWarningsOrErrors(result);
		}

		[TestMethod]
		public void System()
		{
			var result = _marParser.Parse(Embedded("2frames.mar"));
			Assert.IsTrue(result.Success);
			AssertNoWarningsOrErrors(result);
			Assert.AreEqual(SystemCodes.Arcade, result.SystemCode);
		}

		[TestMethod]
		public void Region()
		{
			var result = _marParser.Parse(Embedded("2frames.mar"));
			Assert.IsTrue(result.Success);
			AssertNoWarningsOrErrors(result);
			Assert.AreEqual(RegionType.Ntsc, result.Region);
		}

		[TestMethod]
		public void RerecordCount()
		{
			var result = _marParser.Parse(Embedded("2frames.mar"));
			Assert.IsTrue(result.Success);
			AssertNoWarningsOrErrors(result);
			Assert.AreEqual(33686018, result.RerecordCount);
		}

		[TestMethod]
		public void Length()
		{
			var result = _marParser.Parse(Embedded("2frames.mar"));
			Assert.IsTrue(result.Success);
			AssertNoWarningsOrErrors(result);
			Assert.AreEqual(16843009, result.Frames);
		}

		[TestMethod]
		public void FrameRate()
		{
			var result = _marParser.Parse(Embedded("2frames.mar"));
			Assert.IsTrue(result.Success);
			AssertNoWarningsOrErrors(result);
			Assert.IsNotNull(result.FrameRateOverride);
			Assert.IsTrue(FrameRatesAreEqual(60.606060606308169, result.FrameRateOverride.Value));
		}

		[TestMethod]
		public void WhenFrameRateIsZero_NoOverride()
		{
			var result = _marParser.Parse(Embedded("noframerate.mar"));
			Assert.IsTrue(result.Success);
			AssertNoWarningsOrErrors(result);
			Assert.IsNull(result.FrameRateOverride);
		}
	}
}
