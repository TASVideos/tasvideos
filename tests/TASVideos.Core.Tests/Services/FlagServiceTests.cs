using TASVideos.Core.Services;
using TASVideos.Data.Entity;

namespace TASVideos.Core.Tests.Services;

[TestClass]
public class FlagServiceTests
{
	private readonly TestDbContext _db;
	private readonly TestCache _cache;
	private readonly FlagService _flagService;

	public FlagServiceTests()
	{
		_db = TestDbContext.Create();
		_cache = new TestCache();
		_flagService = new FlagService(_db, _cache);
	}

	[TestMethod]
	public async Task GetAll_EmptyDb_CachesAndReturnsEmpty()
	{
		var result = await _flagService.GetAll();
		Assert.IsNotNull(result);
		Assert.AreEqual(0, result.Count);
		Assert.IsTrue(_cache.ContainsKey(FlagService.FlagsKey));
	}

	[TestMethod]
	public async Task GetAll_CachesAndReturnsEmpty()
	{
		_db.Add(new Flag());
		await _db.SaveChangesAsync();

		var result = await _flagService.GetAll();
		Assert.IsNotNull(result);
		Assert.AreEqual(1, result.Count);
		Assert.IsTrue(_cache.ContainsKey(FlagService.FlagsKey));
	}

	[TestMethod]
	public async Task GetById_DoesNotExist_CachesAndReturnsNull()
	{
		var result = await _flagService.GetById(-1);
		Assert.IsNull(result);
		Assert.IsTrue(_cache.ContainsKey(FlagService.FlagsKey));
	}

	[TestMethod]
	public async Task GetById_Exists_ReturnsFlag()
	{
		const int id = 1;
		const string token = "Test";
		_db.Flags.Add(new Flag { Id = id, Token = token });
		await _db.SaveChangesAsync();

		var result = await _flagService.GetById(id);
		Assert.IsNotNull(result);
		Assert.AreEqual(token, result!.Token);
		Assert.IsTrue(_cache.ContainsKey(FlagService.FlagsKey));
	}

	[TestMethod]
	public async Task GetDiff_BasicTest()
	{
		const int id1 = 1;
		const string token1 = "Test";
		_db.Flags.Add(new Flag { Id = id1, Token = token1 });
		const int id2 = 2;
		const string token2 = "Test2";
		_db.Flags.Add(new Flag { Id = id2, Token = token2 });
		await _db.SaveChangesAsync();

		var result = await _flagService.GetDiff(new[] { id1 }, new[] { id2 });
		Assert.IsNotNull(result);
		Assert.AreEqual(1, result.Added.Count);
		Assert.AreEqual(1, result.Removed.Count);
	}

