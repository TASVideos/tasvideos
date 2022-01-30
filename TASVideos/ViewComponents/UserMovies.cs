using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Pages.UserFiles.Models;
using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents;

[WikiModule(WikiModules.UserMovies)]
public class UserMovies : ViewComponent
{
	private readonly ApplicationDbContext _db;
	private readonly IMapper _mapper;

	public UserMovies(ApplicationDbContext db, IMapper mapper)
	{
		_db = db;
		_mapper = mapper;
	}

	public async Task<IViewComponentResult> InvokeAsync(int? limit)
	{
		var count = limit ?? 5;

		var userMovies = await _mapper.ProjectTo<UserMovieListModel>(
			_db.UserFiles
				.ThatAreMovies()
				.ThatArePublic()
				.ByRecentlyUploaded())
			.Take(count)
			.ToListAsync();

		return View(userMovies);
	}
}
