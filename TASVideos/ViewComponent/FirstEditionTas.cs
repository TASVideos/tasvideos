using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.ViewComponents.Models;
using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents
{
	[WikiModule(WikiModules.FirstEditionTas)]
	public class FirstEditionTas : ViewComponent
	{
		private readonly ApplicationDbContext _db;

		public FirstEditionTas(ApplicationDbContext db)
		{
			_db = db;
		}

		public async Task<IViewComponentResult> InvokeAsync(DateTime? before, DateTime? after, bool splitbyplatform)
		{
			// TODO: add tier argument, default to moon,stars,
			// we want to avoid baking in "business logic" like which tiers are award eligible
			var beforeYear = before ?? new DateTime(DateTime.UtcNow.Year, 1, 1);
			var afterYear = after ?? new DateTime(DateTime.UtcNow.AddYears(1).Year, 1, 1);

			List<FirstEditionGames> firstEditions;

			if (splitbyplatform)
			{
				firstEditions = await _db.Publications
					.GroupBy(
						gkey => new { gkey.GameId },
						gvalue => new { gvalue.Id, gvalue.Submission!.CreateTimeStamp })
					.Select(g => new FirstEditionGames
					{
						GameId = g.Key.GameId,
						PublicationDate = g.Min(gg => gg.CreateTimeStamp)
					})
					.Where(g => g.PublicationDate >= afterYear)
					.Where(g => g.PublicationDate < beforeYear)
					.ToListAsync();
			}
			else
			{
				firstEditions = await _db.Publications
					.GroupBy(
						gkey => new { gkey.Game!.DisplayName },
						gvalue => new { gvalue.Id, gvalue.Submission!.CreateTimeStamp })
					.Select(g => new FirstEditionGames
					{
						GameName = g.Key.DisplayName,
						PublicationDate = g.Min(gg => gg.CreateTimeStamp)
					})
					.Where(g => g.PublicationDate >= afterYear)
					.Where(g => g.PublicationDate < beforeYear)
					.ToListAsync();
			}

			var query = _db.Publications
				.Where(p => p.Tier!.Weight >= 1) // Exclude Vault
				.Where(p => p.CreateTimeStamp >= afterYear)
				.Where(p => p.CreateTimeStamp < beforeYear);

			if (splitbyplatform)
			{
				var firstEditionIds = firstEditions.Select(f => f.GameId).ToList();
				query = query.Where(p => firstEditionIds.Contains(p.GameId));
			}
			else
			{
				var firstEditionNames = firstEditions.Select(f => f.GameName).ToList();
				query = query.Where(p => firstEditionNames.Contains(p.Game!.DisplayName));
			}

			// TODO: first edition logic
			var pubs = await query
				.Select(p => new FirstEditionModel
				{
					Id = p.Id,
					Title = p.Title,
					GameId = p.GameId,
					TierId = p.TierId,
					TierIconPath = p.Tier!.IconPath,
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
			public int GameId { get; init; }
			public string GameName { get; init; } = "";
			public DateTime? PublicationDate { get; init; }
		}
	}
}
