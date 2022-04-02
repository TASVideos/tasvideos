using TASVideos.Core.Services;
using TASVideos.Data.Entity;
using TASVideos.MovieParsers;
using TASVideos.MovieParsers.Result;

namespace TASVideos.Core.Tests.Services;

[TestClass]
public class UserFilesTests
{
	private readonly TestDbContext _db;
	private readonly Mock<IFileService> _fileService;
	private readonly Mock<IMovieParser> _parser;
	private readonly Mock<IWikiPages> _wikiPages;
	private readonly UserFiles _userFiles;

	public UserFilesTests()
	{
		_db = TestDbContext.Create();
		_parser = new Mock<IMovieParser>();
		_fileService = new Mock<IFileService>();
		_wikiPages = new Mock<IWikiPages>();
		_userFiles = new UserFiles(
			_db,
			_parser.Object,
			_fileService.Object,
			_wikiPages.Object);
	}

	[TestMethod]
	public async Task StorageUsed_InvalidUser_ReturnsZero()
	{
		var actual = await _userFiles.StorageUsed(int.MaxValue);
		Assert.AreEqual(0, actual);
	}

	[TestMethod]
	public async Task StorageUsed_Uncompressed_SingleFile()
	{
		const int userId = 1;
		const int fileSize = 100;
		_db.Users.Add(new User { Id = userId });
		_db.UserFiles.Add(new UserFile
		{
			AuthorId = userId,
			Content = new byte[fileSize],
			CompressionType = Compression.None,

			// Ensure these are not used to calculate
			PhysicalLength = fileSize + 1,
			Length = fileSize + 2,
			LogicalLength = fileSize + 3
		});
		await _db.SaveChangesAsync();

		var actual = await _userFiles.StorageUsed(userId);
		Assert.AreEqual(fileSize, actual);
	}

	[TestMethod]
	public async Task StorageUsed_Compressed_SingleFile()
	{
		const int userId = 1;
		const int fileSize = 100;
		_db.Users.Add(new User { Id = userId });
		_db.UserFiles.Add(new UserFile
		{
			AuthorId = userId,
			Content = new byte[fileSize],
			CompressionType = Compression.Gzip,

			// Ensure these are not used to calculate
			PhysicalLength = fileSize + 1,
			Length = fileSize + 2,
			LogicalLength = fileSize + 3
		});
		await _db.SaveChangesAsync();

		var actual = await _userFiles.StorageUsed(userId);
		Assert.AreEqual(fileSize, actual);
	}

	[TestMethod]
	public async Task StorageUsed_MultipleFiles()
	{
		const int userId = 1;
		const int fileSize = 100;
		_db.Users.Add(new User { Id = userId });
		_db.UserFiles.Add(new UserFile
		{
			Id = 1,
			AuthorId = userId,
			Content = new byte[fileSize],
			CompressionType = Compression.None,

			// Ensure these are not used to calculate
			PhysicalLength = fileSize + 1,
			Length = fileSize + 2,
			LogicalLength = fileSize + 3
		});
		_db.UserFiles.Add(new UserFile
		{
			Id = 2,
			AuthorId = userId,
			Content = new byte[fileSize],
			CompressionType = Compression.None,

			// Ensure these are not used to calculate
			PhysicalLength = fileSize + 1,
			Length = fileSize + 2,
			LogicalLength = fileSize + 3
		});
		await _db.SaveChangesAsync();

		var actual = await _userFiles.StorageUsed(userId);
		Assert.AreEqual(fileSize * 2, actual);
	}

	[TestMethod]
	public async Task SpaceAvailable_Available_ReturnsTrue()
	{
		const int userId = 1;
		const int candidateFileSize = 100;
		_db.UserFiles.Add(new UserFile
		{
			Id = 1,
			AuthorId = userId,
			Content = new byte[SiteGlobalConstants.UserFileStorageLimit - candidateFileSize],
			CompressionType = Compression.None
		});
		await _db.SaveChangesAsync();

		var actual = await _userFiles.SpaceAvailable(userId, candidateFileSize);
		Assert.IsTrue(actual);
	}

	[TestMethod]
	public async Task SpaceAvailable_NotUnavailable_ReturnsFalse()
	{
		const int userId = 1;
		const int candidateFileSize = 100;
		_db.UserFiles.Add(new UserFile
		{
			Id = 1,
			AuthorId = userId,
			Content = new byte[SiteGlobalConstants.UserFileStorageLimit - candidateFileSize + 1],
			CompressionType = Compression.None
		});
		await _db.SaveChangesAsync();

		var actual = await _userFiles.SpaceAvailable(userId, candidateFileSize);
		Assert.IsFalse(actual);
	}

	[TestMethod]
	public async Task IsSupportedFileExtension_SupportedIfParserSupports()
	{
		const string fileExt = ".test";
		_parser.Setup(m => m.SupportedMovieExtensions).Returns(new[] { fileExt });

		var actual = await _userFiles.IsSupportedFileExtension(fileExt);
		Assert.IsTrue(actual);
	}

