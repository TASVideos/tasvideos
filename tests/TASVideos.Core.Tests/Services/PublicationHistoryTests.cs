using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Game;

namespace TASVideos.Core.Tests.Services;

[TestClass]
public class PublicationHistoryTests : TestDbBase
{
	private readonly PublicationHistory _publicationHistory;

	#region Test Data
	private readonly PublicationClass _publicationClass = new() { Id = 1 };
	private readonly GameSystem _gameSystem = new() { Id = 1 };
	private Game Smb => new() { Id = 1, DisplayName = "Smb" };
	private GameVersion SmbGameVersion => new() { GameId = Smb.Id, System = _gameSystem };
	private Game Smb2j => new() { Id = 2, DisplayName = "Smb2j" };
	private GameVersion Smb2jGameVersion => new() { GameId = Smb2j.Id, System = _gameSystem };
	private GameSystemFrameRate GameSystemFrameRate => new() { GameSystemId = _gameSystem.Id };
	private int _nextUserId = 1;
	private Submission Submission
	{
		get
		{
			var submission = new Submission
			{
				Submitter = new User
				{
					Id = _nextUserId,
					UserName = "TestUser" + _nextUserId,
					NormalizedUserName = ("TestUser" + _nextUserId).ToUpper(),
					Email = "TestUser" + _nextUserId + "@example.com",
					NormalizedEmail = ("TestUser" + _nextUserId + "@example.com").ToUpper()
				}
			};
			_nextUserId++;
			return submission;
		}
	}

	private Publication SmbWarps => new()
	{
		Id = 1,
		GameId = Smb.Id,
		Title = "Smb in less than 5 minutes",
		GameGoal = new GameGoal { DisplayName = "Warps", GameId = Smb.Id },
		PublicationClass = _publicationClass,
		GameVersion = SmbGameVersion,
		SystemFrameRate = GameSystemFrameRate,
		Submission = Submission,
		MovieFileName = Smb.DisplayName + "1",
		SystemId = _gameSystem.Id
	};

	private Publication SmbWarpsObsolete => new()
	{
		Id = 2,
		GameId = Smb.Id,
		Title = "Smb in 5 minutes",
		GameGoal = new GameGoal { DisplayName = "Warps", GameId = Smb.Id },
		ObsoletedById = SmbWarps.Id,
		PublicationClass = _publicationClass,
		GameVersion = SmbGameVersion,
		SystemFrameRate = GameSystemFrameRate,
		Submission = Submission,
		MovieFileName = Smb.DisplayName + "2",
		SystemId = _gameSystem.Id
	};

	private Publication SmbWarpsObsoleteObsolete => new()
	{
		Id = 3,
		GameId = Smb.Id,
		Title = "Smb in 5.5 minutes",
		GameGoal = new GameGoal { DisplayName = "Warps", GameId = Smb.Id },
		ObsoletedById = SmbWarpsObsolete.Id,
		PublicationClass = _publicationClass,
		GameVersion = SmbGameVersion,
		SystemFrameRate = GameSystemFrameRate,
		Submission = Submission,
		MovieFileName = Smb.DisplayName + "3",
		SystemId = _gameSystem.Id
	};

	private Publication SmbWarpsObsoleteGoal => new()
	{
		Id = 4,
		GameId = Smb.Id,
		Title = "Smb in 6 minutes without using glitches",
		GameGoal = new GameGoal { DisplayName = "Warps", GameId = Smb.Id },
		ObsoletedById = SmbWarps.Id,
		PublicationClass = _publicationClass,
		GameVersion = SmbGameVersion,
		SystemFrameRate = GameSystemFrameRate,
		Submission = Submission,
		MovieFileName = Smb.DisplayName + "4",
		SystemId = _gameSystem.Id
	};

	private Publication SmbWarpless => new()
	{
		Id = 10,
		GameId = Smb.Id,
		Title = "Smb in about 20 minutes",
		GameGoal = new GameGoal { DisplayName = "No Warps", GameId = Smb.Id },
		PublicationClass = _publicationClass,
		GameVersion = SmbGameVersion,
		SystemFrameRate = GameSystemFrameRate,
		Submission = Submission,
		MovieFileName = Smb.DisplayName + "10",
		SystemId = _gameSystem.Id
	};

