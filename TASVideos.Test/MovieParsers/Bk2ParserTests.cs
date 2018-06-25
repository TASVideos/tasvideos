using System.IO;
using System.Linq;
using System.Reflection;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using TASVideos.MovieParsers;

namespace TASVideos.Test.MovieParsers
{
    [TestClass]
	[TestCategory("BK2Parsers")]
	public class Bk2ParserTests
    {
		private const string Bk2ResourcesPath = "TASVideos.Test.MovieParsers.SampleMovieFiles.";
		private Bk2 _bk2Parser;

		[TestInitialize]
		public void Initialize()
		{
			_bk2Parser = new Bk2();
		}

		[TestMethod]
		public void MissingHeader_ErrorResult()
		{
			var result = _bk2Parser.Parse(Embedded("MissingHeader.bk2"));
			Assert.AreEqual(false, result.Success, "Result should not be successfull");
			Assert.IsNotNull(result.Errors, "Errors should not be null");
			Assert.IsTrue(result.Errors.Any(), "Must be at least one error");
		}

		[TestMethod]
		public void MissingInputLog_ErrorResult()
		{
			var result = _bk2Parser.Parse(Embedded("MissingInputLog.bk2"));
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
		public void System_Nes()
		{
			var result = _bk2Parser.Parse(Embedded("Nes.bk2"));
			Assert.IsTrue(result.Success, "Result is successful");
			Assert.AreEqual("nes", result.SystemCode, "System should be NES");
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

		private Stream Embedded(string name)
		{
			return Assembly.GetAssembly(typeof(Bk2ParserTests)).GetManifestResourceStream(Bk2ResourcesPath + name);
		}
	}
}
