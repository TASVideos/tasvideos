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
			new GameSystem
			{
				Id = 27,
				Code = "GC",
				DisplayName = "Nintendo GameCube"
			},
			new GameSystem
			{
				Id = 28,
				Code = "Wii",
				DisplayName = "Nintendo Wii"
			},
			new GameSystem
			{
				Id = 29,
				Code = "Windows",
				DisplayName = "Windows"
			},
			new GameSystem
			{
				Id = 30,
				Code = "SG1000",
				DisplayName = "Sega SG-1000"
			},
			new GameSystem
			{
				Id = 31,
				Code = "TI83",
				DisplayName = "Texas Instruments TI-83 Series"
			},
			new GameSystem
			{
				Id = 32,
				Code = "SGX",
				DisplayName = "SuperGrafx"
			},
			new GameSystem
			{
				Id = 33,
				Code = "DOOM",
				DisplayName = "DooM"
			},
			new GameSystem
			{
				Id = 34,
				Code = "A2600",
				DisplayName = "Atari 2600"
			},
			new GameSystem
			{
				Id = 35,
				Code = "Coleco",
				DisplayName = "ColecoVision"
			},
			new GameSystem
			{
				Id = 36,
				Code = "A7800",
				DisplayName = "Atari 7800"
			},
			new GameSystem
			{
				Id = 37,
				Code = "C64",
				DisplayName = "Commodore 64"
			},
			new GameSystem
			{
				Id = 41,
				Code = "AppleII",
				DisplayName = "Apple II"
			},
			new GameSystem
			{
				Id = 42,
				Code = "INTV",
				DisplayName = "Intellivision"
			},
			new GameSystem
			{
				Id = 43,
				Code = "Uzebox",
				DisplayName = "Uzebox"
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
			new GameSystemFrameRate
			{
				GameSystemId = 27,
				RegionCode = NTSC,
				FrameRate = 60,
				Preliminary = true
			},
			new GameSystemFrameRate
			{
				GameSystemId = 27,
				RegionCode = PAL,
				FrameRate = 50,
				Preliminary = true
			},
			new GameSystemFrameRate
			{
				GameSystemId = 28,
				RegionCode = NTSC,
				FrameRate = 60,
				Preliminary = true
			},
			new GameSystemFrameRate
			{
				GameSystemId = 28,
				RegionCode = PAL,
				FrameRate = 50,
				Preliminary = true
			},
			new GameSystemFrameRate
			{
				GameSystemId = 29,
				RegionCode = NTSC,
				FrameRate = 60,
				Preliminary = true
			},
			new GameSystemFrameRate
			{
				GameSystemId = 30,
				RegionCode = NTSC,
				FrameRate = 59.9227510135505
			},
			new GameSystemFrameRate
			{
				GameSystemId = 30,
				RegionCode = PAL,
				FrameRate = 49.70146011994839
			},
			new GameSystemFrameRate
			{
				GameSystemId = 31,
				RegionCode = NTSC,
				FrameRate = 60,
				Preliminary = true
			},
			new GameSystemFrameRate
			{
				GameSystemId = 32,
				RegionCode = NTSC,
				FrameRate = 59.8261054534819
			},
			new GameSystemFrameRate
			{
				GameSystemId = 33,
				RegionCode = NTSC,
				FrameRate = 35.0029869215506
			},
			new GameSystemFrameRate
			{
				GameSystemId = 34,
				RegionCode = NTSC,
				FrameRate = 59.9227510135505
			},
			new GameSystemFrameRate
			{
				GameSystemId = 34,
				RegionCode = PAL,
				FrameRate = 49.8607596716149
			},
			new GameSystemFrameRate
			{
				GameSystemId = 35,
				RegionCode = NTSC,
				FrameRate = 59.9227510135505
			},
			new GameSystemFrameRate
			{
				GameSystemId = 35,
				RegionCode = PAL,
				FrameRate = 49.70146011994839
			},
			new GameSystemFrameRate
			{
				GameSystemId = 36,
				RegionCode = NTSC,
				FrameRate = 59.9227510135505
			},
			new GameSystemFrameRate
			{
				GameSystemId = 36,
				RegionCode = PAL,
				FrameRate = 49.70146011994839
			},
			new GameSystemFrameRate
			{
				GameSystemId = 37,
				RegionCode = NTSC,
				FrameRate = 59.826089499853765
			},
			new GameSystemFrameRate
			{
				GameSystemId = 37,
				RegionCode = PAL,
				FrameRate = 50.1245421245421
			},
			new GameSystemFrameRate
			{
				GameSystemId = 41,
				RegionCode = NTSC,
				FrameRate = 60,
				Preliminary = true
			},
			new GameSystemFrameRate
			{
				GameSystemId = 41,
				RegionCode = PAL,
				FrameRate = 50,
				Preliminary = true
			},
			new GameSystemFrameRate
			{
				GameSystemId = 42,
				RegionCode = NTSC,
				FrameRate = 59.92
			},
			new GameSystemFrameRate
			{
				GameSystemId = 43,
				RegionCode = NTSC,
				FrameRate = 60.01631993960238
			},
		};
	}
}
