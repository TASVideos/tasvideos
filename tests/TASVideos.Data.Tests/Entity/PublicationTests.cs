using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Game;

namespace TASVideos.Data.Tests.Entity;

[TestClass]
public class PublicationTests
{
	[TestMethod]
	public void GenerateTitle_NoAuthor()
	{
		var publication = new Publication
		{
			Id = 123,
			Frames = 3600,
			System = new GameSystem { Code = "NES" },
			SystemFrameRate = new GameSystemFrameRate { FrameRate = 60.0 },
			Game = new Game { DisplayName = "Super Mario Bros." },
			GameGoal = new GameGoal { DisplayName = "baseline" }
		};

		publication.Title = publication.GenerateTitle();

		Assert.AreEqual("NES Super Mario Bros. by  in 01:00.000", publication.Title);
	}

	[TestMethod]
	public void GenerateTitle_WithSingleAuthor_IncludesAuthorName()
	{
		var publication = new Publication
		{
			Id = 456,
			Frames = 1800,
			System = new GameSystem { Code = "SNES" },
			SystemFrameRate = new GameSystemFrameRate { FrameRate = 60.09881422924901 },
			Game = new Game { DisplayName = "Super Metroid" },
			GameGoal = new GameGoal { DisplayName = "baseline" },
			Authors =
			[
				new PublicationAuthor
				{
					Ordinal = 1,
					Author = new User { UserName = "AuthorOne" }
				}
			]
		};

		publication.Title = publication.GenerateTitle(false);

		Assert.AreEqual("SNES Super Metroid by AuthorOne in 00:29.951", publication.Title);
	}

	[TestMethod]
	public void GenerateTitle_WithMultipleAuthors_JoinsWithCommasAndAmpersand()
	{
		var publication = new Publication
		{
			Id = 789,
			Frames = 2400,
			System = new GameSystem { Code = "GB" },
			SystemFrameRate = new GameSystemFrameRate { FrameRate = 59.7275005696058 },
			Game = new Game { DisplayName = "Tetris" },
			GameGoal = new GameGoal { DisplayName = "baseline" },
			Authors =
			[
				new PublicationAuthor
				{
					Ordinal = 1,
					Author = new User { UserName = "AuthorOne" }
				},
				new PublicationAuthor
				{
					Ordinal = 2,
					Author = new User { UserName = "AuthorTwo" }
				},
				new PublicationAuthor
				{
					Ordinal = 3,
					Author = new User { UserName = "AuthorThree" }
				}
			]
		};

		publication.Title = publication.GenerateTitle();

		Assert.AreEqual("GB Tetris by AuthorOne, AuthorTwo & AuthorThree in 00:40.182", publication.Title);
	}

	[TestMethod]
	public void GenerateTitle_WithAdditionalAuthors_IncludesThemInTitle()
	{
		var publication = new Publication
		{
			Id = 101,
			Frames = 1200,
			System = new GameSystem { Code = "GEN" },
			SystemFrameRate = new GameSystemFrameRate { FrameRate = 59.9227510135733 },
			Game = new Game { DisplayName = "Sonic the Hedgehog" },
			GameGoal = new GameGoal { DisplayName = "baseline" },
			Authors =
			[
				new PublicationAuthor
				{
					Ordinal = 1,
					Author = new User { UserName = "RegisteredUser" }
				}
			],
			AdditionalAuthors = "UnregisteredUser1, UnregisteredUser2"
		};

		publication.Title = publication.GenerateTitle();

		Assert.AreEqual("GEN Sonic the Hedgehog by RegisteredUser, UnregisteredUser1 &  UnregisteredUser2 in 00:20.026", publication.Title);
	}

	[TestMethod]
	public void GenerateTitle_WithGameVersionTitleOverride_UsesVersionTitleOverride()
	{
		var publication = new Publication
		{
			Id = 202,
			Frames = 4800,
			System = new GameSystem { Code = "N64" },
			SystemFrameRate = new GameSystemFrameRate { FrameRate = 60.0 },
			Game = new Game { DisplayName = "The Legend of Zelda: Ocarina of Time" },
			GameVersion = new GameVersion
			{
				TitleOverride = "The Legend of Zelda: Ocarina of Time (USA v1.2)"
			},
			GameGoal = new GameGoal { DisplayName = "baseline" }
		};

		publication.Title = publication.GenerateTitle();

		Assert.AreEqual("N64 The Legend of Zelda: Ocarina of Time (USA v1.2) by  in 01:20.000", publication.Title);
	}

	[TestMethod]
	public void GenerateTitle_WithNonBaselineGoal_IncludesGoalInQuotes()
	{
		var publication = new Publication
		{
			Id = 303,
			Frames = 1800,
			System = new GameSystem { Code = "SNES" },
			SystemFrameRate = new GameSystemFrameRate { FrameRate = 60.09881422924901 },
			Game = new Game { DisplayName = "Super Mario World" },
			GameGoal = new GameGoal { DisplayName = "96 exits" }
		};

		publication.Title = publication.GenerateTitle();

		Assert.AreEqual("SNES Super Mario World \"96 exits\" by  in 00:29.951", publication.Title);
	}

