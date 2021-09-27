using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TASVideos.MovieParsers.Parsers;
using TASVideos.MovieParsers.Result;

namespace TASVideos.MovieParsers.Tests
{
	[TestClass]
	[TestCategory("FbmParsers")]
	public class FbmTests : BaseParserTests
	{
		private readonly Fbm _fbmParser;

		public override string ResourcesPath { get; } = "TASVideos.MovieParsers.Tests.FbmSampleFiles.";

		public FbmTests()
		{
			_fbmParser = new Fbm();
		}

		[TestMethod]
		public async Task InvalidHeader()
		{
			var result = await _fbmParser.Parse(Embedded("wrongmarker.fbm"));
			Assert.IsFalse(result.Success);
			AssertNoWarnings(result);
			Assert.IsNotNull(result.Errors);
			Assert.AreEqual(1, result.Errors.Count());
		}

		[TestMethod]
		public async Task NoInputSection()
		{
			var result = await _fbmParser.Parse(Embedded("missinginput.fbm"));
			Assert.IsFalse(result.Success);
			AssertNoWarnings(result);
			Assert.IsNotNull(result.Errors);
			Assert.AreEqual(1, result.Errors.Count());
		}

		[TestMethod]
		public async Task Rerecords()
		{
			var result = await _fbmParser.Parse(Embedded("basictest.fbm"));
			Assert.IsTrue(result.Success);
			AssertNoWarningsOrErrors(result);
			Assert.AreEqual(12298, result.RerecordCount);
		}

		[TestMethod]
		public async Task Frames()
		{
			var result = await _fbmParser.Parse(Embedded("basictest.fbm"));
			Assert.IsTrue(result.Success);
			AssertNoWarningsOrErrors(result);
			Assert.AreEqual(49064, result.Frames);
		}
	}
}
