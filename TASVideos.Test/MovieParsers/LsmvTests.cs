using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using TASVideos.MovieParsers.Parsers;

// ReSharper disable InconsistentNaming
namespace TASVideos.Test.MovieParsers
{
	[TestClass]
	[TestCategory("LsmvParsers")]
	public class LsmvTests : BaseParserTests
	{
		private Lsmv _lsmvParser;

		public override string ResourcesPath { get; } = "TASVideos.Test.MovieParsers.LsmvSampleFiles.";

		[TestInitialize]
		public void Initialize()
		{
			_lsmvParser = new Lsmv();
		}

		[TestMethod]
		public void Errors()
		{
			var result = _lsmvParser.Parse(Embedded("noinputlog.lsmv"));
			Assert.IsFalse(result.Success);
			AssertNoWarnings(result);
			Assert.IsNotNull(result.Errors);
			Assert.AreEqual(1, result.Errors.Count());
		}

		[TestMethod]
		public void Frames_WithSubFrames()
		{
			var result = _lsmvParser.Parse(Embedded("2frameswithsub.lsmv"));
			Assert.IsTrue(result.Success);
			Assert.AreEqual(2, result.Frames);
			AssertNoWarningsOrErrors(result);
		}

		[TestMethod]
		public void Frames_NoInputFrames_Returns0()
		{
			var result = _lsmvParser.Parse(Embedded("0frameswithsub.lsmv"));
			Assert.IsTrue(result.Success);
			Assert.AreEqual(0, result.Frames);
			AssertNoWarningsOrErrors(result);
		}

		[TestMethod]
		public void NoRerecordEntry_Warning()
		{
			var result = _lsmvParser.Parse(Embedded("norerecordentry.lsmv"));
			Assert.IsTrue(result.Success);
			AssertNoErrors(result);
			Assert.IsNotNull(result.Warnings);
			Assert.AreEqual(1, result.Warnings.Count());
		}

		[TestMethod]
		public void EmptyRerecordEntry_Warning()
		{
			var result = _lsmvParser.Parse(Embedded("emptyrerecordentry.lsmv"));
			Assert.IsTrue(result.Success);
			AssertNoErrors(result);
			Assert.IsNotNull(result.Warnings);
			Assert.AreEqual(1, result.Warnings.Count());
		}

		[TestMethod]
		public void InvalidRerecordEntry_Warning()
		{
			var result = _lsmvParser.Parse(Embedded("invalidrerecordentry.lsmv"));
			Assert.IsTrue(result.Success);
			AssertNoErrors(result);
			Assert.IsNotNull(result.Warnings);
			Assert.AreEqual(1, result.Warnings.Count());
		}

		[TestMethod]
		public void ValidRerecordEntry_Warning()
		{
			var result = _lsmvParser.Parse(Embedded("2frameswithsub.lsmv"));
			Assert.IsTrue(result.Success);
			Assert.AreEqual(1, result.RerecordCount);
			AssertNoWarningsOrErrors(result);
		}
	}
}