	private Publication Smb2jWarps => new()
	{
		Id = 20,
		GameId = Smb2j.Id,
		Title = "Smb2j in about 8 minutes",
		GameGoal = new GameGoal { DisplayName = "Warps", GameId = Smb2j.Id },
		PublicationClass = _publicationClass,
		GameVersion = Smb2jGameVersion,
		SystemFrameRate = GameSystemFrameRate,
		Submission = Submission,
		MovieFileName = Smb2j.DisplayName + "20",
		SystemId = _gameSystem.Id
	};

	#endregion

	public PublicationHistoryTests()
	{
		_publicationHistory = new PublicationHistory(_db);
	}

	[TestMethod]
	public async Task ForGame_NoGame_ReturnsNull()
	{
		var actual = await _publicationHistory.ForGame(int.MaxValue);
		Assert.IsNull(actual);
	}

	[TestMethod]
	public async Task ForGame_GameIdMatches()
	{
		_db.Add(Smb);
		await _db.SaveChangesAsync();

		var actual = await _publicationHistory.ForGame(Smb.Id);
		Assert.IsNotNull(actual);
		Assert.AreEqual(Smb.Id, actual.GameId);
	}

	[TestMethod]
	public async Task ForGame_NoPublications_GoalsEmpty()
	{
		_db.Add(Smb);
		await _db.SaveChangesAsync();

		var actual = await _publicationHistory.ForGame(Smb.Id);
		Assert.IsNotNull(actual);
		Assert.AreEqual(0, actual.Goals.Count());
	}

	[TestMethod]
	public async Task ForGame_FiltersByGame()
	{
		_db.Add(_publicationClass);
		_db.Add(Smb);
		_db.Add(SmbWarps);

		_db.Add(Smb2j);
		_db.Add(Smb2jWarps);

		await _db.SaveChangesAsync();

		var actual = await _publicationHistory.ForGame(Smb.Id);
		Assert.IsNotNull(actual);
		Assert.AreEqual(Smb.Id, actual.GameId);

		var goalList = actual.Goals.ToList();
		Assert.AreEqual(1, goalList.Count);
		Assert.AreEqual(SmbWarps.Id, goalList.Single().Id);
	}

	[TestMethod]
	public async Task ForGame_SinglePublication_ResultMatches()
	{
		_db.Add(Smb);
		_db.Add(SmbWarps);
		await _db.SaveChangesAsync();

		var actual = await _publicationHistory.ForGame(Smb.Id);
		Assert.IsNotNull(actual);

		var goalList = actual.Goals.ToList();
		Assert.AreEqual(1, goalList.Count);

		var movie = goalList.Single();
		Assert.AreEqual(SmbWarps.Id, movie.Id);
		Assert.AreEqual(SmbWarps.Title, movie.Title);
		Assert.AreEqual(SmbWarps.GameGoal!.DisplayName, movie.Goal);
	}

	[TestMethod]
	public async Task ForGame_SinglePublication_NoObsolete_EmptyList()
	{
		_db.Add(Smb);
		_db.Add(SmbWarps);
		await _db.SaveChangesAsync();

		var actual = await _publicationHistory.ForGame(Smb.Id);
		Assert.IsNotNull(actual);

		var goalList = actual.Goals.ToList();
		Assert.AreEqual(1, goalList.Count);

		var movie = goalList.Single();
		Assert.AreEqual(0, movie.Obsoletes.Count());
	}

	[TestMethod]
	public async Task ForGame_MultiGoal_ResultMatches()
	{
		_db.Add(Smb);
		_db.Add(SmbWarps);
		_db.Add(SmbWarpless);
		await _db.SaveChangesAsync();

		var actual = await _publicationHistory.ForGame(Smb.Id);
		Assert.IsNotNull(actual);

		var goalList = actual.Goals.ToList();
		Assert.AreEqual(2, goalList.Count);

		Assert.AreEqual(1, goalList.Count(b => b.Goal == SmbWarps.GameGoal!.DisplayName));
		Assert.AreEqual(1, goalList.Count(b => b.Goal == SmbWarpless.GameGoal!.DisplayName));
	}

