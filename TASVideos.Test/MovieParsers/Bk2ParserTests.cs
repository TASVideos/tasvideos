using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using TASVideos.MovieParsers.Parsers;
using TASVideos.MovieParsers.Result;

// ReSharper disable InconsistentNaming
namespace TASVideos.Test.MovieParsers
{
	[TestClass]
	[TestCategory("BK2Parsers")]
	public class Bk2ParserTests : BaseParserTests
	{
		private Bk2 _bk2Parser;
		public override string ResourcesPath { get; } = "TASVideos.Test.MovieParsers.Bk2SampleFiles.";

		[TestInitialize]
		public void Initialize()
		{
			_bk2Parser = new Bk2();
		}

		[TestMethod]
		[DataRow("MissingHeader.bk2", DisplayName = "Missing Header creates error")]
		[DataRow("MissingInputLog.bk2", DisplayName = "Missing Header creates error")]
		public void Errors(string filename)
		{
			var result = _bk2Parser.Parse(Embedded(filename));
			Assert.AreEqual(false, result.Success, "Result should not be successfull");
			AssertNoWarnings(result);
			Assert.IsNotNull(result.Errors);
			Assert.AreEqual(1, result.Errors.Count());
		}

		[TestMethod]
		public void Frames_CorrectResult()
		{
			var result = _bk2Parser.Parse(Embedded("2Frames.bk2"));
			Assert.AreEqual(true, result.Success);
			Assert.AreEqual(2, result.Frames);
			AssertNoWarningsOrErrors(result);
		}

		[TestMethod]
		public void Frames_NoInputFrames_Returns0()
		{
			var result = _bk2Parser.Parse(Embedded("0Frames.bk2"));
			Assert.AreEqual(true, result.Success);
			Assert.AreEqual(0, result.Frames);
			AssertNoWarningsOrErrors(result);
		}

		[TestMethod]
		public void RerecordCount_CorrectResult()
		{
			var result = _bk2Parser.Parse(Embedded("RerecordCount1.bk2"));
			Assert.IsTrue(result.Success);
			Assert.AreEqual(1, result.RerecordCount);
			AssertNoWarningsOrErrors(result);
		}

		[TestMethod]
		public void RerecordCount_Missing_Returns0()
		{
			var result = _bk2Parser.Parse(Embedded("RerecordCountMissing.bk2"));
			Assert.IsTrue(result.Success);
			Assert.IsNotNull(result.Warnings);
			Assert.AreEqual(1, result.Warnings.Count());
			Assert.AreEqual(0, result.RerecordCount, "Rerecord count is assumed to be 0");
			AssertNoErrors(result);
		}

		[TestMethod]
		[DataRow("Pal1.bk2", RegionType.Pal)]
		[DataRow("0Frames.bk2", RegionType.Ntsc, DisplayName = "Missing flag defaults to Ntsc")]
		public void PalFlag_True(string fileName, RegionType expected)
		{
			var result = _bk2Parser.Parse(Embedded(fileName));
			Assert.AreEqual(expected, result.Region);
			AssertNoWarningsOrErrors(result);
		}

		[TestMethod]
		[DataRow("Nes.bk2", "nes")]
		[DataRow("Gbc.bk2", "gbc")]
		public void Systems(string filename, string expectedSystem)
		{
			var result = _bk2Parser.Parse(Embedded(filename));
			Assert.IsTrue(result.Success);
			Assert.AreEqual(expectedSystem, result.SystemCode);
			AssertNoWarningsOrErrors(result);
		}

		[TestMethod]
		[DataRow("Nes.bk2", MovieStartType.PowerOn)]
		[DataRow("sram.bk2", MovieStartType.Sram)]
		[DataRow("savestate.bk2", MovieStartType.Savestate)]
		public void StartsFrom_PowerOn(string filename, MovieStartType expected)
		{
			var result = _bk2Parser.Parse(Embedded(filename));
			Assert.IsTrue(result.Success);
			Assert.AreEqual(expected, result.StartType);
		}

		[TestMethod]
		public void InnerFileExtensions_AreNotChecked()
		{
			var result = _bk2Parser.Parse(Embedded("NoFileExts.bk2"));
			Assert.IsTrue(result.Success);
			Assert.AreEqual("nes", result.SystemCode);
			Assert.AreEqual(1, result.Frames);
			AssertNoWarningsOrErrors(result);
		}

		[TestMethod]
		public void SubNes_ReportsCorrectFrameCount()
		{
			var result = _bk2Parser.Parse(Embedded("SubNes.bk2"));
			Assert.IsTrue(result.Success);
			Assert.AreEqual("nes", result.SystemCode);
			Assert.AreEqual(12, result.Frames);
			AssertNoWarningsOrErrors(result);
		}

		[TestMethod]
		public void SubNes_MissingVBlank_Error()
		{
			var result = _bk2Parser.Parse(Embedded("SubNesMissingVBlank.bk2"));

			Assert.IsFalse(result.Success);
			Assert.IsNotNull(result.Errors);
			Assert.IsTrue(result.Errors.Any());
		}
	}
}