	[TestMethod]
	public async Task InUse_DoesNotExist_ReturnsFalse()
	{
		var result = await _flagService.InUse(-1);
		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task InUse_Exists_ReturnsFalse()
	{
		const int flagId = 1;
		const int publicationId = 1;
		_db.Flags.Add(new Flag { Id = flagId });
		_db.Publications.Add(new Publication { Id = publicationId });
		_db.PublicationFlags.Add(new PublicationFlag { PublicationId = publicationId, FlagId = flagId });
		await _db.SaveChangesAsync();

		var result = await _flagService.InUse(flagId);
		Assert.IsTrue(result);
	}

	[TestMethod]
	public async Task Add_Success_FlushesCaches()
	{
		const int identity = 1;
		_db.Flags.Add(new Flag { Id = identity });
		await _db.SaveChangesAsync();

		var flag = new Flag
		{
			Id = identity + 10,
			Name = "Name",
			IconPath = "IconPath",
			LinkPath = "LinkPath",
			Token = "Token",
			PermissionRestriction = PermissionTo.AssignRoles
		};

		var result = await _flagService.Add(flag);

		Assert.AreEqual(FlagEditResult.Success, result);
		Assert.AreEqual(2, _db.Flags.Count());
		var savedFlag = _db.Flags.Last();
		Assert.AreEqual(identity + 1, savedFlag.Id);
		Assert.AreEqual(flag.Name, savedFlag.Name);
		Assert.AreEqual(flag.IconPath, savedFlag.IconPath);
		Assert.AreEqual(flag.LinkPath, savedFlag.LinkPath);
		Assert.AreEqual(flag.Token, savedFlag.Token);
		Assert.AreEqual(flag.PermissionRestriction, savedFlag.PermissionRestriction);
		Assert.IsFalse(_cache.ContainsKey(FlagService.FlagsKey));
	}

	[TestMethod]
	public async Task Add_DuplicateError_DoesNotFlushCache()
	{
		const string token = "Test";
		_db.Flags.Add(new Flag { Id = 1, Token = token });
		_cache.Set(FlagService.FlagsKey, new object());
		await _db.SaveChangesAsync();
		_db.CreateUpdateConflict();

		var result = await _flagService.Add(new Flag { Id = 2, Token = token });
		Assert.AreEqual(FlagEditResult.DuplicateCode, result);
		Assert.IsTrue(_cache.ContainsKey(FlagService.FlagsKey));
	}

	[TestMethod]
	public async Task Add_ConcurrencyError_DoesNotFlushCache()
	{
		_db.Flags.Add(new Flag { Id = 1, Token = "token1" });
		_cache.Set(FlagService.FlagsKey, new object());
		await _db.SaveChangesAsync();
		_db.CreateConcurrentUpdateConflict();

		var result = await _flagService.Add(new Flag { Id = 2, Token = "token2" });
		Assert.AreEqual(FlagEditResult.Fail, result);
		Assert.IsTrue(_cache.ContainsKey(FlagService.FlagsKey));
	}

	[TestMethod]
	public async Task Edit_Success_FlushesCaches()
	{
		const int id = 1;
		const string newToken = "Test";
		_db.Flags.Add(new Flag { Id = id });
		await _db.SaveChangesAsync();

		var result = await _flagService.Edit(id, new Flag { Token = newToken });

		Assert.AreEqual(FlagEditResult.Success, result);
		Assert.AreEqual(1, _db.Flags.Count());
		var flag = _db.Flags.Single();
		Assert.AreEqual(newToken, flag.Token);
		Assert.IsFalse(_cache.ContainsKey(FlagService.FlagsKey));
	}

	[TestMethod]
	public async Task Edit_NotFound_DoesNotFlushCache()
	{
		var id = 1;
		_db.Flags.Add(new Flag { Id = id });
		await _db.SaveChangesAsync();
		_cache.Set(FlagService.FlagsKey, new object());

		var result = await _flagService.Edit(id + 1, new Flag());

		Assert.AreEqual(FlagEditResult.NotFound, result);
		Assert.IsTrue(_cache.ContainsKey(FlagService.FlagsKey));
	}

	[TestMethod]
	public async Task Edit_DuplicateError_DoesNotFlushCache()
	{
		const int id = 1;
		const string token = "Test";
		_db.Flags.Add(new Flag { Id = id, Token = token });
		_cache.Set(FlagService.FlagsKey, new object());
		await _db.SaveChangesAsync();
		_db.CreateUpdateConflict();

		var result = await _flagService.Edit(id, new Flag());
		Assert.AreEqual(FlagEditResult.DuplicateCode, result);
		Assert.IsTrue(_cache.ContainsKey(FlagService.FlagsKey));
	}

	[TestMethod]
	public async Task Edit_ConcurrencyError_DoesNotFlushCache()
	{
		const int id = 1;
		const string token = "Test";
		_db.Flags.Add(new Flag { Id = id, Token = token });
		_cache.Set(FlagService.FlagsKey, new object());
		await _db.SaveChangesAsync();
		_db.CreateConcurrentUpdateConflict();

		var result = await _flagService.Edit(id, new Flag());
		Assert.AreEqual(FlagEditResult.Fail, result);
		Assert.IsTrue(_cache.ContainsKey(FlagService.FlagsKey));
	}

	[TestMethod]
	public async Task Delete_Success_FlushesCache()
	{
		const int id = 1;
		var flag = new Flag { Id = id };
		_db.Flags.Add(flag);
		await _db.SaveChangesAsync();

		var result = await _flagService.Delete(id);
		Assert.AreEqual(FlagDeleteResult.Success, result);
		Assert.IsFalse(_cache.ContainsKey(FlagService.FlagsKey));
	}

	[TestMethod]
	public async Task Delete_NotFound_DoesNotFlushCache()
	{
		_cache.Set(FlagService.FlagsKey, new object());

		var result = await _flagService.Delete(-1);
		Assert.AreEqual(FlagDeleteResult.NotFound, result);
		Assert.IsTrue(_cache.ContainsKey(FlagService.FlagsKey));
	}

	[TestMethod]
	public async Task Delete_InUse_FlushesNotCache()
	{
		const int flagId = 1;
		const int publicationId = 1;
		var flag = new Flag { Id = flagId };
		_db.Flags.Add(flag);
		_cache.Set(FlagService.FlagsKey, new object());
		_db.Publications.Add(new Publication { Id = publicationId });
		_db.PublicationFlags.Add(new PublicationFlag { PublicationId = publicationId, FlagId = flagId });
		await _db.SaveChangesAsync();

		var result = await _flagService.Delete(flagId);
		Assert.AreEqual(FlagDeleteResult.InUse, result);
		Assert.IsTrue(_cache.ContainsKey(FlagService.FlagsKey));
	}

	[TestMethod]
	public async Task Delete_ConcurrencyError_FlushesNotCache()
	{
		const int id = 1;
		_db.Flags.Add(new Flag { Id = 1 });
		await _db.SaveChangesAsync();
		_cache.Set(FlagService.FlagsKey, new object());
		_db.CreateConcurrentUpdateConflict();

		var result = await _flagService.Delete(id);
		Assert.AreEqual(FlagDeleteResult.Fail, result);
		Assert.IsTrue(_cache.ContainsKey(FlagService.FlagsKey));
	}
}
