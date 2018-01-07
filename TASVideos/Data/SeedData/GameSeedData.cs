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
				SystemId = 34,
				SearchKey = "a7800-ninja-golf",
				YoutubeTags = "Ninja Golf"
			},
		};
	}
}
