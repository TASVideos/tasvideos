using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using TASVideos.MovieParsers.Parsers;

namespace TASVideos.Test.MovieParsers
{
	[TestClass]
	[TestCategory("DsmParsers")]
	public class DsmParserTests : BaseParserTests
	{
		private Dsm _dsmParser;
		public override string ResourcesPath { get; } = "TASVideos.Test.MovieParsers.DsmSampleFiles.";

		[TestInitialize]
		public void Initialize()
		{
			_dsmParser = new Dsm();
		}

		[TestMethod]
		public void MultipleFrames()
		{
			var result = _dsmParser.Parse(Embedded("2frames.dsm"));
			Assert.IsTrue(result.Success);
			Assert.AreEqual(2, result.Frames);
			Assert.IsNotNull(result.Errors);
			Assert.AreEqual(0, result.Errors.Count());
		}

		[TestMethod]
		public void ZeroFrames()
		{
			var result = _dsmParser.Parse(Embedded("0frames.dsm"));
			Assert.IsTrue(result.Success);
			Assert.AreEqual(0, result.Frames);
			Assert.IsNotNull(result.Errors);
			Assert.AreEqual(0, result.Errors.Count());
		}

		[TestMethod]
		public void RerecordCount()
		{
			var result = _dsmParser.Parse(Embedded("2frames.dsm"));
			Assert.IsTrue(result.Success);
			Assert.AreEqual(1, result.RerecordCount);
			Assert.IsNotNull(result.Errors);
			Assert.AreEqual(0, result.Errors.Count());
		}

		[TestMethod]
		public void NoRerecordCount()
		{
			var result = _dsmParser.Parse(Embedded("norerecords.dsm"));
			Assert.IsTrue(result.Success);
			Assert.AreEqual(0, result.RerecordCount);
			Assert.IsNotNull(result.Warnings);
			Assert.AreEqual(1, result.Warnings.Count());
			Assert.IsNotNull(result.Errors);
			Assert.AreEqual(0, result.Errors.Count());
		}
	}
}
