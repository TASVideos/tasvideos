using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Game;

namespace TASVideos.Data.Tests.Entity;

[TestClass]
public class SubmissionTests
{
	[TestMethod]
	public void GenerateTitle_NoAuthor()
	{
		var submission = new Submission
		{
			Id = 123,
			GameName = "Super Mario Bros.",
			Frames = 100,
			System = new GameSystem { Code = "NES" },
			SystemFrameRate = new GameSystemFrameRate { FrameRate = 60.0 }
		};

		submission.GenerateTitle();

		Assert.AreEqual("#123: 's NES Super Mario Bros. in 00:01.667", submission.Title);
	}

	[TestMethod]
	public void GenerateTitle_WithSingleAuthor()
	{
		var submission = new Submission
		{
			Id = 456,
			GameName = "The Legend of Zelda",
			Frames = 3600,
			System = new GameSystem { Code = "NES" },
			SystemFrameRate = new GameSystemFrameRate { FrameRate = 60.0 },
			SubmissionAuthors =
			[
				new SubmissionAuthor
				{
					Ordinal = 1,
					Author = new User { UserName = "AuthorOne" }
				}
			]
		};

		submission.GenerateTitle();

		Assert.AreEqual("#456: AuthorOne's NES The Legend of Zelda in 01:00.000", submission.Title);
	}

	[TestMethod]
	public void GenerateTitle_WithMultipleAuthors_JoinsWithCommasAndAmpersand()
	{
		var submission = new Submission
		{
			Id = 789,
			GameName = "Metroid",
			Frames = 1800,
			System = new GameSystem { Code = "NES" },
			SystemFrameRate = new GameSystemFrameRate { FrameRate = 60.0 },
			SubmissionAuthors =
			[
				new SubmissionAuthor
				{
					Ordinal = 1,
					Author = new User { UserName = "AuthorOne" }
				},
				new SubmissionAuthor
				{
					Ordinal = 2,
					Author = new User { UserName = "AuthorTwo" }
				},
				new SubmissionAuthor
				{
					Ordinal = 3,
					Author = new User { UserName = "AuthorThree" }
				}
			]
		};

		submission.GenerateTitle();

		Assert.AreEqual("#789: AuthorOne, AuthorTwo & AuthorThree's NES Metroid in 00:30.000", submission.Title);
	}

	[TestMethod]
	public void GenerateTitle_WithAdditionalAuthors_IncludesThemInTitle()
	{
		var submission = new Submission
		{
			Id = 101,
			GameName = "Castlevania",
			Frames = 2400,
			System = new GameSystem { Code = "NES" },
			SystemFrameRate = new GameSystemFrameRate { FrameRate = 60.0 },
			SubmissionAuthors =
			[
				new SubmissionAuthor
				{
					Ordinal = 1,
					Author = new User { UserName = "RegisteredUser" }
				}
			],
			AdditionalAuthors = "UnregisteredUser1, UnregisteredUser2"
		};

		submission.GenerateTitle();

		Assert.AreEqual("#101: RegisteredUser, UnregisteredUser1 &  UnregisteredUser2's NES Castlevania in 00:40.000", submission.Title);
	}

	[TestMethod]
	public void GenerateTitle_WithGameDisplayName_UsesGameDisplayNameInsteadOfGameName()
	{
		var submission = new Submission
		{
			Id = 202,
			GameName = "User Entered Game Name",
			Frames = 1200,
			System = new GameSystem { Code = "SNES" },
			SystemFrameRate = new GameSystemFrameRate { FrameRate = 60.09881422924901 },
			Game = new Game
			{
				Id = 1,
				DisplayName = "Official Game Display Name"
			}
		};

		submission.GenerateTitle();

		Assert.AreEqual("#202: 's SNES Official Game Display Name in 00:19.967", submission.Title);
	}

	[TestMethod]
	public void GenerateTitle_WithGameVersionTitleOverride_UsesVersionTitleOverride()
	{
		var submission = new Submission
		{
			Id = 303,
			GameName = "User Entered Game Name",
			Frames = 600,
			System = new GameSystem { Code = "GB" },
			SystemFrameRate = new GameSystemFrameRate { FrameRate = 59.7275005696058 },
			Game = new Game
			{
				Id = 1,
				DisplayName = "Official Game Display Name"
			},
			GameVersion = new GameVersion
			{
				TitleOverride = "Version Specific Title"
			}
		};

		submission.GenerateTitle();

		Assert.AreEqual("#303: 's GB Version Specific Title in 00:10.046", submission.Title);
	}

