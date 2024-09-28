using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.PublicationsByPlatform)]
public class PublicationsByPlatform(IGameSystemService platforms, IClassService classes) : WikiViewComponent
{
	public IReadOnlyList<(string DisplayName, string Code)> Platforms { get; private set; } = null!;

	public IReadOnlyCollection<PublicationClass> PubClasses { get; set; } = null!;

	public async Task<IViewComponentResult> InvokeAsync(IList<string> groupings)
	{
		var extant = (await platforms.GetAll()).ToList();
		List<IReadOnlyList<SystemsResponse>> rows = [];
		void ProcessGroup(string groupStr)
		{
			List<SystemsResponse> row = [];
			foreach (var idStr in groupStr.Split('-'))
			{
				var found = extant.FirstOrDefault(sys => sys.Code.Equals(idStr, StringComparison.OrdinalIgnoreCase));
				if (found is null)
				{
					// ignore, TODO log?
					return;
				}

				extant.Remove(found);
				row.Add(found);
			}

			rows.Add(row);
		}

		foreach (var groupStr in groupings)
		{
			ProcessGroup(groupStr);
		}

		Platforms = extant.Select(static sys => (sys.DisplayName, sys.Code))
			.Concat(rows.Select(static row => (
				DisplayName: string.Join(" / ", row.Select(static sys => sys.DisplayName)),
				Code: string.Join("-", row.Select(static sys => sys.Code))
			)))
			.OrderBy(static tuple => tuple.DisplayName)
			.ToArray();
		PubClasses = await classes.GetAll();
		return View();
	}
}
