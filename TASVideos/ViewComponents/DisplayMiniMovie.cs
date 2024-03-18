﻿using Microsoft.AspNetCore.Mvc;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents;

// TODO: a better name for this is FrontPageMovie or something like that
[WikiModule(WikiModules.DisplayMiniMovie)]
public class DisplayMiniMovie(ApplicationDbContext db) : ViewComponent
{
	public async Task<IViewComponentResult> InvokeAsync(string? pubClass, IList<string> flags)
	{
		var candidateIds = await FrontPageMovieCandidates(pubClass, flags);
		var id = candidateIds.ToList().AtRandom();
		var movie = await GetPublicationMiniMovie(id);
		return View(movie);
	}

	private async Task<IEnumerable<int>> FrontPageMovieCandidates(string? publicationClass, IList<string> flagsArr)
	{
		var query = db.Publications
			.ThatAreCurrent()
			.AsQueryable();

		if (!string.IsNullOrWhiteSpace(publicationClass))
		{
			query = query.Where(p => p.PublicationClass!.Name == publicationClass);
		}

		if (flagsArr.Count > 0)
		{
			query = query.Where(p => p.PublicationFlags.Any(pf => flagsArr.Contains(pf.Flag!.Token)));
		}

		return await query
			.Select(p => p.Id)
			.ToListAsync();
	}

	private async Task<MiniMovieModel?> GetPublicationMiniMovie(int id)
	{
		// TODO: id == 0 means there are no publications, which is an out-of-the-box problem only, make this scenario more clear and simpler
		if (id != 0)
		{
			return await db.Publications
				.ToMiniMovieModel()
				.SingleOrDefaultAsync(p => p.Id == id);
		}

		return await db.Publications
			.Select(p => new MiniMovieModel
			{
				Id = 0,
				Title = "Error",
				OnlineWatchingUrl = ""
			})
			.SingleOrDefaultAsync(p => p.Id == id);
	}
}
