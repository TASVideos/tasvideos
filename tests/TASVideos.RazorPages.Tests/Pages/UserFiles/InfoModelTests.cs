using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using TASVideos.Core.Services;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Game;
using TASVideos.Pages.UserFiles;
using TASVideos.Tests.Base;

namespace TASVideos.RazorPages.Tests.Pages.UserFiles;

[TestClass]
public class InfoModelTests : TestDbBase
{
	private readonly IFileService _fileService;
	private readonly InfoModel _page;

	public InfoModelTests()
	{
		_fileService = Substitute.For<IFileService>();
		_page = new InfoModel(_db, _fileService);
	}

	[TestMethod]
	public async Task OnGet_WithValidId_LoadsUserFileInfo()
	{
		var author = _db.AddUser("TestAuthor").Entity;
		var game = new Game { Id = 1, DisplayName = "Test Game" };
		_db.Games.Add(game);

		var userFile = new UserFile
		{
			Id = 1,
			FileName = "test-movie.bk2",
			Title = "Test Movie",
			Author = author,
			Game = game,
			Class = UserFileClass.Movie,
			Length = 123.45m,
			Frames = 7890,
			Rerecords = 456,
			Downloads = 10
		};

		_db.UserFiles.Add(userFile);
		await _db.SaveChangesAsync();

		_page.Id = 1;
		var result = await _page.OnGet();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.AreEqual(1, _page.UserFile.Id);
		Assert.AreEqual("Test Movie", _page.UserFile.Title);
		Assert.AreEqual("TestAuthor", _page.UserFile.Author);
		Assert.AreEqual("Test Game", _page.UserFile.GameName);
		Assert.AreEqual(UserFileClass.Movie, _page.UserFile.Class);
		Assert.AreEqual(123.45m, _page.UserFile.Length);
		Assert.AreEqual(7890, _page.UserFile.Frames);
		Assert.AreEqual(456, _page.UserFile.Rerecords);
		Assert.AreEqual(10, _page.UserFile.Downloads);
	}

	[TestMethod]
	public async Task OnGet_WithNonExistentId_ReturnsNotFound()
	{
		_page.Id = 999;
		var result = await _page.OnGet();

		Assert.IsInstanceOfType<NotFoundResult>(result);
	}

	[TestMethod]
	public async Task OnGet_WithPreviewableSupportFile_LoadsContentPreview()
	{
		var author = _db.AddUser("TestAuthor").Entity;
		_db.UserFiles.Add(new UserFile
		{
			Id = 1,
			FileName = "test-config.lua", // Previewable extension
			Author = author,
			Hidden = false,
			Class = UserFileClass.Support,
			Content = "Test file content for preview"u8.ToArray(),
			CompressionType = Compression.None
		});
		await _db.SaveChangesAsync();

		_page.Id = 1;
		var result = await _page.OnGet();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.AreEqual("Test file content for preview", _page.UserFile.ContentPreview);
		Assert.AreEqual("lua", _page.UserFile.Extension);
	}

	[TestMethod]
	public async Task OnGet_WithCompressedPreviewableFile_DecompressesContent()
	{
		var author = _db.AddUser("TestAuthor").Entity;
		var compressedContent = new byte[] { 1, 2, 3, 4, 5 };
		const string decompressedContent = "Decompressed test content";
		_fileService.DecompressGzipToString(compressedContent)
			.Returns(Task.FromResult(decompressedContent));

		_db.UserFiles.Add(new UserFile
		{
			Id = 1,
			FileName = "test-script.lua", // Previewable extension
			Title = "Test Script",
			Author = author,
			Hidden = false,
			Class = UserFileClass.Support,
			Content = compressedContent,
			CompressionType = Compression.Gzip
		});
		await _db.SaveChangesAsync();
		_page.Id = 1;

		var result = await _page.OnGet();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.AreEqual(decompressedContent, _page.UserFile.ContentPreview);
		await _fileService.Received(1).DecompressGzipToString(compressedContent);
	}

	[TestMethod]
	public async Task OnGet_WithNonPreviewableSupportFile_DoesNotLoadContent()
	{
		var author = _db.AddUser("TestAuthor").Entity;
		var userFile = new UserFile
		{
			Id = 1,
			FileName = "test-file.exe", // Non-previewable extension
			Title = "Test Executable",
			Author = author,
			Hidden = false,
			Class = UserFileClass.Support,
			Content = [1, 2, 3, 4, 5],
			CompressionType = Compression.None
		};

		_db.UserFiles.Add(userFile);
		await _db.SaveChangesAsync();
		_page.Id = 1;

		var result = await _page.OnGet();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.AreEqual("", _page.UserFile.ContentPreview);
		await _fileService.DidNotReceive().DecompressGzipToString(Arg.Any<byte[]>());
	}

	[TestMethod]
	public async Task OnGet_WithMovieFile_DoesNotLoadContentPreview()
	{
		var author = _db.AddUser("TestAuthor").Entity;
		_db.UserFiles.Add(new UserFile
		{
			Id = 1,
			FileName = "test-movie.bk2",
			Author = author,
			Hidden = false,
			Class = UserFileClass.Movie, // Movie files don't get content preview
			Content = [1, 2, 3, 4, 5],
			CompressionType = Compression.None
		});
		await _db.SaveChangesAsync();
		_page.Id = 1;

		var result = await _page.OnGet();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.AreEqual("", _page.UserFile.ContentPreview);
		await _fileService.DidNotReceive().DecompressGzipToString(Arg.Any<byte[]>());
	}

