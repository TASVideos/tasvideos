using Microsoft.VisualStudio.TestTools.UnitTesting;

using TASVideos.MovieParsers;
using TASVideos.MovieParsers.Parsers;
using TASVideos.MovieParsers.Result;

namespace TASVideos.Test.MovieParsers
{
	[TestClass]
	[TestCategory("OmrParsers")]
	public class OmrTests : BaseParserTests
	{
		private Omr _omrParser;

		public override string ResourcesPath { get; } = "TASVideos.Test.MovieParsers.OmrSampleFiles.";

		[TestInitialize]
		public void Initialize()
		{
			_omrParser = new Omr();
		}

		[TestMethod]
		public void SystemMsx()
		{
			var result = _omrParser.Parse(Embedded("2seconds.omr"));
			Assert.IsTrue(result.Success);
			AssertNoWarningsOrErrors(result);
			Assert.AreEqual(SystemCodes.Msx, result.SystemCode);
		}

		[TestMethod]
		public void SystemSvi()
		{
			var result = _omrParser.Parse(Embedded("svi.omr"));
			Assert.IsTrue(result.Success);
			AssertNoWarningsOrErrors(result);
			Assert.AreEqual(SystemCodes.Svi, result.SystemCode);
		}

		[TestMethod]
		public void SystemColeco()
		{
			var result = _omrParser.Parse(Embedded("coleco.omr"));
			Assert.IsTrue(result.Success);
			AssertNoWarningsOrErrors(result);
			Assert.AreEqual(SystemCodes.Coleco, result.SystemCode);
		}

		[TestMethod]
		public void Rerecords()
		{
			var result = _omrParser.Parse(Embedded("2seconds.omr"));
			Assert.IsTrue(result.Success);
			AssertNoWarningsOrErrors(result);
			Assert.AreEqual(140, result.RerecordCount);
		}

		[TestMethod]
		public void PowerOn()
		{
			var result = _omrParser.Parse(Embedded("2seconds.omr"));
			Assert.IsTrue(result.Success);
			AssertNoWarningsOrErrors(result);
			Assert.AreEqual(MovieStartType.PowerOn, result.StartType);
		}

		[TestMethod]
		public void Savestate()
		{
			var result = _omrParser.Parse(Embedded("savestate.omr"));
			Assert.IsTrue(result.Success);
			AssertNoWarningsOrErrors(result);
			Assert.AreEqual(MovieStartType.Savestate, result.StartType);
		}

		[TestMethod]
		public void Ntsc()
		{
			var result = _omrParser.Parse(Embedded("2seconds.omr"));
			Assert.IsTrue(result.Success);
			AssertNoWarningsOrErrors(result);
			Assert.AreEqual(RegionType.Ntsc, result.Region);
		}

		[TestMethod]
		public void Pal()
		{
			var result = _omrParser.Parse(Embedded("pal.omr"));
			Assert.IsTrue(result.Success);
			AssertNoWarningsOrErrors(result);
			Assert.AreEqual(RegionType.Pal, result.Region);
		}

		[TestMethod]
		public void Frames()
		{
			var result = _omrParser.Parse(Embedded("2seconds.omr"));
			Assert.IsTrue(result.Success);
			AssertNoWarningsOrErrors(result);
			Assert.AreEqual(120, result.Frames);
		}
	}
}
