using Microsoft.AspNetCore.Mvc;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.ViewComponents.Models;
using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents;

[WikiModule(WikiModules.MoviesByAuthor)]
public class MoviesByAuthor(ApplicationDbContext db) : ViewComponent
{
	public async Task<IViewComponentResult> InvokeAsync(DateTime? before, DateTime? after, string? newbies, bool showTiers)
	{
		if (!before.HasValue || !after.HasValue)
		{
			return View(new MoviesByAuthorModel());
		}

		var newbieFlag = newbies?.ToLower();
		var newbiesOnly = newbieFlag == "only";

		var model = new MoviesByAuthorModel
		{
			MarkNewbies = newbieFlag == "show",
			ShowClasses = showTiers,
			Publications = await db.Publications
				.ForDateRange(before.Value, after.Value)
				.Select(p => new MoviesByAuthorModel.PublicationEntry
				{
					Id = p.Id,
					Title = p.Title,
					Authors = p.Authors.OrderBy(pa => pa.Ordinal).Select(pa => pa.Author!.UserName),
					PublicationClassIconPath = p.PublicationClass!.IconPath
				})
				.ToListAsync()
		};

		if (newbiesOnly || model.MarkNewbies)
		{
			model.NewbieAuthors = await db.Users
				.ThatArePublishedAuthors()
				.Where(u => u.Publications
					.OrderBy(p => p.Publication!.CreateTimestamp)
					.First().Publication!.CreateTimestamp.Year == after.Value.Year)
				.Select(u => u.UserName)
				.ToListAsync();
		}

		if (newbiesOnly)
		{
			model.Publications = model.Publications
				.Where(p => p.Authors.Any(a => model.NewbieAuthors.Contains(a)))
				.ToList();
		}

		return View(model);
	}
}
