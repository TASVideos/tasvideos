using TASVideos.Data.Entity.Game;

// ReSharper disable StyleCop.SA1401
namespace TASVideos.Data.SeedData
{
	public class SystemSeedData
	{
		public const string Ntsc = "NTSC";
		public const string Pal = "PAL";

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
				Id = 38,
				Code = "Linux",
				DisplayName = "Linux"
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
			new GameSystem
			{
				Id = 44,
				Code = "ZXS",
				DisplayName = "ZX Spectrum"
			},
		};

		public static GameSystemFrameRate[] SystemFrameRates =
		{
			new GameSystemFrameRate
			{
				GameSystemId = 1,
				RegionCode = Ntsc,
				FrameRate = 60.0988138974405
			},
			new GameSystemFrameRate
			{
				GameSystemId = 1,
				RegionCode = Pal,
				FrameRate = 50.0069789081886
			},
			new GameSystemFrameRate
			{
				GameSystemId = 1,
				RegionCode = Ntsc + "60",
				FrameRate = 60
			},
			new GameSystemFrameRate
			{
				GameSystemId = 1,
				RegionCode = Pal + "50",
				FrameRate = 50
			},
			new GameSystemFrameRate
			{
				GameSystemId = 2,
				RegionCode = Ntsc,
				FrameRate = 60.0988138974405
			},
			new GameSystemFrameRate
			{
				GameSystemId = 2,
				RegionCode = Pal,
				FrameRate = 50.0069789081886
			},
			new GameSystemFrameRate
			{
				GameSystemId = 2,
				RegionCode = Ntsc + "60",
				FrameRate = 60
			},
			new GameSystemFrameRate
			{
				GameSystemId = 2,
				RegionCode = Pal + "50",
				FrameRate = 50
			},
			new GameSystemFrameRate
			{
				GameSystemId = 3,
				RegionCode = Ntsc,
				FrameRate = 59.922751013550524
			},
			new GameSystemFrameRate
			{
				GameSystemId = 3,
				RegionCode = Pal,
				FrameRate = 49.70146011994842
			},
			new GameSystemFrameRate
			{
				GameSystemId = 3,
				RegionCode = Ntsc + "60",
				FrameRate = 60
			},
			new GameSystemFrameRate
			{
				GameSystemId = 3,
				RegionCode = Pal + "50",
				FrameRate = 50
			},
			new GameSystemFrameRate
			{
				GameSystemId = 4,
				RegionCode = Ntsc,
				FrameRate = 59.7275005696058
			},
			new GameSystemFrameRate
			{
				GameSystemId = 4,
				RegionCode = Ntsc + "60",
				FrameRate = 60
			},
			new GameSystemFrameRate
			{
				GameSystemId = 5,
				RegionCode = Ntsc,
				FrameRate = 60.0988138974405
			},
			new GameSystemFrameRate
			{
				GameSystemId = 5,
				RegionCode = Pal,
				FrameRate = 50.0069789081886
			},
			new GameSystemFrameRate
			{
				GameSystemId = 5,
				RegionCode = Ntsc + "60",
				FrameRate = 60
			},
			new GameSystemFrameRate
			{
				GameSystemId = 6,
				RegionCode = Ntsc,
				FrameRate = 59.7275005696058
			},
			new GameSystemFrameRate
			{
				GameSystemId = 6,
				RegionCode = Ntsc + "60",
				FrameRate = 60
			},
			new GameSystemFrameRate
			{
				GameSystemId = 7,
				RegionCode = Ntsc,
				FrameRate = 59.7275005696058
			},
			new GameSystemFrameRate
			{
				GameSystemId = 7,
				RegionCode = Ntsc + "60",
				FrameRate = 60
			},
			new GameSystemFrameRate
			{
				GameSystemId = 8,
				RegionCode = Ntsc,
				FrameRate = 60,
				Preliminary = true
			},
			new GameSystemFrameRate
			{
				GameSystemId = 8,
				RegionCode = Pal,
				FrameRate = 50,
				Preliminary = true
			},
			new GameSystemFrameRate
			{
				GameSystemId = 9,
				RegionCode = Ntsc,
				FrameRate = 70.0863028953229
			},
			new GameSystemFrameRate
			{
				GameSystemId = 9,
				RegionCode = Ntsc + "60",
				FrameRate = 60
			},
			new GameSystemFrameRate
			{
				GameSystemId = 10,
				RegionCode = Ntsc,
				FrameRate = 59.9227510135505
			},
			new GameSystemFrameRate
			{
				GameSystemId = 10,
				RegionCode = Pal,
				FrameRate = 49.70146011994839
			},
			new GameSystemFrameRate
			{
				GameSystemId = 10,
				RegionCode = Ntsc + "60",
				FrameRate = 60
			},
			new GameSystemFrameRate
			{
				GameSystemId = 10,
				RegionCode = Pal + "50",
				FrameRate = 50
			},
			new GameSystemFrameRate
			{
				GameSystemId = 11,
				RegionCode = Ntsc,
				FrameRate = 59.29286256195557
			},
			new GameSystemFrameRate
			{
				GameSystemId = 11,
				RegionCode = Pal,
				FrameRate = 49.764559357596745
			},
			new GameSystemFrameRate
			{
				GameSystemId = 11,
				RegionCode = Ntsc + "60",
				FrameRate = 60
			},
			new GameSystemFrameRate
			{
				GameSystemId = 11,
				RegionCode = Pal + "50",
				FrameRate = 50
			},
			new GameSystemFrameRate
			{
				GameSystemId = 12,
				RegionCode = Ntsc,
				FrameRate = 59.8261054534819
			},
			new GameSystemFrameRate
			{
				GameSystemId = 13,
				RegionCode = Ntsc,
				FrameRate = 75.4716981132075
			},
			new GameSystemFrameRate
			{
				GameSystemId = 14,
				RegionCode = Ntsc,
				FrameRate = 59.8261054534819
			},
			new GameSystemFrameRate
			{
				GameSystemId = 15,
				RegionCode = Ntsc,
				FrameRate = 60.2530155928214
			},
			new GameSystemFrameRate
			{
				GameSystemId = 16,
				RegionCode = Ntsc,
				FrameRate = 59.89817311
			},
			new GameSystemFrameRate
			{
				GameSystemId = 16,
				RegionCode = Ntsc + "60",
				FrameRate = 60
			},
			new GameSystemFrameRate
			{
				GameSystemId = 17,
				RegionCode = Ntsc,
				FrameRate = 59.82609828808082
			},
			new GameSystemFrameRate
			{
				GameSystemId = 17,
				RegionCode = Ntsc + "60",
				FrameRate = 60
			},
			new GameSystemFrameRate
			{
				GameSystemId = 18,
				RegionCode = Ntsc,
				FrameRate = 59.9227510135505
			},
			new GameSystemFrameRate
			{
				GameSystemId = 18,
				RegionCode = Ntsc + "60",
				FrameRate = 60
			},
			new GameSystemFrameRate
			{
				GameSystemId = 19,
				RegionCode = Ntsc,
				FrameRate = 60,
				Preliminary = true
			},
			new GameSystemFrameRate
			{
				GameSystemId = 19,
				RegionCode = Pal,
				FrameRate = 50,
				Preliminary = true
			},
			new GameSystemFrameRate
			{
				GameSystemId = 20,
				RegionCode = Ntsc,
				FrameRate = 60,
				Preliminary = true
			},
			new GameSystemFrameRate
			{
				GameSystemId = 20,
				RegionCode = Pal,
				FrameRate = 50,
				Preliminary = true
			},
			new GameSystemFrameRate
			{
				GameSystemId = 21,
				RegionCode = Ntsc,
				FrameRate = 59.922751013550524
			},
			new GameSystemFrameRate
			{
				GameSystemId = 21,
				RegionCode = Pal,
				FrameRate = 49.70146011994842
			},
			new GameSystemFrameRate
			{
				GameSystemId = 21,
				RegionCode = Ntsc + "60",
				FrameRate = 60
			},
			new GameSystemFrameRate
			{
				GameSystemId = 22,
				RegionCode = Ntsc,
				FrameRate = 59.922751013550524
			},
			new GameSystemFrameRate
			{
				GameSystemId = 22,
				RegionCode = Pal,
				FrameRate = 49.70146011994842
			},
			new GameSystemFrameRate
			{
				GameSystemId = 22,
				RegionCode = Ntsc + "60",
				FrameRate = 60
			},
			new GameSystemFrameRate
			{
				GameSystemId = 23,
				RegionCode = Ntsc,
				FrameRate = 60.0988138974405
			},
			new GameSystemFrameRate
			{
				GameSystemId = 23,
				RegionCode = Ntsc + "60",
				FrameRate = 60
			},
			new GameSystemFrameRate
			{
				GameSystemId = 24,
				RegionCode = Ntsc,
				FrameRate = 59.8261054534819
			},
			new GameSystemFrameRate
			{
				GameSystemId = 25,
				RegionCode = Ntsc,
				FrameRate = 50.2734877734878
			},
			new GameSystemFrameRate
			{
				GameSystemId = 26,
				RegionCode = Ntsc,
				FrameRate = 59.9227510135505
			},
			new GameSystemFrameRate
			{
				GameSystemId = 26,
				RegionCode = Pal,
				FrameRate = 50.158975804566104
			},
			new GameSystemFrameRate
			{
				GameSystemId = 27,
				RegionCode = Ntsc,
				FrameRate = 60,
				Preliminary = true
			},
			new GameSystemFrameRate
			{
				GameSystemId = 27,
				RegionCode = Pal,
				FrameRate = 50,
				Preliminary = true
			},
			new GameSystemFrameRate
			{
				GameSystemId = 28,
				RegionCode = Ntsc,
				FrameRate = 60,
				Preliminary = true
			},
			new GameSystemFrameRate
			{
				GameSystemId = 28,
				RegionCode = Pal,
				FrameRate = 50,
				Preliminary = true
			},
			new GameSystemFrameRate
			{
				GameSystemId = 29,
				RegionCode = Ntsc,
				FrameRate = 60,
				Preliminary = true
			},
			new GameSystemFrameRate
			{
				GameSystemId = 30,
				RegionCode = Ntsc,
				FrameRate = 59.9227510135505
			},
			new GameSystemFrameRate
			{
				GameSystemId = 30,
				RegionCode = Pal,
				FrameRate = 49.70146011994839
			},
			new GameSystemFrameRate
			{
				GameSystemId = 31,
				RegionCode = Ntsc,
				FrameRate = 60,
				Preliminary = true
			},
			new GameSystemFrameRate
			{
				GameSystemId = 32,
				RegionCode = Ntsc,
				FrameRate = 59.8261054534819
			},
			new GameSystemFrameRate
			{
				GameSystemId = 33,
				RegionCode = Ntsc,
				FrameRate = 35.0029869215506
			},
			new GameSystemFrameRate
			{
				GameSystemId = 34,
				RegionCode = Ntsc,
				FrameRate = 59.9227510135505
			},
			new GameSystemFrameRate
			{
				GameSystemId = 34,
				RegionCode = Pal,
				FrameRate = 49.8607596716149
			},
			new GameSystemFrameRate
			{
				GameSystemId = 34,
				RegionCode = Ntsc + "60",
				FrameRate = 60
			},
			new GameSystemFrameRate
			{
				GameSystemId = 35,
				RegionCode = Ntsc,
				FrameRate = 59.9227510135505
			},
			new GameSystemFrameRate
			{
				GameSystemId = 35,
				RegionCode = Pal,
				FrameRate = 49.70146011994839
			},
			new GameSystemFrameRate
			{
				GameSystemId = 35,
				RegionCode = Ntsc + "60",
				FrameRate = 60
			},
			new GameSystemFrameRate
			{
				GameSystemId = 36,
				RegionCode = Ntsc,
				FrameRate = 59.9227510135505
			},
			new GameSystemFrameRate
			{
				GameSystemId = 36,
				RegionCode = Pal,
				FrameRate = 49.70146011994839
			},
			new GameSystemFrameRate
			{
				GameSystemId = 37,
				RegionCode = Ntsc,
				FrameRate = 59.826089499853765
			},
			new GameSystemFrameRate
			{
				GameSystemId = 37,
				RegionCode = Pal,
				FrameRate = 50.1245421245421
			},
			new GameSystemFrameRate
			{
				GameSystemId = 38,
				RegionCode = Ntsc,
				FrameRate = 60
			},
			new GameSystemFrameRate
			{
				GameSystemId = 41,
				RegionCode = Ntsc,
				FrameRate = 60,
				Preliminary = true
			},
			new GameSystemFrameRate
			{
				GameSystemId = 41,
				RegionCode = Pal,
				FrameRate = 50,
				Preliminary = true
			},
			new GameSystemFrameRate
			{
				GameSystemId = 42,
				RegionCode = Ntsc,
				FrameRate = 59.92
			},
			new GameSystemFrameRate
			{
				GameSystemId = 43,
				RegionCode = Ntsc,
				FrameRate = 60.01631993960238
			},
			new GameSystemFrameRate
			{
				GameSystemId = 44,
				RegionCode = Pal,
				FrameRate = 50.080128205
			},
		};
	}
}
