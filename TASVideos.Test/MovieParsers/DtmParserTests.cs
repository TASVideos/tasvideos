using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using TASVideos.MovieParsers;
using TASVideos.MovieParsers.Parsers;

namespace TASVideos.Test.MovieParsers
{
	[TestClass]
	[TestCategory("DtmParsers")]
	public class DtmParserTests : BaseParserTests
	{
		private Dtm _dtmParser;

		public override string ResourcesPath { get; } = "TASVideos.Test.MovieParsers.DtmSampleFiles.";

		[TestInitialize]
		public void Initialize()
		{
			_dtmParser = new Dtm();
		}

		[TestMethod]
		public void InvalidHeader()
		{
			var result = _dtmParser.Parse(Embedded("wrongheader.dtm"));
			Assert.IsFalse(result.Success);
			AssertNoWarnings(result);
			Assert.IsNotNull(result.Errors);
			Assert.AreEqual(1, result.Errors.Count());
		}

		[TestMethod]
		public void ValidHeader()
		{
			var result = _dtmParser.Parse(Embedded("2frames-gc.dtm"));
			Assert.IsTrue(result.Success);
			AssertNoWarningsOrErrors(result);
		}

		[TestMethod]
		public void SystemGc()
		{
			var result = _dtmParser.Parse(Embedded("2frames-gc.dtm"));
			Assert.IsTrue(result.Success);
			AssertNoWarningsOrErrors(result);
			Assert.AreEqual(SystemCodes.GameCube, result.SystemCode);
		}

		[TestMethod]
		public void SystemWii()
		{
			var result = _dtmParser.Parse(Embedded("2frames-wii.dtm"));
			Assert.IsTrue(result.Success);
			AssertNoWarningsOrErrors(result);
			Assert.AreEqual(SystemCodes.Wii, result.SystemCode);
		}
	}
}
