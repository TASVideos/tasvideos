using Microsoft.VisualStudio.TestTools.UnitTesting;
using TASVideos.MovieParsers.Parsers;

namespace TASVideos.Test.MovieParsers
{
	[TestClass]
	[TestCategory("LtmParsers")]
	public class LtmTests : BaseParserTests
	{
		private Ltm _ltmParser;

		public override string ResourcesPath { get; } = "TASVideos.Test.MovieParsers.LtmSampleFiles.";

		[TestInitialize]
		public void Initialize()
		{
			_ltmParser = new Ltm();
		}

		[TestMethod]
		public void FrameCount()
		{
			var result = _ltmParser.Parse(Embedded("2frames.ltm"));

			Assert.IsNotNull(result);
			Assert.IsTrue(result.Success);
			Assert.AreEqual(2, result.Frames);
		}

		[TestMethod]
		public void RerecordCount()
		{
			var result = _ltmParser.Parse(Embedded("2frames.ltm"));

			Assert.IsNotNull(result);
			Assert.IsTrue(result.Success);
			Assert.AreEqual(7, result.RerecordCount);
		}
	}
}