	[TestMethod]
	public async Task ForGame_ObsoleteGoal_NotParentNode()
	{
		_db.Add(Smb);
		_db.Add(SmbWarps);
		_db.Add(SmbWarpsObsoleteGoal);
		await _db.SaveChangesAsync();

		var actual = await _publicationHistory.ForGame(Smb.Id);
		Assert.IsNotNull(actual);

		var goalList = actual.Goals.ToList();
		Assert.AreEqual(1, goalList.Count);
		Assert.AreEqual(SmbWarps.GameGoal!.DisplayName, goalList.Single().Goal);
	}

	[TestMethod]
	public async Task ForGame_ReturnsObsolete()
	{
		_db.Add(Smb);
		_db.Add(SmbWarps);
		_db.Add(SmbWarpsObsolete);
		await _db.SaveChangesAsync();

		var actual = await _publicationHistory.ForGame(Smb.Id);
		Assert.IsNotNull(actual);

		var goalList = actual.Goals.ToList();
		Assert.AreEqual(1, goalList.Count);

		var currentPub = goalList.Single();
		Assert.AreEqual(SmbWarps.Id, currentPub.Id);

		var obsolete = currentPub.Obsoletes.SingleOrDefault();
		Assert.IsNotNull(obsolete);
		Assert.AreEqual(SmbWarpsObsolete.Id, obsolete.Id);
	}

	[TestMethod]
	public async Task ForGame_OnePubWithMultipleObsoletions()
	{
		_db.Add(Smb);
		_db.Add(SmbWarps);
		_db.Add(SmbWarpsObsolete);
		_db.Add(SmbWarpsObsoleteGoal);
		await _db.SaveChangesAsync();

		var actual = await _publicationHistory.ForGame(Smb.Id);
		Assert.IsNotNull(actual);

		var goalList = actual.Goals.ToList();
		Assert.AreEqual(1, goalList.Count);

		var currentPub = goalList.Single();
		Assert.AreEqual(SmbWarps.Id, currentPub.Id);

		var obsoletes = currentPub.Obsoletes.ToList();
		Assert.AreEqual(2, obsoletes.Count);
		Assert.AreEqual(1, obsoletes.Count(o => o.Id == SmbWarpsObsolete.Id));
		Assert.AreEqual(1, obsoletes.Count(o => o.Id == SmbWarpsObsoleteGoal.Id));
	}

	[TestMethod]
	public async Task ForGame_ObsoletionChain()
	{
		_db.Add(Smb);
		_db.Add(SmbWarps);
		_db.Add(SmbWarpsObsolete);
		_db.Add(SmbWarpsObsoleteObsolete);
		await _db.SaveChangesAsync();

		var actual = await _publicationHistory.ForGame(Smb.Id);
		Assert.IsNotNull(actual);

		var goalList = actual.Goals.ToList();
		Assert.AreEqual(1, goalList.Count);

		var currentPub = goalList.Single();
		Assert.AreEqual(SmbWarps.Id, currentPub.Id);

		var obsoletes = currentPub.Obsoletes.ToList();
		Assert.AreEqual(1, obsoletes.Count);

		var nestedObsoleteList = obsoletes.Single().Obsoletes.ToList();

		Assert.IsNotNull(nestedObsoleteList);
		Assert.AreEqual(1, nestedObsoleteList.Count);
		var nestObsoletePub = nestedObsoleteList.Single();
		Assert.AreEqual(SmbWarpsObsoleteObsolete.Id, nestObsoletePub.Id);
	}

	[TestMethod]
	public async Task ForGameByPublication_PublicationDoesNotExist_ReturnsNull()
	{
		var actual = await _publicationHistory.ForGameByPublication(int.MaxValue);
		Assert.IsNull(actual);
	}

	[TestMethod]
	public async Task ForGameByPublication_PublicationExists_ReturnsByGame()
	{
		_db.Games.Add(Smb);
		_db.Publications.Add(SmbWarps);
		_db.Publications.Add(SmbWarpless);
		await _db.SaveChangesAsync();

		var actual = await _publicationHistory.ForGameByPublication(SmbWarps.Id);
		Assert.IsNotNull(actual);
		Assert.AreEqual(2, actual.Goals.Count());
	}
}
