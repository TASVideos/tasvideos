﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Models;

namespace TASVideos.Pages.Profile;

[Authorize]
public class UserFilesModel : BasePageModel
{
	private readonly ApplicationDbContext _db;

	public UserFilesModel(ApplicationDbContext db)
	{
		_db = db;
	}

	public string UserName { get; set; } = "";

	public IEnumerable<UserFileModel> Files { get; set; } = new List<UserFileModel>();

	public async Task OnGet()
	{
		UserName = User.Name();
		Files = await _db.UserFiles
			.ForAuthor(UserName)
			.FilterByHidden(includeHidden: true)
			.OrderByDescending(uf => uf.UploadTimestamp)
			.ToUserFileModel()
			.ToListAsync();
	}
}
