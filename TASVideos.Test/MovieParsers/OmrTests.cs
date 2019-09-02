using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using TASVideos.MovieParsers;
using TASVideos.MovieParsers.Parsers;

namespace TASVideos.Test.MovieParsers
{
	[TestClass]
	[TestCategory("OmrParsers")]
	public class OmrTests : BaseParserTests
	{
		private Omr _omrParser;

		public override string ResourcesPath { get; } = "TASVideos.Test.MovieParsers.OmrSampleFiles.";

		[TestInitialize]
		public void Initialize()
		{
			_omrParser = new Omr();
		}

		[TestMethod]
		public void System()
		{
			var result = _omrParser.Parse(Embedded("2frames.omr"));
			Assert.IsTrue(result.Success);
			AssertNoWarningsOrErrors(result);
			Assert.AreEqual(SystemCodes.Msx, result.SystemCode);
		}

		[TestMethod]
		public void Rerecords()
		{
			var result = _omrParser.Parse(Embedded("2frames.omr"));
			Assert.IsTrue(result.Success);
			AssertNoWarningsOrErrors(result);
			Assert.AreEqual(140, result.RerecordCount);
		}
	}
}
