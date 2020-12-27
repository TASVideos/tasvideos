using TASVideos.Data.Entity.Game;

namespace TASVideos.Data.SeedData
{
	public static class GenreSeedData
	{
		public static readonly Genre[] Genres =
		{
			new () { Id = 1, DisplayName = "Action" },
			new () { Id = 2, DisplayName = "Adventure" },
			new () { Id = 3, DisplayName = "Fighting" },
			new () { Id = 4, DisplayName = "Platform" },
			new () { Id = 5, DisplayName = "Puzzle" },
			new () { Id = 6, DisplayName = "Racing" },
			new () { Id = 7, DisplayName = "RPG" },
			new () { Id = 8, DisplayName = "Shooter" },
			new () { Id = 9, DisplayName = "Sport" },
			new () { Id = 10, DisplayName = "Storybook" },
			new () { Id = 11, DisplayName = "Strategy" },
			new () { Id = 12, DisplayName = "Board" },
			new () { Id = 13, DisplayName = "Gameshow" }
		};
	}
}
