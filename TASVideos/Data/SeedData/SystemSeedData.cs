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

		public static GameSystemFrameRate[] SystemFrameRates =
		{
			new GameSystemFrameRate
			{
				GameSystemId = 1,
				RegionCode = "NTSC",
				FrameRate = 60.0988138974405
			},
			new GameSystemFrameRate
			{
				GameSystemId = 1,
				RegionCode = "PAL",
				FrameRate = 50.0069789081886
			},
			new GameSystemFrameRate
			{
				GameSystemId = 2,
				RegionCode = "NTSC",
				FrameRate = 60.0988138974405
			},
			new GameSystemFrameRate
			{
				GameSystemId = 2,
				RegionCode = "PAL",
				FrameRate = 50.0069789081886
			},
			new GameSystemFrameRate
			{
				GameSystemId = 3,
				RegionCode = "NTSC",
				FrameRate = 59.922751013550524
			},
			new GameSystemFrameRate
			{
				GameSystemId = 3,
				RegionCode = "PAL",
				FrameRate = 49.70146011994842
			}
		};
	}
}
