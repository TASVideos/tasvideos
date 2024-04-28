﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Core.Services;
using TASVideos.Data.Entity;
using TASVideos.Pages.UserFiles;
using TASVideos.Tests.Base;

namespace TASVideos.RazorPages.Tests.Pages.UserFiles;

#pragma warning disable UTA001 //TODO this class should be public, or these shouldn't be tests
[TestClass]
internal class InfoModelTests
{
	private readonly InfoModel _page;
	private readonly TestDbContext _db;

	public InfoModelTests()
	{
		var fileService = Substitute.For<IFileService>();
		_db = TestDbContext.Create();
		_page = new InfoModel(_db, fileService);
	}

	[TestMethod]
	public async Task OnDownload_NoFile_ReturnsNotFound()
	{
		_page.Id = long.MaxValue;
		var result = await _page.OnGetDownload();
		Assert.IsNotNull(result);
		Assert.IsInstanceOfType<NotFoundResult>(result);
	}

	[TestMethod]
	public async Task OnDownload_UpdatesDownloadCount()
	{
		const long fileId = 1;
		const int originalViewCount = 2;
		byte[] content = [0xFF];
		_db.UserFiles.Add(new UserFile { Id = fileId, Content = content, Downloads = originalViewCount });

		_page.Id = fileId;
		var result = await _page.OnGetDownload();
		Assert.IsNotNull(result);
		Assert.IsInstanceOfType<InfoModel.DownloadResult>(result);
		var fileInDb = await _db.UserFiles.SingleAsync(uf => uf.Id == fileId);
		Assert.AreEqual(originalViewCount + 1, fileInDb.Downloads);
	}
}
