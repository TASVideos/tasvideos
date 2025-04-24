using TASVideos.Core.Services;
using TASVideos.Pages.Publications;
using TASVideos.Tests.Base;

namespace TASVideos.RazorPages.Tests.Pages.Publications;

[TestClass]
public class ViewModelModelTests : TestDbBase
{
	private readonly ViewModel _page;
	private readonly IFileService _fileService;

	public ViewModelModelTests()
	{
		_fileService = Substitute.For<IFileService>();
		_page = new ViewModel(_db, _fileService);
	}

	[TestMethod]
	public async Task OnGet_NoPublication_ReturnsNotFound()
	{
		var actual = await _page.OnGet();
		Assert.IsInstanceOfType<NotFoundResult>(actual);
	}

	[TestMethod]
	public async Task OnGet_Publication_ReturnsPublication()
	{
		var pub = _db.AddPublication().Entity;
		_page.Id = pub.Id;

		var actual = await _page.OnGet();

		Assert.IsInstanceOfType<PageResult>(actual);
		Assert.AreEqual(pub.Id, _page.Publication.Id);
		Assert.AreEqual(pub.Title, _page.Publication.Title);
	}

	[TestMethod]
	public async Task OnGetDownload_NoPublication_ReturnsNotFound()
	{
		var actual = await _page.OnGetDownload();
		Assert.IsInstanceOfType<NotFoundResult>(actual);
	}

	[TestMethod]
	public async Task OnGetDownload_Publication_ReturnsPublication()
	{
		const int publicationId = 1;
		byte[] pubData = [0xFF, 0xFE];
		const string pubPath = "pub.bk2";
		_fileService.GetPublicationFile(publicationId)
			.Returns(new ZippedFile(pubData, pubPath));
		_page.Id = publicationId;

		var actual = await _page.OnGetDownload();
		Assert.IsInstanceOfType<FileContentResult>(actual);
		var fileContentResult = (FileContentResult)actual;
		Assert.AreEqual(pubData.Length, fileContentResult.FileContents.Length);
		Assert.AreEqual(pubPath + ".zip", fileContentResult.FileDownloadName);
	}

	[TestMethod]
	public async Task OnGetDownloadAdditional_NoPublication_ReturnsNotFound()
	{
		var actual = await _page.OnGetDownloadAdditional(0);
		Assert.IsInstanceOfType<NotFoundResult>(actual);
	}

	[TestMethod]
	public async Task OnGetDownloadAdditional_PubWithWrongFileId_ReturnsNotFound()
	{
		const int publicationId = 1;
		byte[] pubData = [0xFF, 0xFE];
		const string pubPath = "pub.bk2";
		const int fileId = 2;
		_fileService.GetAdditionalPublicationFile(publicationId, fileId)
			.Returns(new ZippedFile(pubData, pubPath));
		_page.Id = publicationId;

		var actual = await _page.OnGetDownloadAdditional(fileId + 1);

		Assert.IsInstanceOfType<NotFoundResult>(actual);
	}

	[TestMethod]
	public async Task OnGetDownloadAdditional_PubAndFileId_ReturnsFile()
	{
		const int publicationId = 1;
		byte[] pubData = [0xFF, 0xFE];
		const string pubPath = "pub.bk2";
		const int fileId = 2;
		_fileService.GetAdditionalPublicationFile(publicationId, fileId)
			.Returns(new ZippedFile(pubData, pubPath));
		_page.Id = publicationId;

		var actual = await _page.OnGetDownloadAdditional(fileId);

		Assert.IsInstanceOfType<FileContentResult>(actual);
		var fileContentResult = (FileContentResult)actual;
		Assert.AreEqual(pubData.Length, fileContentResult.FileContents.Length);
		Assert.AreEqual(pubPath + ".zip", fileContentResult.FileDownloadName);
	}
}
