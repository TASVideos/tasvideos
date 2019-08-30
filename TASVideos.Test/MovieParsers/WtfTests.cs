using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using TASVideos.MovieParsers;
using TASVideos.MovieParsers.Parsers;
using TASVideos.MovieParsers.Result;

// ReSharper disable InconsistentNaming
namespace TASVideos.Test.MovieParsers
{
	[TestClass]
	[TestCategory("WtfParsers")]
	public class WtfTests : BaseParserTests
	{
		private Wtf _wtfParser;

		public override string ResourcesPath { get; } = "TASVideos.Test.MovieParsers.WtfSampleFiles.";

		[TestInitialize]
		public void Initialize()
		{
			_wtfParser = new Wtf();
		}

		[TestMethod]
		public void InvalidHeader()
		{
			var result = _wtfParser.Parse(Embedded("wrongheader.wtf"));
			Assert.IsFalse(result.Success);
			AssertNoWarnings(result);
			Assert.IsNotNull(result.Errors);
			Assert.AreEqual(1, result.Errors.Count());
		}

		[TestMethod]
		public void ValidHeader()
		{
			var result = _wtfParser.Parse(Embedded("2frames.wtf"));
			Assert.IsTrue(result.Success);
			AssertNoWarningsOrErrors(result);
		}

		[TestMethod]
		public void System()
		{
			var result = _wtfParser.Parse(Embedded("2frames.wtf"));
			Assert.IsTrue(result.Success);
			AssertNoWarningsOrErrors(result);
			Assert.AreEqual(SystemCodes.Windows, result.SystemCode);
		}

		[TestMethod]
		public void RerecordCount()
		{
			var result = _wtfParser.Parse(Embedded("2frames.wtf"));
			Assert.IsTrue(result.Success);
			AssertNoWarningsOrErrors(result);
			Assert.AreEqual(984, result.RerecordCount);
		}

		[TestMethod]
		public void Ntsc()
		{
			var result = _wtfParser.Parse(Embedded("2frames.wtf"));
			Assert.IsTrue(result.Success);
			AssertNoWarningsOrErrors(result);
			Assert.AreEqual(RegionType.Ntsc, result.Region);
		}

		[TestMethod]
		public void PowerOn()
		{
			var result = _wtfParser.Parse(Embedded("2frames.wtf"));
			Assert.IsTrue(result.Success);
			AssertNoWarningsOrErrors(result);
			Assert.AreEqual(MovieStartType.PowerOn, result.StartType);
		}

		[TestMethod]
		public void FrameRate()
		{
			var result = _wtfParser.Parse(Embedded("2frames.wtf"));
			Assert.IsTrue(result.Success);
			AssertNoWarningsOrErrors(result);
			Assert.IsNotNull(result.FrameRateOverride);
			Assert.IsTrue(FrameRatesAreEqual(61, result.FrameRateOverride.Value));
		}

		[TestMethod]
		public void WhenFrameRateIsZero_NoOverride()
		{
			var result = _wtfParser.Parse(Embedded("noframerate.wtf"));
			Assert.IsTrue(result.Success);
			AssertNoWarningsOrErrors(result);
			Assert.IsNull(result.FrameRateOverride);
		}

		[TestMethod]
		public void Length()
		{
			var result = _wtfParser.Parse(Embedded("2frames.wtf"));
			Assert.IsTrue(result.Success);
			AssertNoWarningsOrErrors(result);
			Assert.AreEqual(2, result.Frames);
		}
	}
}