	[TestMethod]
	public async Task IsSupportedFileExtension_SupportedIfSupplemental()
	{
		const string fileExt = ".lua";
		_wikiPages.Setup(m => m.Page(It.IsAny<string>(), null))
			.ReturnsAsync(new WikiPage { Markup = fileExt + ", .nothing" });

		var actual = await _userFiles.IsSupportedFileExtension(fileExt);
		Assert.IsTrue(actual);
	}

	[TestMethod]
	[ExpectedException(typeof(InvalidOperationException))]
	public async Task Upload_Throws_IfSpaceNotAvailable()
	{
		const int userId = 1;
		_db.UserFiles.Add(new UserFile
		{
			Id = 1,
			AuthorId = userId,
			Content = new byte[SiteGlobalConstants.UserFileStorageLimit],
			CompressionType = Compression.None
		});
		await _db.SaveChangesAsync();

		await _userFiles.Upload(userId, new("title", "desc", null, null, new byte[] { 0xFF }, "script.lua", true));
	}

	[TestMethod]
	public async Task Upload_SupplementalFile_Success()
	{
		const int userId = 1;
		byte[] fileData = { 0xFF };
		const string title = "title";
		const string desc = "description";
		const int systemId = 2;
		const int gameId = 3;
		const string fileName = "script.lua";
		const bool hidden = true;
		_fileService
			.Setup(m => m.Compress(It.IsAny<byte[]>()))
			.ReturnsAsync(new CompressedFile(100, 99, Compression.Gzip, fileData));
		_wikiPages
			.Setup(m => m.Page(It.IsAny<string>(), null))
			.ReturnsAsync(new WikiPage { Markup = ".lua" });

		var (id, parseResult) = await _userFiles.Upload(userId, new(title, desc, systemId, gameId, fileData, fileName, hidden));

		Assert.IsTrue(id > 0);
		Assert.IsNull(parseResult);
		Assert.AreEqual(1, _db.UserFiles.Count());
		var userFile = _db.UserFiles.Single();
		Assert.AreEqual(title, userFile.Title);
		Assert.AreEqual(desc, userFile.Description);
		Assert.AreEqual(systemId, userFile.SystemId);
		Assert.AreEqual(gameId, userFile.GameId);
		Assert.AreEqual(fileName, userFile.FileName);
		Assert.AreEqual(hidden, userFile.Hidden);
		Assert.AreEqual(UserFileClass.Support, userFile.Class);
	}

	[TestMethod]
	public async Task Upload_MovieFile_Success()
	{
		const int userId = 1;
		byte[] fileData = { 0xFF };
		const string title = "title";
		const string desc = "description";
		const int systemId = 2;
		const int gameId = 3;
		const string fileName = "movie.bk2";
		const bool hidden = true;
		_fileService
			.Setup(m => m.Compress(It.IsAny<byte[]>()))
			.ReturnsAsync(new CompressedFile(100, 99, Compression.Gzip, fileData));
		_parser
			.Setup(m => m.SupportedMovieExtensions)
			.Returns(new[] { ".bk2" });
		_parser
			.Setup(m => m.ParseFile(It.IsAny<string>(), It.IsAny<Stream>()))
			.ReturnsAsync(new TestParseResult());

		var (id, parseResult) = await _userFiles.Upload(userId, new(title, desc, systemId, gameId, fileData, fileName, hidden));

		Assert.IsTrue(id > 0);
		Assert.IsNotNull(parseResult);
		Assert.AreEqual(1, _db.UserFiles.Count());
		var userFile = _db.UserFiles.Single();
		Assert.AreEqual(title, userFile.Title);
		Assert.AreEqual(desc, userFile.Description);
		Assert.AreEqual(systemId, userFile.SystemId);
		Assert.AreEqual(gameId, userFile.GameId);
		Assert.AreEqual(fileName, userFile.FileName);
		Assert.AreEqual(hidden, userFile.Hidden);
		Assert.AreEqual(UserFileClass.Movie, userFile.Class);
	}

	[TestMethod]
	[DataRow(".lua,.wch")]
	[DataRow(" .lua, .wch ")]
	[DataRow(" .lua,\n .wch ")]
	public async Task SupportedSupplementalFiles(string markup)
	{
		string[] extensions = { ".lua", ".wch" };
		_wikiPages
			.Setup(m => m.Page(It.IsAny<string>(), null))
			.ReturnsAsync(new WikiPage { Markup = markup });

		var actual = await _userFiles.SupportedSupplementalFiles();
		Assert.IsNotNull(actual);
		Assert.IsTrue(extensions.OrderBy(e => e).SequenceEqual(actual));
	}

	private class TestParseResult : IParseResult
	{
		public bool Success => true;
		public IEnumerable<string> Errors => Enumerable.Empty<string>();
		public IEnumerable<ParseWarnings> Warnings => Enumerable.Empty<ParseWarnings>();
		public string FileExtension => "";
		public RegionType Region => RegionType.Unknown;
		public int Frames => 0;
		public string SystemCode => "";
		public int RerecordCount => 0;
		public MovieStartType StartType => MovieStartType.PowerOn;
		public double? FrameRateOverride => null;
		public long? CycleCount => null;
	}
}
