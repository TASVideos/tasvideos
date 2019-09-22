using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Game;
using TASVideos.Services;
using TASVideos.Services.PublicationChain;

// ReSharper disable InconsistentNaming
namespace TASVideos.Test.Services
{
	[TestClass]
	public class PublicationHistoryTests
	{
		private IPublicationHistory _publicationHistory;
		private TestDbContext _db;

		#region Test Data

		private static readonly Game Smb = new Game { Id = 1 };
		private static readonly Game Smb2j = new Game { Id = 2 };
		private static readonly Publication SmbWarps = new Publication
		{
			Id = 1,
			GameId = Smb.Id,
			Title = "Smb in less than 5 minutes",
			Branch = "Warps"
		};

		private static readonly Publication Smb2jWarps = new Publication
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
			_publicationHistory = new PublicationHistory(_db, new NoCacheService());
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
			Assert.AreEqual(Smb.Id, actual.GameId);
		}

		[TestMethod]
		public async Task ForGame_NoPublications_BranchesEmpty()
		{
			_db.Add(Smb);
			_db.SaveChanges();

			var actual = await _publicationHistory.ForGame(Smb.Id);
			Assert.IsNotNull(actual);
			Assert.IsNotNull(actual.Branches);
			Assert.AreEqual(0, actual.Branches.Count());
		}

		[TestMethod]
		public async Task ForGame_SinglePublication_ResultMatches()
		{
			_db.Add(Smb);
			_db.Add(SmbWarps);
			_db.SaveChanges();

			var actual = await _publicationHistory.ForGame(Smb.Id);
			Assert.IsNotNull(actual);
			Assert.IsNotNull(actual.Branches);
			
			var branchList = actual.Branches.ToList();
			Assert.AreEqual(1, branchList.Count);
			
			var movie = branchList.Single();
			Assert.AreEqual(SmbWarps.Id, movie.Id);
			Assert.AreEqual(SmbWarps.Title, movie.Title);
			Assert.AreEqual(SmbWarps.Branch, movie.Branch);
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
			Assert.AreEqual(Smb.Id, actual.GameId);
			Assert.IsNotNull(actual.Branches);

			var branchList = actual.Branches.ToList();
			Assert.AreEqual(1, branchList.Count);
			Assert.AreEqual(SmbWarps.Id, branchList.Single().Id);
		}
	}
}
