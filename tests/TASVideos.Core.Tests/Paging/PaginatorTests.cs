using TASVideos.Data.Entity.Game;

namespace TASVideos.Core.Tests.Paging;

[TestClass]
public class PaginatorTests : TestDbBase
{
	[TestMethod]
	public async Task SortedPageOf_DeterministicOrdering()
	{
		const int AddCount = 30;
		const int MaxSuffix = 4; // forces duplicates

		for (int i = 0; i < AddCount; i++)
		{
			_db.Games.Add(new Game
			{
				DisplayName = $"Game {Random.Shared.Next(MaxSuffix)}"
			});
		}

		await _db.SaveChangesAsync();

		HashSet<int> pageResults = [];

		for (int i = 0; i < AddCount; i++)
		{
			var page = await _db.Games
				.Select(g => new GameEntry
				{
					Id = g.Id,
					DisplayName = g.DisplayName
				})
				.SortedPageOf(new PagingModel
				{
					Sort = nameof(GameEntry.DisplayName),
					PageSize = 1,
					CurrentPage = i + 1
				});

			pageResults.Add(page.First().Id);
		}

		// if the ordering is deterministic, each id should appear exactly once, so the count should be equal to the number of entries added
		Assert.HasCount(AddCount, pageResults);
	}

	private class GameEntry
	{
		public int Id { get; set; }
		[Sortable]
		public string DisplayName { get; set; } = "";
	}
}
