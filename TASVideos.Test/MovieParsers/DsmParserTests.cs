using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TASVideos.MovieParsers;
using TASVideos.MovieParsers.Parsers;
using TASVideos.MovieParsers.Result;

namespace TASVideos.Test.MovieParsers
{
	[TestClass]
	[TestCategory("DsmParsers")]
	public class DsmParserTests : BaseParserTests
	{
		private Dsm _dsmParser = null!;
		public override string ResourcesPath { get; } = "TASVideos.Test.MovieParsers.DsmSampleFiles.";

		[TestInitialize]
		public void Initialize()
		{
			_dsmParser = new Dsm();
		}

		[TestMethod]
		public async Task System()
		{
			var result = await _dsmParser.Parse(Embedded("2frames.dsm"));
			Assert.IsTrue(result.Success);
			Assert.AreEqual(SystemCodes.Ds, result.SystemCode);
			AssertNoWarningsOrErrors(result);
		}

		[TestMethod]
		public async Task MultipleFrames()
		{
			var result = await _dsmParser.Parse(Embedded("2frames.dsm"));
			Assert.IsTrue(result.Success);
			Assert.AreEqual(2, result.Frames);
			AssertNoWarningsOrErrors(result);
		}

		[TestMethod]
		public async Task ZeroFrames()
		{
			var result = await _dsmParser.Parse(Embedded("0frames.dsm"));
			Assert.IsTrue(result.Success);
			Assert.AreEqual(0, result.Frames);
			AssertNoWarningsOrErrors(result);
		}

		[TestMethod]
		public async Task RerecordCount()
		{
			var result = await _dsmParser.Parse(Embedded("2frames.dsm"));
			Assert.IsTrue(result.Success);
			Assert.AreEqual(1, result.RerecordCount);
			AssertNoWarningsOrErrors(result);
		}

		[TestMethod]
		public async Task NoRerecordCount()
		{
			var result = await _dsmParser.Parse(Embedded("norerecords.dsm"));
			Assert.IsTrue(result.Success);
			Assert.AreEqual(0, result.RerecordCount, "Rerecord count is assumed to be 0");
			Assert.IsNotNull(result.Warnings);
			Assert.AreEqual(1, result.Warnings.Count());
			AssertNoErrors(result);
		}

		[TestMethod]
		public async Task PowerOn()
		{
			var result = await _dsmParser.Parse(Embedded("2frames.dsm"));
			Assert.IsTrue(result.Success);
			Assert.AreEqual(MovieStartType.PowerOn, result.StartType);
			AssertNoWarningsOrErrors(result);
		}

		[TestMethod]
		public async Task Sram()
		{
			var result = await _dsmParser.Parse(Embedded("sram.dsm"));
			Assert.IsTrue(result.Success);
			Assert.AreEqual(MovieStartType.Sram, result.StartType);
			AssertNoWarningsOrErrors(result);
		}

		[TestMethod]
		public async Task Savestate()
		{
			var result = await _dsmParser.Parse(Embedded("savestate.dsm"));
			Assert.IsTrue(result.Success);
			Assert.AreEqual(MovieStartType.Savestate, result.StartType);
			AssertNoWarningsOrErrors(result);
		}
	}
}
