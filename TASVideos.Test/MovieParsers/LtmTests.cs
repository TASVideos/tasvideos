using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using TASVideos.MovieParsers;
using TASVideos.MovieParsers.Parsers;
using TASVideos.MovieParsers.Result;

// ReSharper disable InconsistentNaming
namespace TASVideos.Test.MovieParsers
{
	[TestClass]
	[TestCategory("LtmParsers")]
	public class LtmTests : BaseParserTests
	{
		private Ltm _ltmParser = null!;

		public override string ResourcesPath { get; } = "TASVideos.Test.MovieParsers.LtmSampleFiles.";

		[TestInitialize]
		public void Initialize()
		{
			_ltmParser = new Ltm();
		}

		[TestMethod]
		public async Task Region()
		{
			var result = await _ltmParser.Parse(Embedded("2frames.ltm"));

			Assert.IsTrue(result.Success);
			Assert.AreEqual(RegionType.Ntsc, result.Region);
		}

		[TestMethod]
		public async Task FrameCount()
		{
			var result = await _ltmParser.Parse(Embedded("2frames.ltm"));

			Assert.IsTrue(result.Success);
			Assert.AreEqual(SystemCodes.Linux, result.SystemCode);
			Assert.AreEqual(2, result.Frames);
		}

		[TestMethod]
		public async Task RerecordCount()
		{
			var result = await _ltmParser.Parse(Embedded("2frames.ltm"));

			Assert.IsTrue(result.Success);
			Assert.AreEqual(SystemCodes.Linux, result.SystemCode);
			Assert.AreEqual(7, result.RerecordCount);
			AssertNoWarningsOrErrors(result);
		}

		[TestMethod]
		public async Task FrameRate()
		{
			var result = await _ltmParser.Parse(Embedded("2frames.ltm"));

			Assert.IsTrue(result.Success);
			Assert.AreEqual(SystemCodes.Linux, result.SystemCode);
			Assert.AreEqual(120, result.FrameRateOverride);
			AssertNoWarningsOrErrors(result);
		}

		[TestMethod]
		public async Task MissingFrameRate_Defaults()
		{
			var result = await _ltmParser.Parse(Embedded("noframerate.ltm"));

			Assert.IsTrue(result.Success);
			Assert.AreEqual(Ltm.DefaultFrameRate, result.FrameRateOverride);
			AssertNoErrors(result);
			Assert.IsNotNull(result.Warnings);
			Assert.AreEqual(1, result.Warnings.Count());
		}

		[TestMethod]
		public async Task PowerOn()
		{
			var result = await _ltmParser.Parse(Embedded("2frames.ltm"));

			Assert.IsTrue(result.Success);
			Assert.AreEqual(SystemCodes.Linux, result.SystemCode);
			Assert.AreEqual(MovieStartType.PowerOn, result.StartType);
			AssertNoWarningsOrErrors(result);
		}

		[TestMethod]
		public async Task Savestate()
		{
			var result = await _ltmParser.Parse(Embedded("savestate.ltm"));

			Assert.IsTrue(result.Success);
			Assert.AreEqual(SystemCodes.Linux, result.SystemCode);
			Assert.AreEqual(MovieStartType.Savestate, result.StartType);
			AssertNoWarningsOrErrors(result);
		}
	}
}