	[TestMethod]
	public async Task OnGetDownload_NoFile_ReturnsNotFound()
	{
		_page.Id = long.MaxValue;

		var result = await _page.OnGetDownload();

		Assert.IsInstanceOfType<NotFoundResult>(result);
	}

	[TestMethod]
	public async Task OnGetDownload_UpdatesDownloadCount()
	{
		const long fileId = 1;
		const int originalViewCount = 2;
		var user = _db.AddUser(0).Entity;
		_db.UserFiles.Add(new UserFile { Id = fileId, Content = [0xFF], Downloads = originalViewCount, Author = user });
		await _db.SaveChangesAsync();
		_page.Id = fileId;

		var result = await _page.OnGetDownload();

		Assert.IsInstanceOfType<InfoModel.DownloadResult>(result);
		var fileInDb = await _db.UserFiles.FindAsync(fileId);
		Assert.IsNotNull(fileInDb);
		Assert.AreEqual(originalViewCount + 1, fileInDb.Downloads);
	}

	[TestMethod]
	public async Task DownloadResult_ExecutesWithCorrectHeaders()
	{
		var author = _db.AddUser("TestAuthor").Entity;
		var fileContent = "Test file content"u8.ToArray();
		var userFile = new UserFile
		{
			Id = 1,
			FileName = "test-file.txt",
			Title = "Test File",
			Author = author,
			Hidden = false,
			Class = UserFileClass.Support,
			Content = fileContent,
			CompressionType = Compression.None
		};

		_db.UserFiles.Add(userFile);
		await _db.SaveChangesAsync();

		var downloadResult = new InfoModel.DownloadResult(userFile);
		var httpContext = new DefaultHttpContext();
		var actionContext = new ActionContext
		{
			HttpContext = httpContext
		};

		using var responseStream = new MemoryStream();
		httpContext.Response.Body = responseStream;

		await downloadResult.ExecuteResultAsync(actionContext);

		Assert.AreEqual(200, httpContext.Response.StatusCode);
		Assert.AreEqual(fileContent.Length.ToString(), httpContext.Response.Headers["Content-Length"].ToString());
		Assert.AreEqual("application/octet-stream", httpContext.Response.Headers.ContentType.ToString());
		Assert.IsTrue(httpContext.Response.Headers.ContainsKey("Content-Disposition"));

		var downloadedContent = responseStream.ToArray();
		CollectionAssert.AreEqual(fileContent, downloadedContent);
	}

	[TestMethod]
	public async Task DownloadResult_WithGzipCompression_SetsCorrectHeaders()
	{
		var author = _db.AddUser("TestAuthor").Entity;
		var userFile = new UserFile
		{
			Id = 1,
			FileName = "compressed-file.txt",
			Title = "Compressed File",
			Author = author,
			Hidden = false,
			Class = UserFileClass.Support,
			Content = [1, 2, 3, 4, 5],
			CompressionType = Compression.Gzip
		};

		var downloadResult = new InfoModel.DownloadResult(userFile);
		var httpContext = new DefaultHttpContext();
		var actionContext = new ActionContext
		{
			HttpContext = httpContext
		};

		using var responseStream = new MemoryStream();
		httpContext.Response.Body = responseStream;

		await downloadResult.ExecuteResultAsync(actionContext);

		Assert.AreEqual("gzip", httpContext.Response.Headers.ContentEncoding.ToString());
		Assert.AreEqual("application/octet-stream", httpContext.Response.Headers.ContentType.ToString());
	}

	[TestMethod]
	public void UserFileModel_IsMovieProperty_ReturnsTrueForMovieClass()
	{
		var movieModel = new InfoModel.UserFileModel
		{
			Class = UserFileClass.Movie
		};

		var supportModel = new InfoModel.UserFileModel
		{
			Class = UserFileClass.Support
		};

		Assert.IsTrue(movieModel.IsMovie);
		Assert.IsFalse(supportModel.IsMovie);
	}

	[TestMethod]
	public void UserFileModel_ExtensionProperty_ExtractsCorrectly()
	{
		var model = new InfoModel.UserFileModel
		{
			FileName = "test-file.BK2"
		};

		Assert.AreEqual("bk2", model.Extension);
	}

	[TestMethod]
	public void UserFileModel_ExtensionProperty_HandlesNoExtension()
	{
		var model = new InfoModel.UserFileModel
		{
			FileName = "test-file-no-ext"
		};

		Assert.AreEqual("test-file-no-ext", model.Extension);
	}

	[TestMethod]
	public void UserFileModel_ExtensionProperty_HandlesNullFileName()
	{
		var model = new InfoModel.UserFileModel
		{
			FileName = null
		};

		Assert.AreEqual("", model.Extension);
	}

	[TestMethod]
	public void InfoModel_AllowsAnonymousUsers()
	{
		var infoModelType = typeof(InfoModel);
		var allowAnonymousAttribute = infoModelType.GetCustomAttributes(typeof(AllowAnonymousAttribute), inherit: false);

		Assert.IsTrue(allowAnonymousAttribute.Length > 0);
	}
}
