using System.IO;
using System.Linq;
using System.Reflection;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using TASVideos.MovieParsers;
using TASVideos.MovieParsers.Parsers;
using TASVideos.MovieParsers.Result;

namespace TASVideos.Test.MovieParsers
{
	[TestClass]
	[TestCategory("BK2Parsers")]
	public class Fm2ParserTests
	{
		private const string Fm2ResourcesPath = "TASVideos.Test.MovieParsers.Fm2SampleFiles.";
		private Fm2 _fm2Parser;

		[TestInitialize]
		public void Initialize()
		{
			_fm2Parser = new Fm2();
		}

		[TestMethod]
		public void Ntsc()
		{
			var result = _fm2Parser.Parse(Embedded("ntsc.fm2"));
			Assert.IsTrue(result.Success, "Result is successful");
			Assert.AreEqual(2, result.Frames, "Frame count should be 2");
			Assert.AreEqual(RegionType.Ntsc, result.Region);
			Assert.AreEqual(21, result.RerecordCount);
			Assert.AreEqual(SystemCodes.Nes, result.SystemCode, "System chould be NES");
			Assert.AreEqual(MovieStartType.PowerOn, result.StartType);
		}

		[TestMethod]
		public void Pal()
		{
			var result = _fm2Parser.Parse(Embedded("pal.fm2"));
			Assert.IsTrue(result.Success, "Result is successful");
			Assert.AreEqual(2, result.Frames, "Frame count should be 2");
			Assert.AreEqual(RegionType.Pal, result.Region);
			Assert.AreEqual(21, result.RerecordCount);
			Assert.AreEqual(SystemCodes.Nes, result.SystemCode, "System chould be NES");
			Assert.AreEqual(MovieStartType.PowerOn, result.StartType);
		}

		[TestMethod]
		public void Fds()
		{
			var result = _fm2Parser.Parse(Embedded("fds.fm2"));
			Assert.IsTrue(result.Success, "Result is successful");
			Assert.AreEqual(2, result.Frames, "Frame count should be 2");
			Assert.AreEqual(RegionType.Ntsc, result.Region);
			Assert.AreEqual(21, result.RerecordCount);
			Assert.AreEqual(SystemCodes.Fds, result.SystemCode, "System should be FDS");
			Assert.AreEqual(MovieStartType.PowerOn, result.StartType);
		}

		[TestMethod]
		public void Savestate()
		{
			var result = _fm2Parser.Parse(Embedded("savestate.fm2"));
			Assert.IsTrue(result.Success, "Result is successful");
			Assert.AreEqual(2, result.Frames, "Frame count should be 2");
			Assert.AreEqual(RegionType.Ntsc, result.Region);
			Assert.AreEqual(21, result.RerecordCount);
			Assert.AreEqual(SystemCodes.Nes, result.SystemCode, "System chould be NES");
			Assert.AreEqual(MovieStartType.Savestate, result.StartType);
		}

		private Stream Embedded(string name)
		{
			return Assembly.GetAssembly(typeof(Bk2ParserTests)).GetManifestResourceStream(Fm2ResourcesPath + name);
		}
	}
}
