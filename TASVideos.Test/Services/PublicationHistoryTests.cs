using System.Linq;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Game;
using TASVideos.Services.PublicationChain;

namespace TASVideos.Test.Services
{
	[TestClass]
	public class PublicationHistoryTests
	{
		private IPublicationHistory _publicationHistory = null!;
		private TestDbContext _db = null!;

		#region Test Data

		private static Game Smb => new () { Id = 1 };
		private static Game Smb2j => new () { Id = 2 };

		private static Publication SmbWarps => new ()
		{
			Id = 1,
			GameId = Smb.Id,
			Title = "Smb in less than 5 minutes",
			Branch = "Warps"
		};

		private static Publication SmbWarpsObsolete => new ()
		{
			Id = 2,
			GameId = Smb.Id,
			Title = "Smb in 5 minutes",
			Branch = "Warps",
			ObsoletedById = SmbWarps.Id
		};

		private static Publication SmbWarpsObsoleteObsolete => new ()
		{
			Id = 3,
			GameId = Smb.Id,
			Title = "Smb in 5.5 minutes",
			Branch = "Warps",
			ObsoletedById = SmbWarpsObsolete.Id
		};

		private static Publication SmbWarpsObsoleteBranch => new ()
		{
			Id = 4,
			GameId = Smb.Id,
			Title = "Smb in 6 minutes without using glitches",
			Branch = "Warps",
			ObsoletedById = SmbWarps.Id
		};

		private static Publication SmbWarpless => new ()
		{
			Id = 10,
			GameId = Smb.Id,
			Title = "Smb in about 20 minutes",
			Branch = "No Warps"
		};

		private static Publication Smb2jWarps => new ()
		{
			Id = 20,
			GameId = Smb2j.Id,
			Title = "Smb2j in about 8 minutes",
			Branch = "Warps"
		};

		#endregion

		[TestInitialize]
		public void Initialize()
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
			_db.SaveChanges();

			var actual = await _publicationHistory.ForGame(Smb.Id);
			Assert.IsNotNull(actual);
			Assert.AreEqual(Smb.Id, actual!.GameId);
		}

		[TestMethod]
		public async Task ForGame_NoPublications_BranchesEmpty()
		{
			_db.Add(Smb);
			_db.SaveChanges();

			var actual = await _publicationHistory.ForGame(Smb.Id);
			Assert.IsNotNull(actual);
			Assert.IsNotNull(actual!.Branches);
			Assert.AreEqual(0, actual.Branches.Count());
		}

		[TestMethod]
		public async Task ForGame_FiltersByGame()
		{
			_db.Add(Smb);
			_db.Add(SmbWarps);

			_db.Add(Smb2j);
			_db.Add(Smb2jWarps);

			_db.SaveChanges();

			var actual = await _publicationHistory.ForGame(Smb.Id);
			Assert.IsNotNull(actual);
			Assert.AreEqual(Smb.Id, actual!.GameId);
			Assert.IsNotNull(actual.Branches);

			var branchList = actual.Branches.ToList();
			Assert.AreEqual(1, branchList.Count);
			Assert.AreEqual(SmbWarps.Id, branchList.Single().Id);
		}

		[TestMethod]
		public async Task ForGame_SinglePublication_ResultMatches()
		{
			_db.Add(Smb);
			_db.Add(SmbWarps);
			_db.SaveChanges();

			var actual = await _publicationHistory.ForGame(Smb.Id);
			Assert.IsNotNull(actual);
			Assert.IsNotNull(actual!.Branches);

			var branchList = actual.Branches.ToList();
			Assert.AreEqual(1, branchList.Count);

			var movie = branchList.Single();
			Assert.AreEqual(SmbWarps.Id, movie.Id);
			Assert.AreEqual(SmbWarps.Title, movie.Title);
			Assert.AreEqual(SmbWarps.Branch, movie.Branch);
		}

		[TestMethod]
		public async Task ForGame_SinglePublication_NoObsolete_EmptyList()
		{
			_db.Add(Smb);
			_db.Add(SmbWarps);
			_db.SaveChanges();

			var actual = await _publicationHistory.ForGame(Smb.Id);
			Assert.IsNotNull(actual);
			Assert.IsNotNull(actual!.Branches);

			var branchList = actual.Branches.ToList();
			Assert.AreEqual(1, branchList.Count);

			var movie = branchList.Single();
			Assert.IsNotNull(movie.Obsoletes);
			Assert.AreEqual(0, movie.Obsoletes.Count());
		}

