using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TASVideos.Core.Services;
using TASVideos.Data.Entity;
using TASVideos.Tests.Base;

namespace TASVideos.Core.Tests.Services
{
	[TestClass]
	public class TierServiceTests
	{
		private readonly TestDbContext _db;
		private readonly TestCache _cache;
		private readonly TierService _tierService;

		public TierServiceTests()
		{
			_db = TestDbContext.Create();
			_cache = new TestCache();
			_tierService = new TierService(_db, _cache);
		}

		[TestMethod]
		public async Task GetAll_EmptyDb_CachesAndReturnsEmpty()
		{
			var result = await _tierService.GetAll();
			Assert.IsNotNull(result);
			Assert.AreEqual(0, result.Count);
			Assert.IsTrue(_cache.ContainsKey(TierService.TiersKey));
		}

		[TestMethod]
		public async Task GetAll_CachesAndReturnsEmpty()
		{
			_db.Add(new Tier());
			await _db.SaveChangesAsync();

			var result = await _tierService.GetAll();
			Assert.IsNotNull(result);
			Assert.AreEqual(1, result.Count);
			Assert.IsTrue(_cache.ContainsKey(TierService.TiersKey));
		}

		[TestMethod]
		public async Task GetById_DoesNotExist_CachesAndReturnsNull()
		{
			var result = await _tierService.GetById(-1);
			Assert.IsNull(result);
			Assert.IsTrue(_cache.ContainsKey(TierService.TiersKey));
		}

		[TestMethod]
		public async Task GetById_Exists_ReturnsFlag()
		{
			const int id = 1;
			const string name = "Test";
			_db.Tiers.Add(new Tier { Id = id, Name = name });
			await _db.SaveChangesAsync();

			var result = await _tierService.GetById(id);
			Assert.IsNotNull(result);
			Assert.AreEqual(name, result!.Name);
			Assert.IsTrue(_cache.ContainsKey(TierService.TiersKey));
		}

		[TestMethod]
		public async Task InUse_DoesNotExist_ReturnsFalse()
		{
			var result = await _tierService.InUse(-1);
			Assert.IsFalse(result);
		}

		[TestMethod]
		public async Task InUse_Exists_ReturnsFalse()
		{
			const int tierId = 1;
			const int publicationId = 1;
			_db.Tiers.Add(new Tier { Id = tierId });
			_db.Publications.Add(new Publication { Id = publicationId, TierId = tierId });
			await _db.SaveChangesAsync();

			var result = await _tierService.InUse(tierId);
			Assert.IsTrue(result);
		}

		[TestMethod]
		public async Task Add_Success_FlushesCaches()
		{
			const int identity = 1;
			_db.Tiers.Add(new Tier { Id = identity });
			await _db.SaveChangesAsync();

			var tier = new Tier
			{
				Id = identity + 10,
				Name = "Name",
				IconPath = "IconPath",
				Link = "Link",
				Weight = 1
			};

			var (id, result) = await _tierService.Add(tier);

			Assert.AreEqual(TierEditResult.Success, result);
			Assert.AreEqual(identity + 1, id);
			Assert.AreEqual(2, _db.Tiers.Count());
			var savedTier = _db.Tiers.Last();
			Assert.AreEqual(identity + 1, savedTier.Id);
			Assert.AreEqual(tier.Name, savedTier.Name);
			Assert.AreEqual(tier.IconPath, savedTier.IconPath);
			Assert.AreEqual(tier.Link, savedTier.Link);
			Assert.AreEqual(tier.Weight, savedTier.Weight);
			Assert.IsFalse(_cache.ContainsKey(TierService.TiersKey));
		}

		[TestMethod]
		public async Task Add_DuplicateError_DoesNotFlushCache()
		{
			const string name = "Test";
			_db.Tiers.Add(new Tier { Id = 1, Name = name });
			_cache.Set(TierService.TiersKey, new object());
			await _db.SaveChangesAsync();
			_db.CreateUpdateConflict();

			var (id, result) = await _tierService.Add(new Tier { Id = 2, Name = name });
			Assert.AreEqual(TierEditResult.DuplicateName, result);
			Assert.IsNull(id);
			Assert.IsTrue(_cache.ContainsKey(TierService.TiersKey));
		}

