using TASVideos.Core.Services;
using TASVideos.Core.Services.Wiki;
using TASVideos.Data.Entity;
using TASVideos.Extensions;
using TASVideos.MovieParsers;
using TASVideos.Pages.Submissions;
using TASVideos.Tests.Base;
using static TASVideos.RazorPages.Tests.RazorTestHelpers;

namespace TASVideos.RazorPages.Tests.Pages.Submissions;

[TestClass]
public class ViewModelTests : TestDbBase
{
	private readonly IWikiPages _wikiPages;
	private readonly IFileService _fileService;
	private readonly ViewModel _page;

	public ViewModelTests()
	{
		_wikiPages = Substitute.For<IWikiPages>();
		_fileService = Substitute.For<IFileService>();
		_page = new ViewModel(_db, _wikiPages, _fileService, Substitute.For<IMovieParser>());
	}

	[TestMethod]
	public async Task OnGet_NoSubmissionReturnsNotFound()
	{
		_page.Id = 1;
		var actual = await _page.OnGet();
		Assert.IsInstanceOfType<NotFoundResult>(actual);
	}

	[TestMethod]
	public async Task OnGet_UnpublishedSubmissionExistsReturnsSubmission()
	{
		var sub = CreateUnpublishedSubmission();
		_page.Id = sub.Id;

		var actual = await _page.OnGet();

		Assert.IsInstanceOfType<PageResult>(actual);
		Assert.AreEqual(sub.RerecordCount, _page.Submission.RerecordCount);
		Assert.IsFalse(_page.CanEdit);
		Assert.AreEqual(0, _page.PublicationId);
		Assert.AreEqual(sub.Submitter!.UserName, _page.Submission.LastUpdateUser);
		Assert.IsTrue(_page.Submission.LastUpdateTimestamp < DateTime.UtcNow.AddDays(-1));
		Assert.IsFalse(_page.IsPublished);
	}

	[TestMethod]
	public async Task OnGet_SubmitterCanEdit()
	{
		var sub = CreateUnpublishedSubmission();
		_page.Id = sub.Id;
		AddAuthenticatedUser(_page, sub.Submitter!, [PermissionTo.SubmitMovies]);

		var actual = await _page.OnGet();

		Assert.IsInstanceOfType<PageResult>(actual);
		Assert.IsTrue(_page.CanEdit);
	}

	[TestMethod]
	public async Task OnGet_AuthorCanEdit()
	{
		var sub = CreateUnpublishedSubmission();
		var user = _db.AddUser("Author");
		_db.SubmissionAuthors.Add(new SubmissionAuthor
		{
			Author = user.Entity,
			Submission = sub
		});
		await _db.SaveChangesAsync();
		_page.Id = sub.Id;
		AddAuthenticatedUser(_page, user.Entity, [PermissionTo.SubmitMovies]);

		var actual = await _page.OnGet();

		Assert.IsInstanceOfType<PageResult>(actual);
		Assert.IsTrue(_page.CanEdit);
	}

	[TestMethod]
	public async Task OnGet_PublishedSubmissionReturnsPublicationId()
	{
		var sub = CreatePublishedSubmission();
		_page.Id = sub.Id;

		var actual = await _page.OnGet();

		Assert.IsInstanceOfType<PageResult>(actual);
		Assert.AreEqual(sub.Publication!.Id, _page.PublicationId);
		Assert.AreEqual(sub.System!.DisplayName, _page.Submission.SystemDisplayName);
		Assert.AreEqual(sub.Game!.DisplayName, _page.Submission.GameName);
		Assert.AreEqual(sub.GameVersion!.Name, _page.Submission.GameVersion);
		Assert.IsTrue(_page.IsPublished);
	}

	private Submission CreateUnpublishedSubmission()
	{
		var sub = _db.AddAndSaveUnpublishedSubmission();
		sub.Entity.RerecordCount = 5;
		_db.SaveChanges();
		var wikiPage = new WikiResult
		{
			CreateTimestamp = DateTime.UtcNow.AddYears(-1),
			AuthorName = sub.Entity.Submitter!.UserName
		};
		_wikiPages.Page(WikiHelper.ToSubmissionWikiPageName(sub.Entity.Id)).Returns(wikiPage);
		return sub.Entity;
	}

	[TestMethod]
	public async Task OnGetDownload_SubmissionDoesNotExist_ReturnsNotFound()
	{
		const int nonExistentSubId = 1;
		_page.Id = nonExistentSubId;
		_fileService.GetSubmissionFile(nonExistentSubId).Returns((ZippedFile?)null);

		var actual = await _page.OnGetDownload();
		Assert.IsInstanceOfType<NotFoundResult>(actual);
	}

	[TestMethod]
	public async Task OnGetDownload_SubmissionExists_ReturnsFile()
	{
		const int subId = 1;
		const string path = "path";
		byte[] data = [0xFF, 0xFF];

		_page.Id = subId;
		_fileService.GetSubmissionFile(subId).Returns(new ZippedFile(data, path));

		var actual = await _page.OnGetDownload();

		Assert.IsInstanceOfType<FileContentResult>(actual);
		var fileContentResult = (FileContentResult)actual;
		Assert.AreEqual(data, fileContentResult.FileContents);
		Assert.AreEqual($"{path}.zip", fileContentResult.FileDownloadName);
	}

	private Submission CreatePublishedSubmission()
	{
		var pub = _db.AddPublication();
		var sub = pub.Entity.Submission!;
		sub.System = pub.Entity.System;
		sub.SystemFrameRate = pub.Entity.SystemFrameRate;
		sub.Game = pub.Entity.Game;
		sub.GameVersion = pub.Entity.GameVersion;
		_db.SaveChanges();

		return sub;
	}
}
