using System.IO;
using System.Linq;
using System.Reflection;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using TASVideos.MovieParsers.Parsers;
using TASVideos.MovieParsers.Result;

namespace TASVideos.Test.MovieParsers
{
	[TestClass]
	[TestCategory("BK2Parsers")]
	public class Bk2ParserTests
	{
		private const string Bk2ResourcesPath = "TASVideos.Test.MovieParsers.Bk2SampleFiles.";
		private Bk2 _bk2Parser;

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
			Assert.IsNotNull(result.Errors, "Errors should not be null");
			Assert.IsTrue(result.Errors.Any(), "Must be at least one error");
		}

		[TestMethod]
		public void Frames_CorrectResult()
		{
			var result = _bk2Parser.Parse(Embedded("2Frames.bk2"));
			Assert.AreEqual(true, result.Success, "Parsing must be successful");
			Assert.AreEqual(2, result.Frames, "Result should have 2 frames");
		}

		[TestMethod]
		public void Frames_NoInputFrames_Returns0()
		{
			var result = _bk2Parser.Parse(Embedded("0Frames.bk2"));
			Assert.AreEqual(true, result.Success, "Parsing must be successful");
			Assert.AreEqual(0, result.Frames, "Result should have 0 frames");
		}

		[TestMethod]
		public void RerecordCount_CorrectResult()
		{
			var result = _bk2Parser.Parse(Embedded("RerecordCount1.bk2"));
			Assert.IsTrue(result.Success, "Result is successful");
			Assert.AreEqual(1, result.RerecordCount, "Rerecord count must be 1");
		}

		[TestMethod]
		public void RerecordCount_Missing_Returns0()
		{
			var result = _bk2Parser.Parse(Embedded("RerecordCountMissing.bk2"));
			Assert.IsTrue(result.Success, "Result is successful");
			Assert.IsNotNull(result.Warnings, "Warnings should not be null");
			Assert.IsTrue(result.Warnings.Any(), "Must be at least one warning");
			Assert.AreEqual(0, result.RerecordCount, "Rerecord count is assumed to be 0");
		}

		[TestMethod]
		[DataRow("Pal1.bk2", RegionType.Pal)]
		[DataRow("0Frames.bk2", RegionType.Ntsc, DisplayName = "Missing flag defaults to Ntsc")]
		public void PalFlag_True(string fileName, RegionType expected)
		{
			var result = _bk2Parser.Parse(Embedded(fileName));
			Assert.AreEqual(expected, result.Region);
		}

		[TestMethod]
		[DataRow("Nes.bk2", "nes")]
		[DataRow("Gbc.bk2", "gbc")]
		public void Systems(string filename, string expectedSystem)
		{
			var result = _bk2Parser.Parse(Embedded(filename));
			Assert.IsTrue(result.Success);
			Assert.AreEqual(expectedSystem, result.SystemCode);
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
			Assert.IsTrue(result.Success, "Result is successful");
			Assert.AreEqual("nes", result.SystemCode, "System should be NES");
			Assert.AreEqual(1, result.Frames, "Frame count should be 1");
		}

		private Stream Embedded(string name)
		{
			return Assembly.GetAssembly(typeof(Bk2ParserTests)).GetManifestResourceStream(Bk2ResourcesPath + name);
		}
	}
}
