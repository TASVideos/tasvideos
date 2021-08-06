using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TASVideos.MovieParsers.Parsers;
using TASVideos.MovieParsers.Result;

namespace TASVideos.MovieParsers.Tests
{
	[TestClass]
	[TestCategory("JrsrParsers")]
	public class JrsrTests : BaseParserTests
	{
		private readonly Jrsr _jrsrParser;
		public override string ResourcesPath { get; } = "TASVideos.MovieParsers.Tests.JrsrSampleFiles.";

		public JrsrTests()
		{
			_jrsrParser = new Jrsr();
		}

		[TestMethod]
		[DataRow(null, null)]
		[DataRow("", null)]
		[DataRow(" ", null)]
		[DataRow("NoPlus", null)]
		[DataRow("+NoNumber", null)]
		[DataRow("+1", 1L)]
		[DataRow("+196064706 org.jpc.emulator.peripheral.Keyboard KEYEDGE 28", 196064706L)]
		public void GetTime(string line, long? expected)
		{
			var actual = Jrsr.GetTime(line);
			Assert.AreEqual(expected, actual);
		}

		[TestMethod]
		public async Task EmptyFile()
		{
			var result = await _jrsrParser.Parse(Embedded("emptyfile.jrsr"));
			Assert.IsFalse(result.Success);
		}

		[TestMethod]
		public async Task CorrectMagic()
		{
			var result = await _jrsrParser.Parse(Embedded("correctmagic.jrsr"));
			Assert.IsTrue(result.Success);
			Assert.AreEqual(result.FileExtension, "jrsr");
			AssertNoWarningsOrErrors(result);
		}

		[TestMethod]
		public async Task WrongLineMagic()
		{
			var result = await _jrsrParser.Parse(Embedded("wronglinemagic.jrsr"));
			Assert.IsFalse(result.Success);
			AssertNoWarnings(result);
			Assert.AreEqual(1, result.Errors.Count());
		}

		[TestMethod]
		public async Task WrongMagic()
		{
			var result = await _jrsrParser.Parse(Embedded("wrongmagic.jrsr"));
			Assert.IsFalse(result.Success);
			AssertNoWarnings(result);
			Assert.AreEqual(1, result.Errors.Count());
		}

		[TestMethod]
		public async Task NoBeginHeader()
		{
			var result = await _jrsrParser.Parse(Embedded("nobeginheader.jrsr"));
			Assert.IsFalse(result.Success);
			AssertNoWarnings(result);
			Assert.AreEqual(1, result.Errors.Count());
		}

		[TestMethod]
		public async Task Rerecords()
		{
			var result = await _jrsrParser.Parse(Embedded("correctmagic.jrsr"));
			Assert.IsTrue(result.Success);
			Assert.AreEqual(17984, result.RerecordCount);
			AssertNoWarningsOrErrors(result);
		}

		[TestMethod]
		public async Task Savestate()
		{
			var result = await _jrsrParser.Parse(Embedded("savestate.jrsr"));
			Assert.IsTrue(result.Success);
			Assert.AreEqual(MovieStartType.Savestate, result.StartType);
			AssertNoWarningsOrErrors(result);
		}

		[TestMethod]
		public async Task ContainsSavestate_ReturnError()
		{
			var result = await _jrsrParser.Parse(Embedded("containssavestate.jrsr"));
			Assert.IsFalse(result.Success);
			AssertNoWarnings(result);
			Assert.AreEqual(1, result.Errors.Count());
		}

		[TestMethod]
		public async Task Frames()
		{
			var result = await _jrsrParser.Parse(Embedded("frames.jrsr"));
			Assert.IsTrue(result.Success);
			Assert.AreEqual(73900, result.Frames);
			Assert.AreEqual(60, result.FrameRateOverride);
			AssertNoWarningsOrErrors(result);
		}

		[TestMethod]
		public async Task MissingRerecords()
		{
			var result = await _jrsrParser.Parse(Embedded("missingrerecords.jrsr"));
			Assert.IsTrue(result.Success);
			Assert.AreEqual(0, result.RerecordCount, "Rerecord count assumed to be 0");
			AssertNoErrors(result);
			Assert.AreEqual(1, result.Warnings.Count());
		}

		[TestMethod]
		public async Task NegativeRerecords()
		{
			var result = await _jrsrParser.Parse(Embedded("negativererecords.jrsr"));
			Assert.IsTrue(result.Success);
			Assert.AreEqual(0, result.RerecordCount, "Rerecord count assumed to be 0");
			AssertNoErrors(result);
			Assert.AreEqual(1, result.Warnings.Count());
		}
	}
}