		[TestMethod]
		public async Task ForGame_MultiBranch_ResultMatches()
		{
			_db.Add(Smb);
			_db.Add(SmbWarps);
			_db.Add(SmbWarpless);
			_db.SaveChanges();

			var actual = await _publicationHistory.ForGame(Smb.Id);
			Assert.IsNotNull(actual);
			Assert.IsNotNull(actual!.Branches);

			var branchList = actual.Branches.ToList();
			Assert.AreEqual(2, branchList.Count);

			Assert.AreEqual(1, branchList.Count(b => b.Branch == SmbWarps.Branch));
			Assert.AreEqual(1, branchList.Count(b => b.Branch == SmbWarpless.Branch));
		}

		[TestMethod]
		public async Task ForGame_ObsoleteBranch_NotParentNode()
		{
			_db.Add(Smb);
			_db.Add(SmbWarps);
			_db.Add(SmbWarpsObsoleteBranch);
			_db.SaveChanges();

			var actual = await _publicationHistory.ForGame(Smb.Id);
			Assert.IsNotNull(actual);
			Assert.IsNotNull(actual!.Branches);

			var branchList = actual.Branches.ToList();
			Assert.AreEqual(1, branchList.Count);
			Assert.AreEqual(SmbWarps.Branch, branchList.Single().Branch);
		}

		[TestMethod]
		public async Task ForGame_ReturnsObsolete()
		{
			_db.Add(Smb);
			_db.Add(SmbWarps);
			_db.Add(SmbWarpsObsolete);
			_db.SaveChanges();

			var actual = await _publicationHistory.ForGame(Smb.Id);
			Assert.IsNotNull(actual);
			Assert.IsNotNull(actual!.Branches);

			var branchList = actual.Branches.ToList();
			Assert.AreEqual(1, branchList.Count);

			var currentPub = branchList.Single();
			Assert.AreEqual(SmbWarps.Id, currentPub.Id);

			Assert.IsNotNull(currentPub.Obsoletes);
			var obsolete = currentPub.Obsoletes.SingleOrDefault();

			Assert.IsNotNull(obsolete);
			Assert.AreEqual(SmbWarpsObsolete.Id, obsolete!.Id);
		}

		[TestMethod]
		public async Task ForGame_OnePubWithMultipleObsoletions()
		{
			_db.Add(Smb);
			_db.Add(SmbWarps);
			_db.Add(SmbWarpsObsolete);
			_db.Add(SmbWarpsObsoleteBranch);
			_db.SaveChanges();

			var actual = await _publicationHistory.ForGame(Smb.Id);
			Assert.IsNotNull(actual);
			Assert.IsNotNull(actual!.Branches);

			var branchList = actual.Branches.ToList();
			Assert.AreEqual(1, branchList.Count);

			var currentPub = branchList.Single();
			Assert.AreEqual(SmbWarps.Id, currentPub.Id);

			Assert.IsNotNull(currentPub.Obsoletes);
			var obsoletes = currentPub.Obsoletes.ToList();
			Assert.AreEqual(2, obsoletes.Count);
			Assert.AreEqual(1, obsoletes.Count(o => o.Id == SmbWarpsObsolete.Id));
			Assert.AreEqual(1, obsoletes.Count(o => o.Id == SmbWarpsObsoleteBranch.Id));
		}

		[TestMethod]
		public async Task ForGame_ObsoletionChain()
		{
			_db.Add(Smb);
			_db.Add(SmbWarps);
			_db.Add(SmbWarpsObsolete);
			_db.Add(SmbWarpsObsoleteObsolete);
			_db.SaveChanges();

			var actual = await _publicationHistory.ForGame(Smb.Id);
			Assert.IsNotNull(actual);
			Assert.IsNotNull(actual!.Branches);

			var branchList = actual.Branches.ToList();
			Assert.AreEqual(1, branchList.Count);

			var currentPub = branchList.Single();
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
}
