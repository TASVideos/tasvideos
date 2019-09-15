using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using TASVideos.MovieParsers;
using TASVideos.MovieParsers.Parsers;
using TASVideos.MovieParsers.Result;


namespace TASVideos.Test.MovieParsers
{
	[TestClass]
	[TestCategory("VbmParsers")]
	public class VbmTests : BaseParserTests
	{
		private Vbm _vbmParser;

		public override string ResourcesPath { get; } = "TASVideos.Test.MovieParsers.VbmSampleFiles.";

		[TestInitialize]
		public void Initialize()
		{
			_vbmParser = new Vbm();
		}

		[TestMethod]
		public void InvalidHeader()
		{
			var result = _vbmParser.Parse(Embedded("wrongheader.vbm"));
			Assert.IsFalse(result.Success);
			AssertNoWarnings(result);
			Assert.IsNotNull(result.Errors);
			Assert.AreEqual(1, result.Errors.Count());
		}

		[TestMethod]
		public void ValidHeader()
		{
			var result = _vbmParser.Parse(Embedded("2frames.vbm"));
			Assert.IsTrue(result.Success);
			AssertNoWarningsOrErrors(result);
		}

		[TestMethod]
		public void Ntsc()
		{
			var result = _vbmParser.Parse(Embedded("2frames.vbm"));
			Assert.IsTrue(result.Success);
			AssertNoWarningsOrErrors(result);
			Assert.AreEqual(RegionType.Ntsc, result.Region);
		}

		[TestMethod]
		public void RerecordCount()
		{
			var result = _vbmParser.Parse(Embedded("2frames.vbm"));
			Assert.IsTrue(result.Success);
			AssertNoWarningsOrErrors(result);
			Assert.AreEqual(39098, result.RerecordCount);
		}

		[TestMethod]
		public void Length()
		{
			var result = _vbmParser.Parse(Embedded("2frames.vbm"));
			Assert.IsTrue(result.Success);
			AssertNoWarningsOrErrors(result);
			Assert.AreEqual(2, result.Frames);
		}

		[TestMethod]
		public void PowerOn()
		{
			var result = _vbmParser.Parse(Embedded("2frames.vbm"));
			Assert.IsTrue(result.Success);
			AssertNoWarningsOrErrors(result);
			Assert.AreEqual(MovieStartType.PowerOn, result.StartType);
		}

		[TestMethod]
		public void Savestate()
		{
			var result = _vbmParser.Parse(Embedded("savestate.vbm"));
			Assert.IsTrue(result.Success);
			AssertNoWarningsOrErrors(result);
			Assert.AreEqual(MovieStartType.Savestate, result.StartType);
		}

		[TestMethod]
		public void Sram()
		{
			var result = _vbmParser.Parse(Embedded("sram.vbm"));
			Assert.IsTrue(result.Success);
			AssertNoWarningsOrErrors(result);
			Assert.AreEqual(MovieStartType.Sram, result.StartType);
		}

		[TestMethod]
		public void Gba()
		{
			var result = _vbmParser.Parse(Embedded("2frames.vbm"));
			Assert.IsTrue(result.Success);
			AssertNoWarningsOrErrors(result);
			Assert.AreEqual(SystemCodes.Gba, result.SystemCode);
		}
	}
}
