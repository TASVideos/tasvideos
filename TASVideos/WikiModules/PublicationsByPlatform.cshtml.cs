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
		rows.AddRange(groupings
			.Select(groupStr => ProcessGroup(extant, groupStr))
			.OfType<List<SystemsResponse>>());

		Platforms = extant
			.Select(sys => (sys.DisplayName, sys.Code))
			.Concat(rows.Select(row => (
				DisplayName: string.Join(" / ", row.Select(sys => sys.DisplayName)),
				Code: string.Join("-", row.Select(sys => sys.Code))
			)))
			.OrderBy(tuple => tuple.DisplayName)
			.ToArray();
		PubClasses = await classes.GetAll();

		return View();
	}

	private static List<SystemsResponse>? ProcessGroup(List<SystemsResponse> extant, string groupStr)
	{
		List<SystemsResponse> row = [];
		foreach (var idStr in groupStr.Split('-'))
		{
			var found = extant.FirstOrDefault(sys => sys.Code.Equals(idStr, StringComparison.OrdinalIgnoreCase));
			if (found is null)
			{
				// ignore, TODO log?
				return null;
			}

			extant.Remove(found);
			row.Add(found);
		}

		return row;
	}
}
