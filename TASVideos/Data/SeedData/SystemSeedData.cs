using TASVideos.Data.Entity.Game;

namespace TASVideos.Data.SeedData
{
    public class SystemSeedData
	{
		public const string NTSC = "NTSC";
		public const string PAL = "PAL";

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
			},
			new GameSystem
			{
				Id = 4,
				Code = "GB",
				DisplayName = "Game Boy"
			},
			new GameSystem
			{
				Id = 5,
				Code = "SGB",
				DisplayName = "Super Game Boy"
			},
			new GameSystem
			{
				Id = 6,
				Code = "GBC",
				DisplayName = "Game Boy Color"
			},
			new GameSystem
			{
				Id = 7,
				Code = "GBA",
				DisplayName = "Game Boy Advance"
			}
		};

		public static GameSystemFrameRate[] SystemFrameRates =
		{
			new GameSystemFrameRate
			{
				GameSystemId = 1,
				RegionCode = NTSC,
				FrameRate = 60.0988138974405
			},
			new GameSystemFrameRate
			{
				GameSystemId = 1,
				RegionCode = PAL,
				FrameRate = 50.0069789081886
			},
			new GameSystemFrameRate
			{
				GameSystemId = 2,
				RegionCode = NTSC,
				FrameRate = 60.0988138974405
			},
			new GameSystemFrameRate
			{
				GameSystemId = 2,
				RegionCode = PAL,
				FrameRate = 50.0069789081886
			},
			new GameSystemFrameRate
			{
				GameSystemId = 3,
				RegionCode = NTSC,
				FrameRate = 59.922751013550524
			},
			new GameSystemFrameRate
			{
				GameSystemId = 3,
				RegionCode = PAL,
				FrameRate = 49.70146011994842
			},
			new GameSystemFrameRate
			{
				GameSystemId = 4,
				RegionCode = NTSC,
				FrameRate = 59.7275005696058
			},
			new GameSystemFrameRate
			{
				GameSystemId = 5,
				RegionCode = NTSC,
				FrameRate = 60.0988138974405
			},
			new GameSystemFrameRate
			{
				GameSystemId = 5,
				RegionCode = PAL,
				FrameRate = 50.0069789081886
			},
			new GameSystemFrameRate
			{
				GameSystemId = 6,
				RegionCode = NTSC,
				FrameRate = 59.7275005696058
			},
			new GameSystemFrameRate
			{
				GameSystemId = 7,
				RegionCode = NTSC,
				FrameRate = 59.7275005696058
			},
		};
	}
}
