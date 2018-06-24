using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Reflection;
using System.Text;

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
		public void Frames_CorrectResult()
		{
			var embbedded = Assembly.GetAssembly(typeof(Bk2ParserTests)).GetManifestResourceStream("TASVideos.Test.MovieParsers.SampleMovieFiles.2frames.bk2");
			var result = _bk2Parser.Parse(embbedded);

			Assert.AreEqual(true, result.Success, "Parsing must be successful");
			Assert.AreEqual(2, result.Frames, "REsult should have 2 frames");
		}

		[TestMethod]
		public void Frames_NoInputFrames_Returns0()
		{
			var embbedded = Assembly.GetAssembly(typeof(Bk2ParserTests)).GetManifestResourceStream("TASVideos.Test.MovieParsers.SampleMovieFiles.0frames.bk2");
			var result = _bk2Parser.Parse(embbedded);

			Assert.AreEqual(true, result.Success, "Parsing must be successful");
			Assert.AreEqual(0, result.Frames, "Result should have 0 frames");
		}
	}
}
