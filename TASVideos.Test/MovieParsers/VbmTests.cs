using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using TASVideos.MovieParsers;
using TASVideos.MovieParsers.Parsers;
using TASVideos.MovieParsers.Result;


namespace TASVideos.Test.MovieParsers
{
	[TestClass]
	[TestCategory("VbmParsers")]
	public class VbmTests : BaseParserTests
	{
		private Vbm _vbmParser;

		public override string ResourcesPath { get; } = "TASVideos.Test.MovieParsers.VbmSampleFiles.";

		[TestInitialize]
		public void Initialize()
		{
			_vbmParser = new Vbm();
		}

		[TestMethod]
		public void InvalidHeader()
		{
			var result = _vbmParser.Parse(Embedded("wrongheader.vbm"));
			Assert.IsFalse(result.Success);
			AssertNoWarnings(result);
			Assert.IsNotNull(result.Errors);
			Assert.AreEqual(1, result.Errors.Count());
		}
	}
}
