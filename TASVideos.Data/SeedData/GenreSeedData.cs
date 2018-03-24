using TASVideos.Data.Entity.Game;

// ReSharper disable StyleCop.SA1401
namespace TASVideos.Data.SeedData
{
    public static class GenreSeedData
	{
		public static readonly Genre[] Genres =
		{
			new Genre { Id = 1, DisplayName = "Action" },
			new Genre { Id = 2, DisplayName = "Adventure" },
			new Genre { Id = 3, DisplayName = "Fighting" },
			new Genre { Id = 4, DisplayName = "Platform" },
			new Genre { Id = 5, DisplayName = "Puzzle" },
			new Genre { Id = 6, DisplayName = "Racing" },
			new Genre { Id = 7, DisplayName = "RPG" },
			new Genre { Id = 8, DisplayName = "Shooter" },
			new Genre { Id = 9, DisplayName = "Sport" },
			new Genre { Id = 10, DisplayName = "Storybook" },
			new Genre { Id = 11, DisplayName = "Strategy" },
			new Genre { Id = 12, DisplayName = "Board" },
			new Genre { Id = 13, DisplayName = "Gameshow" }
		};
	}
}
