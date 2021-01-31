using TASVideos.Data.Entity.Game;

namespace TASVideos.Data.SeedData
{
	public class SystemSeedData
	{
		public const string Ntsc = "NTSC";
		public const string Pal = "PAL";

		public static readonly GameSystem[] Systems =
		{
			new ()
			{
				Id = 1,
				Code = "NES",
				DisplayName = "Nintendo Entertainment System"
			},
			new ()
			{
				Id = 2,
				Code = "SNES",
				DisplayName = "Super NES"
			},
			new ()
			{
				Id = 3,
				Code = "Genesis",
				DisplayName = "Sega Genesis"
			},
			new ()
			{
				Id = 4,
				Code = "GB",
				DisplayName = "Game Boy"
			},
			new ()
			{
				Id = 5,
				Code = "SGB",
				DisplayName = "Super Game Boy"
			},
			new ()
			{
				Id = 6,
				Code = "GBC",
				DisplayName = "Game Boy Color"
			},
			new ()
			{
				Id = 7,
				Code = "GBA",
				DisplayName = "Game Boy Advance"
			},
			new ()
			{
				Id = 8,
				Code = "N64",
				DisplayName = "Nintendo 64"
			},
			new ()
			{
				Id = 9,
				Code = "DOS",
				DisplayName = "DOS"
			},
			new ()
			{
				Id = 10,
				Code = "SMS",
				DisplayName = "Sega MasterSystem"
			},
			new ()
			{
				Id = 11,
				Code = "PSX",
				DisplayName = "Sony PlayStation"
			},
			new ()
			{
				Id = 12,
				Code = "PCE",
				DisplayName = "TurboGrafx 16"
			},
			new ()
			{
				Id = 13,
				Code = "WSWAN",
				DisplayName = "WonderSwan"
			},
			new ()
			{
				Id = 14,
				Code = "PCFX",
				DisplayName = "PC-FX"
			},
			new ()
			{
				Id = 15,
				Code = "NGP",
				DisplayName = "Neo Geo Pocket"
			},
			new ()
			{
				Id = 16,
				Code = "Lynx",
				DisplayName = "Atari Lynx"
			},
			new ()
			{
				Id = 17,
				Code = "DS",
				DisplayName = "Nintendo DS"
			},
			new ()
			{
				Id = 18,
				Code = "GG",
				DisplayName = "Game Gear"
			},
			new ()
			{
				Id = 19,
				Code = "Arcade",
				DisplayName = "Arcade"
			},
			new ()
			{
				Id = 20,
				Code = "Saturn",
				DisplayName = "Sega Saturn"
			},
			new ()
			{
				Id = 21,
				Code = "32X",
				DisplayName = "Sega 32X"
			},
			new ()
			{
				Id = 22,
				Code = "SegaCD",
				DisplayName = "Sega CD"
			},
			new ()
			{
				Id = 23,
				Code = "FDS",
				DisplayName = "Famicom Disk System"
			},
			new ()
			{
				Id = 24,
				Code = "PCECD",
				DisplayName = "TurboGrafx 16 CD"
			},
			new ()
			{
				Id = 25,
				Code = "Vboy",
				DisplayName = "Virtual Boy"
			},
			new ()
			{
				Id = 26,
				Code = "MSX",
				DisplayName = "MSX Home Computer System"
			},
			new ()
			{
				Id = 27,
				Code = "GC",
				DisplayName = "Nintendo GameCube"
			},
			new ()
			{
				Id = 28,
				Code = "Wii",
				DisplayName = "Nintendo Wii"
			},
			new ()
			{
				Id = 29,
				Code = "Windows",
				DisplayName = "Windows"
			},
			new ()
			{
				Id = 30,
				Code = "SG1000",
				DisplayName = "Sega SG-1000"
			},
			new ()
			{
				Id = 31,
				Code = "TI83",
				DisplayName = "Texas Instruments TI-83 Series"
			},
			new ()
			{
				Id = 32,
				Code = "SGX",
				DisplayName = "SuperGrafx"
			},
			new ()
			{
				Id = 33,
				Code = "DOOM",
				DisplayName = "DooM"
			},
			new ()
			{
				Id = 34,
				Code = "A2600",
				DisplayName = "Atari 2600"
			},
			new ()
			{
				Id = 35,
				Code = "Coleco",
				DisplayName = "ColecoVision"
			},
			new ()
			{
				Id = 36,
				Code = "A7800",
				DisplayName = "Atari 7800"
			},
			new ()
			{
				Id = 37,
				Code = "C64",
				DisplayName = "Commodore 64"
			},
			new ()
			{
				Id = 38,
				Code = "Linux",
				DisplayName = "Linux"
			},
			new ()
			{
				Id = 39,
				Code = "SVI3x8",
				DisplayName = "Spectravideo SVI-318/328"
			},
			new ()
			{
				Id = 41,
				Code = "AppleII",
				DisplayName = "Apple II"
			},
			new ()
			{
				Id = 42,
				Code = "INTV",
				DisplayName = "Intellivision"
			},
			new ()
			{
				Id = 43,
				Code = "Uzebox",
				DisplayName = "Uzebox"
			},
			new ()
			{
				Id = 44,
				Code = "ZXS",
				DisplayName = "ZX Spectrum"
			},
			new ()
			{
				Id = 45,
				Code = "VEC",
				DisplayName = "General Computer Vectrex"
			},
			new ()
			{
				Id = 46,
				Code = "O2",
				DisplayName = "Odyssey 2"
			}
		};