		[TestMethod]
		public async Task Add_ConcurrencyError_DoesNotFlushCache()
		{
			_db.Tiers.Add(new Tier { Id = 1, Name = "name1" });
			_cache.Set(TierService.TiersKey, new object());
			await _db.SaveChangesAsync();
			_db.CreateConcurrentUpdateConflict();

			var (id, result) = await _tierService.Add(new Tier { Id = 2, Name = "name2" });
			Assert.AreEqual(TierEditResult.Fail, result);
			Assert.IsNull(id);
			Assert.IsTrue(_cache.ContainsKey(TierService.TiersKey));
		}

		[TestMethod]
		public async Task Edit_Success_FlushesCaches()
		{
			const int id = 1;
			const string newName = "Test";
			_db.Tiers.Add(new Tier { Id = id });
			await _db.SaveChangesAsync();

			var result = await _tierService.Edit(id, new Tier { Name = newName });

			Assert.AreEqual(TierEditResult.Success, result);
			Assert.AreEqual(1, _db.Tiers.Count());
			var tier = _db.Tiers.Single();
			Assert.AreEqual(newName, tier.Name);
			Assert.IsFalse(_cache.ContainsKey(TierService.TiersKey));
		}

		[TestMethod]
		public async Task Edit_NotFound_DoesNotFlushCache()
		{
			var id = 1;
			_db.Tiers.Add(new Tier { Id = id });
			await _db.SaveChangesAsync();
			_cache.Set(TierService.TiersKey, new object());

			var result = await _tierService.Edit(id + 1, new Tier());

			Assert.AreEqual(TierEditResult.NotFound, result);
			Assert.IsTrue(_cache.ContainsKey(TierService.TiersKey));
		}

		[TestMethod]
		public async Task Edit_DuplicateError_DoesNotFlushCache()
		{
			const int id = 1;
			const string name = "Test";
			_db.Tiers.Add(new Tier { Id = id, Name = name });
			_cache.Set(TierService.TiersKey, new object());
			await _db.SaveChangesAsync();
			_db.CreateUpdateConflict();

			var result = await _tierService.Edit(id, new Tier());
			Assert.AreEqual(TierEditResult.DuplicateName, result);
			Assert.IsTrue(_cache.ContainsKey(TierService.TiersKey));
		}

		[TestMethod]
		public async Task Edit_ConcurrencyError_DoesNotFlushCache()
		{
			const int id = 1;
			const string name = "Test";
			_db.Tiers.Add(new Tier { Id = id, Name = name });
			_cache.Set(TierService.TiersKey, new object());
			await _db.SaveChangesAsync();
			_db.CreateConcurrentUpdateConflict();

			var result = await _tierService.Edit(id, new Tier());
			Assert.AreEqual(TierEditResult.Fail, result);
			Assert.IsTrue(_cache.ContainsKey(TierService.TiersKey));
		}

		[TestMethod]
		public async Task Delete_Success_FlushesCache()
		{
			const int id = 1;
			var tier = new Tier { Id = id };
			_db.Tiers.Add(tier);
			await _db.SaveChangesAsync();

			var result = await _tierService.Delete(id);
			Assert.AreEqual(TierDeleteResult.Success, result);
			Assert.IsFalse(_cache.ContainsKey(TierService.TiersKey));
		}

		[TestMethod]
		public async Task Delete_NotFound_DoesNotFlushCache()
		{
			_cache.Set(TierService.TiersKey, new object());

			var result = await _tierService.Delete(-1);
			Assert.AreEqual(TierDeleteResult.NotFound, result);
			Assert.IsTrue(_cache.ContainsKey(TierService.TiersKey));
		}

		[TestMethod]
		public async Task Delete_InUse_FlushesNotCache()
		{
			const int tierId = 1;
			const int publicationId = 1;
			var tier = new Tier { Id = tierId };
			_db.Tiers.Add(tier);
			_cache.Set(TierService.TiersKey, new object());
			_db.Publications.Add(new Publication { Id = publicationId, TierId = tierId });
			await _db.SaveChangesAsync();

			var result = await _tierService.Delete(tierId);
			Assert.AreEqual(TierDeleteResult.InUse, result);
			Assert.IsTrue(_cache.ContainsKey(TierService.TiersKey));
		}

		[TestMethod]
		public async Task Delete_ConcurrencyError_FlushesNotCache()
		{
			const int id = 1;
			_db.Tiers.Add(new Tier { Id = 1 });
			await _db.SaveChangesAsync();
			_cache.Set(TierService.TiersKey, new object());
			_db.CreateConcurrentUpdateConflict();

			var result = await _tierService.Delete(id);
			Assert.AreEqual(TierDeleteResult.Fail, result);
			Assert.IsTrue(_cache.ContainsKey(TierService.TiersKey));
		}
	}
}
