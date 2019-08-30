using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using TASVideos.MovieParsers;
using TASVideos.MovieParsers.Parsers;
using TASVideos.MovieParsers.Result;

namespace TASVideos.Test.MovieParsers
{
	[TestClass]
	[TestCategory("MarParsers")]
	public class MarTests : BaseParserTests
	{
		private Mar _marParser;

		public override string ResourcesPath { get; } = "TASVideos.Test.MovieParsers.MarSampleFiles.";

		[TestInitialize]
		public void Initialize()
		{
			_marParser = new Mar();
		}

		[TestMethod]
		public void InvalidHeader()
		{
			var result = _marParser.Parse(Embedded("wrongheader.mar"));
			Assert.IsFalse(result.Success);
			AssertNoWarnings(result);
			Assert.IsNotNull(result.Errors);
			Assert.AreEqual(1, result.Errors.Count());
		}

		[TestMethod]
		public void ValidHeader()
		{
			var result = _marParser.Parse(Embedded("2frames.mar"));
			Assert.IsTrue(result.Success);
			AssertNoWarningsOrErrors(result);
		}
	}
}
