using TASVideos.Data.Entity.Game;

namespace TASVideos.Data.SeedData
{
    public static class GameSeedData
	{
		public static Game[] Games =
		{
			new Game
			{
				GoodName = "Super Mario Bros.",
				DisplayName = "Super Mario Bros.",
				Abbreviation = "SMB",
				SystemId = 1,
				SearchKey = "nes-super-mario-bros",
				YoutubeTags = "Super Mario Bros,Mario,SMB"
			},
			new Game
			{
				GoodName = "Bad Dudes",
				DisplayName = "Bad Dudes",
				Abbreviation = "Bad Dudes",
				SystemId = 1,
				SearchKey = "nes-bad-dudes",
				YoutubeTags = "Bad Dudes"
			},
			new Game
			{
				GoodName = "Sonic Advance 2",
				DisplayName = "Sonic Advance 2",
				Abbreviation = "SAdva2",
				SystemId = 7,
				SearchKey = "gba-sonic-advance-2",
				YoutubeTags = "Sonic,Advance 2,SAdva2,SA2"
			},
			new Game
			{
				GoodName = "E.T. - The Extra-Terrestrial",
				DisplayName = "E.T.: The Extra-Terrestrial",
				Abbreviation = "ET",
				SystemId = 34,
				SearchKey = "a2600-et-the-extraterrestrial",
				YoutubeTags = "E.T,,ET,Extra-Terrestrial,Extra Terrestrial,infamous,video game crash"
			},
			new Game
			{
				GoodName = "Ninja Golf",
				DisplayName = "Ninja Golf",
				Abbreviation = "njagolf",
				SystemId = 36,
				SearchKey = "a7800-ninja-golf",
				YoutubeTags = "Ninja Golf"
			},
		};

		public static GameRom[] Roms =
		{
			new GameRom
			{
				Md5 = "12345", // TODO
				Sha1 = "12345", // TODO
				Name = "Super Mario Bros. (W) [!].nes",
				Region = "Any",
				Version = "",
				Type = RomTypes.Good,
				SystemId = 1
			},
			new GameRom
			{
				Md5 = "12345", // TODO
				Sha1 = "12345", // TODO
				Name = "Bad Dudes (U) [!].nes",
				Region = "USA",
				Version = "",
				Type = RomTypes.Good,
				SystemId = 1
			},
			new GameRom
			{
				Md5 = "12345", // TODO
				Sha1 = "12345", // TODO
				Name = "E.T. The Extra-terrestrial (U) [!].a26",
				Region = "USA",
				Version = "",
				Type = RomTypes.Good,
				SystemId = 34
			}
		};
	}
}
