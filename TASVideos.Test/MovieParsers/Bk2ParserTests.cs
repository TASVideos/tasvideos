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
		private Bk2 _bk2Parser;

		[TestInitialize]
		public void Initialize()
		{
			_bk2Parser = new Bk2();
		}

		[TestMethod]
		public void MissingHeader_ErrorResult()
		{
			var embbedded = Assembly.GetAssembly(typeof(Bk2ParserTests)).GetManifestResourceStream("TASVideos.Test.MovieParsers.SampleMovieFiles.MissingHeader.bk2");
			var result = _bk2Parser.Parse(embbedded);
			Assert.AreEqual(false, result.Success, "Result should not be successfull");
			Assert.IsNotNull(result.Errors, "Errors should not be null");
			Assert.IsTrue(result.Errors.Any(), "Must be at least one error");
		}

		[TestMethod]
		public void MissingInputLog_ErrorResult()
		{
			var embbedded = Assembly.GetAssembly(typeof(Bk2ParserTests)).GetManifestResourceStream("TASVideos.Test.MovieParsers.SampleMovieFiles.MissingInputLog.bk2");
			var result = _bk2Parser.Parse(embbedded);
			Assert.AreEqual(false, result.Success, "Result should not be successfull");
			Assert.IsNotNull(result.Errors, "Errors should not be null");
			Assert.IsTrue(result.Errors.Any(), "Must be at least one error");
		}

		[TestMethod]
		public void Frames_CorrectResult()
		{
			var embbedded = Assembly.GetAssembly(typeof(Bk2ParserTests)).GetManifestResourceStream("TASVideos.Test.MovieParsers.SampleMovieFiles.2Frames.bk2");
			var result = _bk2Parser.Parse(embbedded);

			Assert.AreEqual(true, result.Success, "Parsing must be successful");
			Assert.AreEqual(2, result.Frames, "Result should have 2 frames");
		}

		[TestMethod]
		public void Frames_NoInputFrames_Returns0()
		{
			var embbedded = Assembly.GetAssembly(typeof(Bk2ParserTests)).GetManifestResourceStream("TASVideos.Test.MovieParsers.SampleMovieFiles.0Frames.bk2");
			var result = _bk2Parser.Parse(embbedded);

			Assert.AreEqual(true, result.Success, "Parsing must be successful");
			Assert.AreEqual(0, result.Frames, "Result should have 0 frames");
		}
	}
}