	[TestMethod]
	public void GenerateTitle_WithBranch_IncludesBranchInQuotes()
	{
		var submission = new Submission
		{
			Id = 404,
			GameName = "Super Metroid",
			Frames = 4800,
			System = new GameSystem { Code = "SNES" },
			SystemFrameRate = new GameSystemFrameRate { FrameRate = 60.09881422924901 },
			Branch = "low%"
		};

		submission.GenerateTitle();

		Assert.AreEqual("#404: 's SNES Super Metroid \"low%\" in 01:19.868", submission.Title);
	}

	[TestMethod]
	public void GenerateTitle_WithGameGoal_OverridesBranchAndUsesGameGoalInQuotes()
	{
		var submission = new Submission
		{
			Id = 505,
			GameName = "Sonic the Hedgehog",
			Frames = 2400,
			System = new GameSystem { Code = "GEN" },
			SystemFrameRate = new GameSystemFrameRate { FrameRate = 59.9227510135733 },
			Branch = "any%",
			GameGoal = new GameGoal
			{
				DisplayName = "game end glitch"
			}
		};

		submission.GenerateTitle();

		Assert.AreEqual("#505: 's GEN Sonic the Hedgehog \"game end glitch\" in 00:40.052", submission.Title);
	}

	[TestMethod]
	public void GenerateTitle_WithGameGoalBaseline_IgnoresGoalAndBranch()
	{
		var submission = new Submission
		{
			Id = 606,
			GameName = "Mega Man",
			Frames = 3000,
			System = new GameSystem { Code = "NES" },
			SystemFrameRate = new GameSystemFrameRate { FrameRate = 60.0 },
			Branch = "any%",
			GameGoal = new GameGoal
			{
				DisplayName = "baseline"
			}
		};

		submission.GenerateTitle();

		Assert.AreEqual("#606: 's NES Mega Man in 00:50.000", submission.Title);
	}

	[TestMethod]
	public void GenerateTitle_NoSystemCode_ShowsUnknown()
	{
		var submission = new Submission
		{
			Id = 707,
			GameName = "Test Game",
			Frames = 1800,
			SystemFrameRate = new GameSystemFrameRate { FrameRate = 60.0 }
		};

		submission.GenerateTitle();

		Assert.AreEqual("#707: 's Unknown Test Game in 00:30.000", submission.Title);
	}

	[TestMethod]
	public void GenerateTitle_AuthorsOrderedByOrdinal_MaintainsCorrectOrder()
	{
		var submission = new Submission
		{
			Id = 909,
			GameName = "Test Game",
			Frames = 1800,
			System = new GameSystem { Code = "NES" },
			SystemFrameRate = new GameSystemFrameRate { FrameRate = 60.0 },
			SubmissionAuthors =
			[
				new SubmissionAuthor
				{
					Ordinal = 3,
					Author = new User { UserName = "Third" }
				},
				new SubmissionAuthor
				{
					Ordinal = 1,
					Author = new User { UserName = "First" }
				},
				new SubmissionAuthor
				{
					Ordinal = 2,
					Author = new User { UserName = "Second" }
				}
			]
		};

		submission.GenerateTitle();

		Assert.AreEqual("#909: First, Second & Third's NES Test Game in 00:30.000", submission.Title);
	}

	[TestMethod]
	public void GenerateTitle_LongTimespan_ShowsHours()
	{
		var submission = new Submission
		{
			Id = 1010,
			GameName = "Long Game",
			Frames = 432000, // 2 hours at 60 fps
			System = new GameSystem { Code = "NES" },
			SystemFrameRate = new GameSystemFrameRate { FrameRate = 60.0 }
		};

		submission.GenerateTitle();

		Assert.AreEqual("#1010: 's NES Long Game in 2:00:00.000", submission.Title);
	}

	[TestMethod]
	public void GenerateTitle_VeryLongTimespan_ShowsDays()
	{
		var submission = new Submission
		{
			Id = 1111,
			GameName = "Very Long Game",
			Frames = 5184000, // 1 day at 60 fps
			System = new GameSystem { Code = "NES" },
			SystemFrameRate = new GameSystemFrameRate { FrameRate = 60.0 }
		};

		submission.GenerateTitle();

		Assert.AreEqual("#1111: 's NES Very Long Game in 1:00:00:00.000", submission.Title);
	}
}
