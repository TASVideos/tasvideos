using TASVideos.Data.Entity.Game;

namespace TASVideos.Data.SeedData
{
    public class SystemSeedData
	{
		public static GameSystem[] Systems =
		{
			new GameSystem
			{
				Id = 1,
				Code = "NES",
				DisplayName = "Nintendo Entertainment System"
			},
			new GameSystem
			{
				Id = 2,
				Code = "SNES",
				DisplayName = "Super NES"
			},
			new GameSystem
			{
				Id = 3,
				Code = "Genesis",
				DisplayName = "Sega Genesis"
			}
		};
	}
}
