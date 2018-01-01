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
			},
			new GameSystem
			{
				Id = 8,
				Code = "N64",
				DisplayName = "Nintendo 64"
			},
			new GameSystem
			{
				Id = 9,
				Code = "DOS",
				DisplayName = "DOS"
			},
			new GameSystem
			{
				Id = 10,
				Code = "SMS",
				DisplayName = "Sega MasterSystem"
			},
			new GameSystem
			{
				Id = 11,
				Code = "PSX",
				DisplayName = "Sony PlayStation"
			},
			new GameSystem
			{
				Id = 12,
				Code = "PCE",
				DisplayName = "TurboGrafx 16"
			},
			new GameSystem
			{
				Id = 13,
				Code = "WSWAN",
				DisplayName = "WonderSwan"
			},
			new GameSystem
			{
				Id = 14,
				Code = "PCFX",
				DisplayName = "PC-FX"
			},
			new GameSystem
			{
				Id = 15,
				Code = "NGP",
				DisplayName = "Neo Geo Pocket"
			},
			new GameSystem
			{
				Id = 16,
				Code = "Lynx",
				DisplayName = "Atari Lynx"
			},
			new GameSystem
			{
				Id = 17,
				Code = "DS",
				DisplayName = "Nintendo DS"
			},
			new GameSystem
			{
				Id = 18,
				Code = "GG",
				DisplayName = "Game Gear"
			},
			new GameSystem
			{
				Id = 19,
				Code = "Arcade",
				DisplayName = "Arcade"
			},
			new GameSystem
			{
				Id = 20,
				Code = "Saturn",
				DisplayName = "Sega Saturn"
			},
			new GameSystem
			{
				Id = 21,
				Code = "32X",
				DisplayName = "Sega 32X"
			},
			new GameSystem
			{
				Id = 22,
				Code = "SegaCD",
				DisplayName = "Sega CD"
			},
			new GameSystem
			{
				Id = 23,
				Code = "FDS",
				DisplayName = "Famicom Disk System"
			},
			new GameSystem
			{
				Id = 24,
				Code = "PCECD",
				DisplayName = "TurboGrafx 16 CD"
			},
			new GameSystem
			{
				Id = 25,
				Code = "Vboy",
				DisplayName = "Virtual Boy"
			},
			new GameSystem
			{
				Id = 26,
				Code = "MSX",
				DisplayName = "MSX Home Computer System"
			},
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
			new GameSystemFrameRate
			{
				GameSystemId = 8,
				RegionCode = NTSC,
				FrameRate = 60,
				Preliminary = true
			},
			new GameSystemFrameRate
			{
				GameSystemId = 8,
				RegionCode = PAL,
				FrameRate = 50,
				Preliminary = true
			},
			new GameSystemFrameRate
			{
				GameSystemId = 9,
				RegionCode = NTSC,
				FrameRate = 70.0863028953229
			},
			new GameSystemFrameRate
			{
				GameSystemId = 10,
				RegionCode = NTSC,
				FrameRate = 59.9227510135505
			},
			new GameSystemFrameRate
			{
				GameSystemId = 10,
				RegionCode = PAL,
				FrameRate = 49.70146011994839
			},
			new GameSystemFrameRate
			{
				GameSystemId = 11,
				RegionCode = NTSC,
				FrameRate = 59.29286256195557
			},
			new GameSystemFrameRate
			{
				GameSystemId = 11,
				RegionCode = PAL,
				FrameRate = 49.764559357596745
			},
			new GameSystemFrameRate
			{
				GameSystemId = 12,
				RegionCode = NTSC,
				FrameRate = 59.8261054534819
			},
			new GameSystemFrameRate
			{
				GameSystemId = 13,
				RegionCode = NTSC,
				FrameRate = 75.4716981132075
			},
			new GameSystemFrameRate
			{
				GameSystemId = 14,
				RegionCode = NTSC,
				FrameRate = 59.8261054534819
			},
			new GameSystemFrameRate
			{
				GameSystemId = 15,
				RegionCode = NTSC,
				FrameRate = 60.2530155928214
			},
			new GameSystemFrameRate
			{
				GameSystemId = 16,
				RegionCode = NTSC,
				FrameRate = 59.89817311
			},
			new GameSystemFrameRate
			{
				GameSystemId = 17,
				RegionCode = NTSC,
				FrameRate = 59.82609828808082
			},
			new GameSystemFrameRate
			{
				GameSystemId = 18,
				RegionCode = NTSC,
				FrameRate = 59.9227510135505
			},
			new GameSystemFrameRate
			{
				GameSystemId = 19,
				RegionCode = NTSC,
				FrameRate = 60,
				Preliminary = true
			},
			new GameSystemFrameRate
			{
				GameSystemId = 19,
				RegionCode = PAL,
				FrameRate = 50,
				Preliminary = true
			},
			new GameSystemFrameRate
			{
				GameSystemId = 20,
				RegionCode = NTSC,
				FrameRate = 60,
				Preliminary = true
			},
			new GameSystemFrameRate
			{
				GameSystemId = 20,
				RegionCode = PAL,
				FrameRate = 50,
				Preliminary = true
			},
			new GameSystemFrameRate
			{
				GameSystemId = 21,
				RegionCode = NTSC,
				FrameRate = 59.922751013550524
			},
			new GameSystemFrameRate
			{
				GameSystemId = 21,
				RegionCode = PAL,
				FrameRate = 49.70146011994842
			},
			new GameSystemFrameRate
			{
				GameSystemId = 22,
				RegionCode = NTSC,
				FrameRate = 59.922751013550524
			},
			new GameSystemFrameRate
			{
				GameSystemId = 22,
				RegionCode = PAL,
				FrameRate = 49.70146011994842
			},
			new GameSystemFrameRate
			{
				GameSystemId = 23,
				RegionCode = NTSC,
				FrameRate = 60.0988138974405
			},
			new GameSystemFrameRate
			{
				GameSystemId = 24,
				RegionCode = NTSC,
				FrameRate = 59.8261054534819
			},
			new GameSystemFrameRate
			{
				GameSystemId = 25,
				RegionCode = NTSC,
				FrameRate = 50.2734877734878
			},
			new GameSystemFrameRate
			{
				GameSystemId = 26,
				RegionCode = NTSC,
				FrameRate = 59.9227510135505
			},
			new GameSystemFrameRate
			{
				GameSystemId = 26,
				RegionCode = PAL,
				FrameRate = 50.158975804566104
			},
		};
	}
}
