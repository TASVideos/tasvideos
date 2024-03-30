using TASVideos.WikiModules.Models;
using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.CrossGameObsoletions)]

public class CrossGameObsoletions(ApplicationDbContext db) : ViewComponent
{
	public async Task<IViewComponentResult> InvokeAsync()
	{
		var obsoletionList = await db.Publications
			.Include(p => p.ObsoletedBy)
			.Include(p => p.Game)
			.Where(p => p.ObsoletedBy != null)
			.Select(p => new
			{
				p.GameId,
				p.Game!.DisplayName,
				ObsGameId = p.ObsoletedBy == null ? -1 : p.ObsoletedBy.GameId,
				ObsDisplayName = p.ObsoletedBy == null ? "" : p.ObsoletedBy.Game!.DisplayName,
			})
			.ToListAsync();

		var allGroups = new Dictionary<CrossGameObsoletionsModel.Entry, HashSet<CrossGameObsoletionsModel.Entry>>();
		var addedGames = new HashSet<CrossGameObsoletionsModel.Entry>();

		foreach (var cur in obsoletionList)
		{
			if (cur.ObsGameId != -1 && cur.GameId != cur.ObsGameId)
			{
				var entry1 = new CrossGameObsoletionsModel.Entry(cur.GameId, cur.DisplayName);
				var entry2 = new CrossGameObsoletionsModel.Entry(cur.ObsGameId, cur.ObsDisplayName);
				if (addedGames.Contains(entry1) || addedGames.Contains(entry2))
				{
					if (allGroups.ContainsKey(entry1))
					{
						addedGames.Add(entry2);
						allGroups[entry1].Add(entry2);
					}
					else if (allGroups.ContainsKey(entry2))
					{
						addedGames.Add(entry1);
						allGroups[entry2].Add(entry1);
					}
					else
					{
						foreach (var (k, v) in allGroups)
						{
							if (v.Contains(entry1) || v.Contains(entry2))
							{
								addedGames.Add(entry1);
								addedGames.Add(entry2);
								v.Add(entry1);
								v.Add(entry2);
								break;
							}
						}
					}
				}
				else
				{
					addedGames.Add(entry1);
					addedGames.Add(entry2);
					allGroups.Add(entry1, []);
					allGroups[entry1].Add(entry2);
				}
			}
		}

		var model = new CrossGameObsoletionsModel
		{
			AllObsoletionGroups = allGroups
		};
		return View(model);
	}
}
