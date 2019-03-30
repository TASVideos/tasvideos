using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.ViewComponents.Models;

namespace TASVideos.ViewComponents
{
	public class FirstEditionTas : ViewComponent
	{
		private readonly ApplicationDbContext _db;

		public FirstEditionTas(ApplicationDbContext db)
		{
			_db = db;
		}

		public async Task<IViewComponentResult> InvokeAsync(WikiPage pageData, string pp)
		{
			// TODO: add tier argument, default to moon,stars
			int before = ParamHelper.GetYear(pp, "before")
				?? DateTime.UtcNow.Year;
			int after = ParamHelper.GetYear(pp, "after")
				?? DateTime.UtcNow.AddYears(1).Year;

			var beforeYear = new DateTime(before, 1, 1);
			var afterYear = new DateTime(after, 1, 1);

			var firstEditions = await _db.Publications
				.GroupBy(
					gkey => new { gkey.GameId },
					gvalue => new { gvalue.Id, gvalue.Submission.CreateTimeStamp })
				.Select(g => new FirstEditionGames
				{
					GameId = g.Key.GameId,
					PublicationDate = g.Min(gg => gg.CreateTimeStamp)
				})
				.Where(g => g.PublicationDate >= afterYear)
				.Where(g => g.PublicationDate < beforeYear)
				.ToListAsync();

			var firstEditionGames = firstEditions.Select(f => f.GameId).ToList();

			// TODO: first edition logic
			var pubs = await _db.Publications
				.Where(p => p.Tier.Weight >= 1) // Exclude Vault
				.Where(p => p.CreateTimeStamp >= afterYear)
				.Where(p => p.CreateTimeStamp < beforeYear)
				.Where(p => firstEditionGames.Contains(p.GameId))
				.Select(p => new FirstEditionModel
				{
					Id = p.Id,
					Title = p.Title,
					GameId = p.GameId,
					TierId = p.TierId,
					TierIconPath = p.Tier.IconPath,
					TierName = p.Tier.Name,
					PublicationDate = p.CreateTimeStamp
				})
				.ToListAsync();

			// If multiple first editions in the same year, go with the first
			var model = pubs
				.GroupBy(g => new { g.GameId })
				.Select(g => g.OrderBy(gg => gg.PublicationDate).First())
				.ToList();

			return View(model);
		}

		private class FirstEditionGames
		{
			public int GameId { get; set; }
			public DateTime? PublicationDate { get; set; }
		}
	}
}
