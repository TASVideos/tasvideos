using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using TASVideos.MovieParsers.Parsers;

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
	}
}
