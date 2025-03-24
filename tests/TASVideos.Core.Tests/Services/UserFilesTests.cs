using TASVideos.Core.Services.Wiki;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Game;
using TASVideos.MovieParsers;
using TASVideos.MovieParsers.Result;

namespace TASVideos.Core.Tests.Services;

[TestClass]
public class UserFilesTests : TestDbBase
{
	private readonly IFileService _fileService;
	private readonly IMovieParser _parser;
	private readonly IWikiPages _wikiPages;
	private readonly UserFiles _userFiles;

	public UserFilesTests()
	{
		_parser = Substitute.For<IMovieParser>();
		_fileService = Substitute.For<IFileService>();
		_wikiPages = Substitute.For<IWikiPages>();
		_userFiles = new UserFiles(_db, _parser, _fileService, _wikiPages);
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
		_db.AddUser(userId, "_");
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
		_db.AddUser(userId, "_");
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
		_db.AddUser(userId, "_");
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
		var user = _db.AddUser(0).Entity;
		const int candidateFileSize = 100;
		_db.UserFiles.Add(new UserFile
		{
			Id = 1,
			Author = user,
			Content = new byte[SiteGlobalConstants.UserFileStorageLimit - candidateFileSize],
			CompressionType = Compression.None
		});
		await _db.SaveChangesAsync();

		var actual = await _userFiles.SpaceAvailable(user.Id, candidateFileSize);
		Assert.IsTrue(actual);
	}

	[TestMethod]
	public async Task SpaceAvailable_NotUnavailable_ReturnsFalse()
	{
		var user = _db.AddUser(0).Entity;
		const int candidateFileSize = 100;
		_db.UserFiles.Add(new UserFile
		{
			Id = 1,
			Author = user,
			Content = new byte[SiteGlobalConstants.UserFileStorageLimit - candidateFileSize + 1],
			CompressionType = Compression.None
		});
		await _db.SaveChangesAsync();

		var actual = await _userFiles.SpaceAvailable(user.Id, candidateFileSize);
		Assert.IsFalse(actual);
	}

	[TestMethod]
	public async Task SupportedFileExtensions_SupportedIfParserSupports()
	{
		const string fileExt = ".test";
		_parser.SupportedMovieExtensions.Returns([fileExt]);

		var actual = await _userFiles.SupportedFileExtensions();
		Assert.IsTrue(actual.Contains(fileExt));
	}

	[TestMethod]
	public async Task SupportedFileExtensions_SupportedIfSupplemental()
	{
		const string fileExt = ".lua";
		_wikiPages.Page(Arg.Any<string>()).Returns(new WikiResult { Markup = fileExt + ", .nothing" });

		var actual = await _userFiles.SupportedFileExtensions();
		Assert.IsTrue(actual.Contains(fileExt));
	}

	[TestMethod]
	public async Task Upload_SupplementalFile_Success()
	{
		var user = _db.AddUser(0).Entity;
		var system = _db.GameSystems.Add(new GameSystem()).Entity;
		var game = _db.Games.Add(new Game()).Entity;
		await _db.SaveChangesAsync();
		byte[] fileData = [0xFF];
		const string title = "title";
		const string desc = "description";
		const string fileName = "script.lua";
		const bool hidden = true;
		_fileService.Compress(Arg.Any<byte[]>()).Returns(new CompressedFile(100, 99, Compression.Gzip, fileData));
		_wikiPages.Page(Arg.Any<string>()).Returns(new WikiResult { Markup = ".lua" });

		var (id, parseResult) = await _userFiles.Upload(user.Id, new(title, desc, system.Id, game.Id, fileData, fileName, hidden));

		Assert.IsTrue(id > 0);
		Assert.IsNull(parseResult);
		Assert.AreEqual(1, _db.UserFiles.Count());
		var userFile = _db.UserFiles.Single();
		Assert.AreEqual(title, userFile.Title);
		Assert.AreEqual(desc, userFile.Description);
		Assert.AreEqual(system.Id, userFile.SystemId);
		Assert.AreEqual(game.Id, userFile.GameId);
		Assert.AreEqual(fileName, userFile.FileName);
		Assert.AreEqual(hidden, userFile.Hidden);
		Assert.AreEqual(UserFileClass.Support, userFile.Class);
	}

	[TestMethod]
	public async Task Upload_MovieFile_Success()
	{
		var user = _db.AddUser(0).Entity;
		var system = _db.GameSystems.Add(new GameSystem()).Entity;
		var game = _db.Games.Add(new Game()).Entity;
		await _db.SaveChangesAsync();
		byte[] fileData = [0xFF];
		const string title = "title";
		const string desc = "description";
		const string fileName = "movie.bk2";
		const bool hidden = true;
		_fileService.Compress(Arg.Any<byte[]>()).Returns(new CompressedFile(100, 99, Compression.Gzip, fileData));
		_parser.SupportedMovieExtensions.Returns([".bk2"]);
		_parser.ParseFile(Arg.Any<string>(), Arg.Any<Stream>()).Returns(new TestParseResult());

		var (id, parseResult) = await _userFiles.Upload(user.Id, new(title, desc, system.Id, game.Id, fileData, fileName, hidden));

		Assert.IsTrue(id > 0);
		Assert.IsNotNull(parseResult);
		Assert.AreEqual(1, _db.UserFiles.Count());
		var userFile = _db.UserFiles.Single();
		Assert.AreEqual(title, userFile.Title);
		Assert.AreEqual(desc, userFile.Description);
		Assert.AreEqual(system.Id, userFile.SystemId);
		Assert.AreEqual(game.Id, userFile.GameId);
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
		string[] extensions = [".lua", ".wch"];
		_wikiPages.Page(Arg.Any<string>()).Returns(new WikiResult { Markup = markup });

		var actual = await _userFiles.SupportedSupplementalFiles();
		Assert.IsNotNull(actual);
		Assert.IsTrue(extensions.OrderBy(e => e).SequenceEqual(actual));
	}

	private class TestParseResult : IParseResult
	{
		public bool Success => true;
		public IEnumerable<string> Errors => [];
		public IEnumerable<ParseWarnings> Warnings => [];
		public string FileExtension => "";
		public RegionType Region => RegionType.Unknown;
		public int Frames => 0;
		public string SystemCode => "";
		public int RerecordCount => 0;
		public MovieStartType StartType => MovieStartType.PowerOn;
		public double? FrameRateOverride => null;
		public long? CycleCount => null;
		public string? Annotations => null;
		public Dictionary<HashType, string> Hashes { get; } = [];
	}
}
