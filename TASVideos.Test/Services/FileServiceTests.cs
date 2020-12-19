using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TASVideos.Services;
using TASVideos.Test.MovieParsers;

namespace TASVideos.Test.Services
{
	[TestClass]
	public class FileServiceTests : BaseParserTests
	{
		readonly FileService _fileService = new FileService();

		public override string ResourcesPath { get; } = "TASVideos.Test.MovieParsers.Bk2SampleFiles.";
		
		[TestMethod]
		public async Task CopyZip_BasicTest()
		{
			// Arrange
			const string newName = "1M";
			var stream = Embedded("2Frames.zip");
			await using var ms = new MemoryStream();
			await stream.CopyToAsync(ms);
			var bytes = ms.ToArray();

			// Act
			var result = await _fileService.CopyZip(bytes, newName);
			
			// Assert
			Assert.IsNotNull(result);
			Assert.IsTrue(result.Any());
			
			await using var resultStream = new MemoryStream(result);
			using var resultZipArchive = new ZipArchive(resultStream, ZipArchiveMode.Read);

			Assert.AreEqual(1, resultZipArchive.Entries.Count);
			var entry = resultZipArchive.Entries.Single();
			Assert.AreEqual(newName, entry.Name);
		}
	}
}