	[TestMethod]
	public void GenerateTitle_WithBaselineGoal_OmitsGoal()
	{
		var publication = new Publication
		{
			Id = 404,
			Frames = 3000,
			System = new GameSystem { Code = "NES" },
			SystemFrameRate = new GameSystemFrameRate { FrameRate = 60.0 },
			Game = new Game { DisplayName = "Mega Man 2" },
			GameGoal = new GameGoal { DisplayName = "baseline" }
		};

		publication.Title = publication.GenerateTitle();

		Assert.AreEqual("NES Mega Man 2 by  in 00:50.000", publication.Title);
	}

	[TestMethod]
	public void GenerateTitle_AuthorsOrderedByOrdinal_MaintainsCorrectOrder()
	{
		var publication = new Publication
		{
			Id = 505,
			Frames = 1800,
			System = new GameSystem { Code = "SMS" },
			SystemFrameRate = new GameSystemFrameRate { FrameRate = 59.9227510135733 },
			Game = new Game { DisplayName = "Alex Kidd in Miracle World" },
			GameGoal = new GameGoal { DisplayName = "baseline" },
			Authors =
			[
				new PublicationAuthor
				{
					Ordinal = 3,
					Author = new User { UserName = "Third" }
				},
				new PublicationAuthor
				{
					Ordinal = 1,
					Author = new User { UserName = "First" }
				},
				new PublicationAuthor
				{
					Ordinal = 2,
					Author = new User { UserName = "Second" }
				}
			]
		};

		publication.Title = publication.GenerateTitle();

		Assert.AreEqual("SMS Alex Kidd in Miracle World by First, Second & Third in 00:30.039", publication.Title);
	}

	[TestMethod]
	public void GenerateTitle_LongTimespan_ShowsHours()
	{
		var publication = new Publication
		{
			Id = 606,
			Frames = 432000, // 2 hours at 60 fps
			System = new GameSystem { Code = "SNES" },
			SystemFrameRate = new GameSystemFrameRate { FrameRate = 60.0 },
			Game = new Game { DisplayName = "Super Metroid" },
			GameGoal = new GameGoal { DisplayName = "baseline" }
		};

		publication.Title = publication.GenerateTitle();

		Assert.AreEqual("SNES Super Metroid by  in 2:00:00.000", publication.Title);
	}

	[TestMethod]
	public void GenerateTitle_VeryLongTimespan_ShowsDays()
	{
		var publication = new Publication
		{
			Id = 707,
			Frames = 5184000, // 1 day at 60 fps
			System = new GameSystem { Code = "PC" },
			SystemFrameRate = new GameSystemFrameRate { FrameRate = 60.0 },
			Game = new Game { DisplayName = "Desert Bus" },
			GameGoal = new GameGoal { DisplayName = "baseline" }
		};

		publication.Title = publication.GenerateTitle();

		Assert.AreEqual("PC Desert Bus by  in 1:00:00:00.000", publication.Title);
	}

	[TestMethod]
	public void GenerateTitle_NullSystem_ThrowsInvalidOperationException()
	{
		var publication = new Publication
		{
			Id = 808,
			Frames = 1800,
			Game = new Game { DisplayName = "Test Game" },
			GameGoal = new GameGoal { DisplayName = "baseline" }
		};

		Assert.ThrowsExactly<InvalidOperationException>(() => publication.GenerateTitle());
	}

	[TestMethod]
	public void GenerateTitle_NullGame_ThrowsInvalidOperationException()
	{
		var publication = new Publication
		{
			Id = 909,
			Frames = 1800,
			System = new GameSystem { Code = "NES" },
			GameGoal = new GameGoal { DisplayName = "baseline" }
		};

		Assert.ThrowsExactly<InvalidOperationException>(() => publication.GenerateTitle());
	}

	[TestMethod]
	public void GenerateTitle_NullGameGoal_ThrowsNullReferenceException()
	{
		var publication = new Publication
		{
			Id = 1010,
			Frames = 1800,
			System = new GameSystem { Code = "NES" },
			Game = new Game { DisplayName = "Test Game" }
		};

		Assert.ThrowsExactly<NullReferenceException>(() => publication.GenerateTitle());
	}

	[TestMethod]
	public void GenerateTitle_EmptyGoalName_IncludesEmptyGoal()
	{
		var publication = new Publication
		{
			Id = 1111,
			Frames = 1800,
			System = new GameSystem { Code = "NES" },
			SystemFrameRate = new GameSystemFrameRate { FrameRate = 60.0 },
			Game = new Game { DisplayName = "Test Game" },
			GameGoal = new GameGoal { DisplayName = "" }
		};

		publication.Title = publication.GenerateTitle();

		Assert.AreEqual("NES Test Game by  in 00:30.000", publication.Title);
	}
}