		public static readonly GameSystemFrameRate[] SystemFrameRates =
		{
			new ()
			{
				GameSystemId = 1,
				RegionCode = Ntsc,
				FrameRate = 60.0988138974405
			},
			new ()
			{
				GameSystemId = 1,
				RegionCode = Pal,
				FrameRate = 50.0069789081886
			},
			new ()
			{
				GameSystemId = 1,
				RegionCode = Ntsc + "60",
				FrameRate = 60,
				Obsolete = true
			},
			new ()
			{
				GameSystemId = 1,
				RegionCode = Pal + "50",
				FrameRate = 50,
				Obsolete = true
			},
			new ()
			{
				GameSystemId = 2,
				RegionCode = Ntsc,
				FrameRate = 60.0988138974405
			},
			new ()
			{
				GameSystemId = 2,
				RegionCode = Pal,
				FrameRate = 50.0069789081886
			},
			new ()
			{
				GameSystemId = 2,
				RegionCode = Ntsc + "60",
				FrameRate = 60,
				Obsolete = true
			},
			new ()
			{
				GameSystemId = 2,
				RegionCode = Pal + "50",
				FrameRate = 50,
				Obsolete = true
			},
			new ()
			{
				GameSystemId = 3,
				RegionCode = Ntsc,
				FrameRate = 59.922751013550524
			},
			new ()
			{
				GameSystemId = 3,
				RegionCode = Pal,
				FrameRate = 49.70146011994842
			},
			new ()
			{
				GameSystemId = 3,
				RegionCode = Ntsc + "60",
				FrameRate = 60,
				Obsolete = true
			},
			new ()
			{
				GameSystemId = 3,
				RegionCode = Pal + "50",
				FrameRate = 50,
				Obsolete = true
			},
			new ()
			{
				GameSystemId = 4,
				RegionCode = Ntsc,
				FrameRate = 59.7275005696058
			},
			new ()
			{
				GameSystemId = 4,
				RegionCode = Ntsc + "60",
				FrameRate = 60,
				Obsolete = true
			},
			new ()
			{
				GameSystemId = 5,
				RegionCode = Ntsc,
				FrameRate = 60.0988138974405
			},
			new ()
			{
				GameSystemId = 5,
				RegionCode = Pal,
				FrameRate = 50.0069789081886
			},
			new ()
			{
				GameSystemId = 5,
				RegionCode = Ntsc + "60",
				FrameRate = 60,
				Obsolete = true
			},
			new ()
			{
				GameSystemId = 6,
				RegionCode = Ntsc,
				FrameRate = 59.7275005696058
			},
			new ()
			{
				GameSystemId = 6,
				RegionCode = Ntsc + "60",
				FrameRate = 60,
				Obsolete = true
			},
			new ()
			{
				GameSystemId = 7,
				RegionCode = Ntsc,
				FrameRate = 59.7275005696058
			},
			new ()
			{
				GameSystemId = 7,
				RegionCode = Ntsc + "60",
				FrameRate = 60,
				Obsolete = true
			},
			new ()
			{
				GameSystemId = 8,
				RegionCode = Ntsc,
				FrameRate = 60,
				Preliminary = true
			},
			new ()
			{
				GameSystemId = 8,
				RegionCode = Pal,
				FrameRate = 50,
				Preliminary = true
			},
			new ()
			{
				GameSystemId = 9,
				RegionCode = Ntsc,
				FrameRate = 70.0863028953229
			},
			new ()
			{
				GameSystemId = 9,
				RegionCode = Ntsc + "60",
				FrameRate = 60,
				Obsolete = true
			},
			new ()
			{
				GameSystemId = 10,
				RegionCode = Ntsc,
				FrameRate = 59.9227510135505
			},
			new ()
			{
				GameSystemId = 10,
				RegionCode = Pal,
				FrameRate = 49.70146011994839
			},
			new ()
			{
				GameSystemId = 10,
				RegionCode = Ntsc + "60",
				FrameRate = 60,
				Obsolete = true
			},
			new ()
			{
				GameSystemId = 10,
				RegionCode = Pal + "50",
				FrameRate = 50,
				Obsolete = true
			},
			new ()
			{
				GameSystemId = 11,
				RegionCode = Ntsc,
				FrameRate = 59.29286256195557
			},
			new ()
			{
				GameSystemId = 11,
				RegionCode = Pal,
				FrameRate = 49.764559357596745
			},
			new ()
			{
				GameSystemId = 11,
				RegionCode = Ntsc + "60",
				FrameRate = 60,
				Obsolete = true
			},
			new ()
			{
				GameSystemId = 11,
				RegionCode = Pal + "50",
				FrameRate = 50,
				Obsolete = true
			},
			new ()
			{
				GameSystemId = 12,
				RegionCode = Ntsc,
				FrameRate = 59.8261054534819
			},
			new ()
			{
				GameSystemId = 13,
				RegionCode = Ntsc,
				FrameRate = 75.4716981132075
			},
			new ()
			{
				GameSystemId = 14,
				RegionCode = Ntsc,
				FrameRate = 59.8261054534819
			},
			new ()
			{
				GameSystemId = 15,
				RegionCode = Ntsc,
				FrameRate = 60.2530155928214
			},
			new ()
			{
				GameSystemId = 16,
				RegionCode = Ntsc,
				FrameRate = 59.89817311
			},
			new ()
			{
				GameSystemId = 16,
				RegionCode = Ntsc + "60",
				FrameRate = 60,
				Obsolete = true
			},
			new ()
			{
				GameSystemId = 17,
				RegionCode = Ntsc,
				FrameRate = 59.82609828808082
			},
			new ()
			{
				GameSystemId = 17,
				RegionCode = Ntsc + "60",
				FrameRate = 60,
				Obsolete = true
			},
			new ()
			{
				GameSystemId = 18,
				RegionCode = Ntsc,
				FrameRate = 59.9227510135505
			},
			new ()
			{
				GameSystemId = 18,
				RegionCode = Ntsc + "60",
				FrameRate = 60,
				Obsolete = true
			},
			new ()
			{
				GameSystemId = 19,
				RegionCode = Ntsc,
				FrameRate = 60,
				Preliminary = true
			},
			new ()
			{
				GameSystemId = 19,
				RegionCode = Pal,
				FrameRate = 50,
				Preliminary = true
			},
			new ()
			{
				GameSystemId = 20,
				RegionCode = Ntsc,
				FrameRate = 60,
				Preliminary = true
			},
			new ()
			{
				GameSystemId = 20,
				RegionCode = Pal,
				FrameRate = 50,
				Preliminary = true
			},
			new ()
			{
				GameSystemId = 21,
				RegionCode = Ntsc,
				FrameRate = 59.922751013550524
			},
			new ()
			{
				GameSystemId = 21,
				RegionCode = Pal,
				FrameRate = 49.70146011994842
			},
			new ()
			{
				GameSystemId = 21,
				RegionCode = Ntsc + "60",
				FrameRate = 60,
				Obsolete = true
			},
			new ()
			{
				GameSystemId = 22,
				RegionCode = Ntsc,
				FrameRate = 59.922751013550524
			},
			new ()
			{
				GameSystemId = 22,
				RegionCode = Pal,
				FrameRate = 49.70146011994842
			},
			new ()
			{
				GameSystemId = 22,
				RegionCode = Ntsc + "60",
				FrameRate = 60,
				Obsolete = true
			},
			new ()
			{
				GameSystemId = 23,
				RegionCode = Ntsc,
				FrameRate = 60.0988138974405
			},
			new ()
			{
				GameSystemId = 23,
				RegionCode = Ntsc + "60",
				FrameRate = 60,
				Obsolete = true
			},
			new ()
			{
				GameSystemId = 24,
				RegionCode = Ntsc,
				FrameRate = 59.8261054534819
			},
			new ()
			{
				GameSystemId = 25,
				RegionCode = Ntsc,
				FrameRate = 50.2734877734878
			},
			new ()
			{
				GameSystemId = 26,
				RegionCode = Ntsc,
				FrameRate = 59.9227510135505
			},
			new ()
			{
				GameSystemId = 26,
				RegionCode = Pal,
				FrameRate = 50.158975804566104
			},
			new ()
			{
				GameSystemId = 27,
				RegionCode = Ntsc,
				FrameRate = 60,
				Preliminary = true
			},
			new ()
			{
				GameSystemId = 27,
				RegionCode = Pal,
				FrameRate = 50,
				Preliminary = true
			},
			new ()
			{
				GameSystemId = 28,
				RegionCode = Ntsc,
				FrameRate = 60,
				Preliminary = true
			},
			new ()
			{
				GameSystemId = 28,
				RegionCode = Pal,
				FrameRate = 50,
				Preliminary = true
			},
			new ()
			{
				GameSystemId = 29,
				RegionCode = Ntsc,
				FrameRate = 60,
				Preliminary = true
			},
			new ()
			{
				GameSystemId = 30,
				RegionCode = Ntsc,
				FrameRate = 59.9227510135505
			},
			new ()
			{
				GameSystemId = 30,
				RegionCode = Pal,
				FrameRate = 49.70146011994839
			},
			new ()
			{
				GameSystemId = 31,
				RegionCode = Ntsc,
				FrameRate = 60,
				Preliminary = true
			},
			new ()
			{
				GameSystemId = 32,
				RegionCode = Ntsc,
				FrameRate = 59.8261054534819
			},
			new ()
			{
				GameSystemId = 33,
				RegionCode = Ntsc,
				FrameRate = 35.0029869215506
			},
			new ()
			{
				GameSystemId = 34,
				RegionCode = Ntsc,
				FrameRate = 59.9227510135505
			},
			new ()
			{
				GameSystemId = 34,
				RegionCode = Pal,
				FrameRate = 49.8607596716149
			},
			new ()
			{
				GameSystemId = 34,
				RegionCode = Ntsc + "60",
				FrameRate = 60,
				Obsolete = true
			},
			new ()
			{
				GameSystemId = 35,
				RegionCode = Ntsc,
				FrameRate = 59.9227510135505
			},
			new ()
			{
				GameSystemId = 35,
				RegionCode = Pal,
				FrameRate = 49.70146011994839
			},
			new ()
			{
				GameSystemId = 35,
				RegionCode = Ntsc + "60",
				FrameRate = 60,
				Obsolete = true
			},
			new ()
			{
				GameSystemId = 36,
				RegionCode = Ntsc,
				FrameRate = 59.9227510135505
			},
			new ()
			{
				GameSystemId = 36,
				RegionCode = Pal,
				FrameRate = 49.70146011994839
			},
			new ()
			{
				GameSystemId = 37,
				RegionCode = Ntsc,
				FrameRate = 59.826089499853765
			},
			new ()
			{
				GameSystemId = 37,
				RegionCode = Pal,
				FrameRate = 50.1245421245421
			},
			new ()
			{
				GameSystemId = 38,
				RegionCode = Ntsc,
				FrameRate = 60
			},
			new ()
			{
				GameSystemId = 39,
				RegionCode = Ntsc,
				FrameRate = 59.9227510135505
			},
			new ()
			{
				GameSystemId = 39,
				RegionCode = Pal,
				FrameRate = 50.158975804566104
			},
			new ()
			{
				GameSystemId = 41,
				RegionCode = Ntsc,
				FrameRate = 60,
				Preliminary = true
			},
			new ()
			{
				GameSystemId = 41,
				RegionCode = Pal,
				FrameRate = 50,
				Preliminary = true
			},
			new ()
			{
				GameSystemId = 42,
				RegionCode = Ntsc,
				FrameRate = 59.92
			},
			new ()
			{
				GameSystemId = 43,
				RegionCode = Ntsc,
				FrameRate = 60.01631993960238
			},
			new ()
			{
				GameSystemId = 44,
				RegionCode = Pal,
				FrameRate = 50.080128205
			},
			new ()
			{
				GameSystemId = 45,
				RegionCode = Ntsc,
				FrameRate = 50
			},
			new ()
			{
				GameSystemId = 46,
				RegionCode = Ntsc,
				FrameRate = 60.056453065881932
			},
			new ()
			{
				GameSystemId = 46,
				RegionCode = Pal,
				FrameRate = 49.970017989206475
			}
		};
	}
}
