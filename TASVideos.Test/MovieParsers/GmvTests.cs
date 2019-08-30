using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using TASVideos.MovieParsers;
using TASVideos.MovieParsers.Parsers;
using TASVideos.MovieParsers.Result;

namespace TASVideos.Test.MovieParsers
{
	[TestClass]
	[TestCategory("GmvParsers")]
	public class GmvTests : BaseParserTests
	{
		private Gmv _gmvParser;

		public override string ResourcesPath { get; } = "TASVideos.Test.MovieParsers.GmvSampleFiles.";

		[TestInitialize]
		public void Initialize()
		{
			_gmvParser = new Gmv();
		}

		[TestMethod]
		public void InvalidHeader()
		{
			var result = _gmvParser.Parse(Embedded("wrongheader.gmv"));
			Assert.IsFalse(result.Success);
			AssertNoWarnings(result);
			Assert.IsNotNull(result.Errors);
			Assert.AreEqual(1, result.Errors.Count());
		}

		[TestMethod]
		public void ValidHeader()
		{
			var result = _gmvParser.Parse(Embedded("2frames.gmv"));
			Assert.IsTrue(result.Success);
			AssertNoWarningsOrErrors(result);
		}

		[TestMethod]
		public void System()
		{
			var result = _gmvParser.Parse(Embedded("2frames.gmv"));
			Assert.IsTrue(result.Success);
			AssertNoWarningsOrErrors(result);
			Assert.AreEqual(SystemCodes.Genesis, result.SystemCode);
		}

		[TestMethod]
		public void RerecordCount()
		{
			var result = _gmvParser.Parse(Embedded("2frames.gmv"));
			Assert.IsTrue(result.Success);
			AssertNoWarningsOrErrors(result);
			Assert.AreEqual(10319, result.RerecordCount);
		}

		[TestMethod]
		public void Ntsc()
		{
			var result = _gmvParser.Parse(Embedded("2frames.gmv"));
			Assert.IsTrue(result.Success);
			AssertNoWarningsOrErrors(result);
			Assert.AreEqual(RegionType.Ntsc, result.Region);
		}

		[TestMethod]
		public void Pal()
		{
			var result = _gmvParser.Parse(Embedded("pal.gmv"));
			Assert.IsTrue(result.Success);
			AssertNoWarningsOrErrors(result);
			Assert.AreEqual(RegionType.Pal, result.Region);
		}

		[TestMethod]
		public void PowerOn()
		{
			var result = _gmvParser.Parse(Embedded("2frames.gmv"));
			Assert.IsTrue(result.Success);
			AssertNoWarningsOrErrors(result);
			Assert.AreEqual(MovieStartType.PowerOn, result.StartType);
		}

		[TestMethod]
		public void Savestate()
		{
			var result = _gmvParser.Parse(Embedded("savestate.gmv"));
			Assert.IsTrue(result.Success);
			AssertNoWarningsOrErrors(result);
			Assert.AreEqual(MovieStartType.Savestate, result.StartType);
		}

		[TestMethod]
		public void Length()
		{
			var result = _gmvParser.Parse(Embedded("2frames.gmv"));
			Assert.IsTrue(result.Success);
			AssertNoWarningsOrErrors(result);
			Assert.AreEqual(2, result.Frames);
		}
	}
}
