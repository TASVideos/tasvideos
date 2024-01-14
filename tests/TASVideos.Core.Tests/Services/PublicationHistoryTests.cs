using TASVideos.Core.Services.PublicationChain;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Game;

namespace TASVideos.Core.Tests.Services;

[TestClass]
public class PublicationHistoryTests
{
	private readonly IPublicationHistory _publicationHistory;
	private readonly TestDbContext _db;

	#region Test Data
	private static readonly PublicationClass PublicationClass = new() { Id = 1 };
	private static Game Smb => new() { Id = 1 };
	private static Game Smb2j => new() { Id = 2 };

	private static Publication SmbWarps => new()
	{
		Id = 1,
		GameId = Smb.Id,
		Title = "Smb in less than 5 minutes",
		GameGoal = new GameGoal { DisplayName = "Warps" },
		PublicationClass = PublicationClass
	};

	private static Publication SmbWarpsObsolete => new()
	{
		Id = 2,
		GameId = Smb.Id,
		Title = "Smb in 5 minutes",
		GameGoal = new GameGoal { DisplayName = "Warps" },
		ObsoletedById = SmbWarps.Id,
		PublicationClass = PublicationClass
	};

	private static Publication SmbWarpsObsoleteObsolete => new()
	{
		Id = 3,
		GameId = Smb.Id,
		Title = "Smb in 5.5 minutes",
		GameGoal = new GameGoal { DisplayName = "Warps" },
		ObsoletedById = SmbWarpsObsolete.Id,
		PublicationClass = PublicationClass
	};

	private static Publication SmbWarpsObsoleteGoal => new()
	{
		Id = 4,
		GameId = Smb.Id,
		Title = "Smb in 6 minutes without using glitches",
		GameGoal = new GameGoal { DisplayName = "Warps" },
		ObsoletedById = SmbWarps.Id,
		PublicationClass = PublicationClass
	};

	private static Publication SmbWarpless => new()
	{
		Id = 10,
		GameId = Smb.Id,
		Title = "Smb in about 20 minutes",
		GameGoal = new GameGoal { DisplayName = "No Warps" },
		PublicationClass = PublicationClass
	};

	private static Publication Smb2jWarps => new()
	{
		Id = 20,
		GameId = Smb2j.Id,
		Title = "Smb2j in about 8 minutes",
		GameGoal = new GameGoal { DisplayName = "Warps" },
		PublicationClass = PublicationClass
	};

	#endregion

	public PublicationHistoryTests()
	{
		_db = TestDbContext.Create();
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
		Assert.IsNotNull(actual.Goals);
		Assert.AreEqual(0, actual.Goals.Count());
	}

	[TestMethod]
	public async Task ForGame_FiltersByGame()
	{
		_db.Add(PublicationClass);
		_db.Add(Smb);
		_db.Add(SmbWarps);

		_db.Add(Smb2j);
		_db.Add(Smb2jWarps);

		await _db.SaveChangesAsync();

		var actual = await _publicationHistory.ForGame(Smb.Id);
		Assert.IsNotNull(actual);
		Assert.AreEqual(Smb.Id, actual.GameId);
		Assert.IsNotNull(actual.Goals);

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
		Assert.IsNotNull(actual.Goals);

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
		Assert.IsNotNull(actual.Goals);

		var goalList = actual.Goals.ToList();
		Assert.AreEqual(1, goalList.Count);

		var movie = goalList.Single();
		Assert.IsNotNull(movie.Obsoletes);
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
		Assert.IsNotNull(actual.Goals);

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
		Assert.IsNotNull(actual.Goals);

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
		Assert.IsNotNull(actual.Goals);

		var goalList = actual.Goals.ToList();
		Assert.AreEqual(1, goalList.Count);

		var currentPub = goalList.Single();
		Assert.AreEqual(SmbWarps.Id, currentPub.Id);

		Assert.IsNotNull(currentPub.Obsoletes);
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
		Assert.IsNotNull(actual.Goals);

		var goalList = actual.Goals.ToList();
		Assert.AreEqual(1, goalList.Count);

		var currentPub = goalList.Single();
		Assert.AreEqual(SmbWarps.Id, currentPub.Id);

		Assert.IsNotNull(currentPub.Obsoletes);
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
		Assert.IsNotNull(actual.Goals);

		var goalList = actual.Goals.ToList();
		Assert.AreEqual(1, goalList.Count);

		var currentPub = goalList.Single();
		Assert.AreEqual(SmbWarps.Id, currentPub.Id);

		Assert.IsNotNull(currentPub.Obsoletes);
		var obsoletes = currentPub.Obsoletes.ToList();
		Assert.AreEqual(1, obsoletes.Count);

		var nestedObsoleteList = obsoletes.Single().Obsoletes.ToList();

		Assert.IsNotNull(nestedObsoleteList);
		Assert.AreEqual(1, nestedObsoleteList.Count);
		var nestObsoletePub = nestedObsoleteList.Single();
		Assert.AreEqual(SmbWarpsObsoleteObsolete.Id, nestObsoletePub.Id);
	}
}
