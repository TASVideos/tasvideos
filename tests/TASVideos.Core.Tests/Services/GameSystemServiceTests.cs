using TASVideos.Core.Services;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Game;

namespace TASVideos.Core.Tests.Services;

[TestClass]
public class GameSystemServiceTests
{
	private readonly TestDbContext _db;
	private readonly TestCache _cache;
	private readonly GameSystemService _systemService;

	public GameSystemServiceTests()
	{
		_db = TestDbContext.Create();
		_cache = new TestCache();
		_systemService = new GameSystemService(_db, _cache);
	}

	[TestMethod]
	public async Task GetAll_EmptyDb_CachesAndReturnsEmpty()
	{
		var result = await _systemService.GetAll();
		Assert.IsNotNull(result);
		Assert.AreEqual(0, result.Count);
		Assert.IsTrue(_cache.ContainsKey(GameSystemService.SystemsKey));
	}

	[TestMethod]
	public async Task GetAll_CachesAndReturns()
	{
		_db.Add(new GameSystem());
		await _db.SaveChangesAsync();

		var result = await _systemService.GetAll();
		Assert.IsNotNull(result);
		Assert.AreEqual(1, result.Count);
		Assert.IsTrue(_cache.ContainsKey(GameSystemService.SystemsKey));
	}

	[TestMethod]
	public async Task GetById_DoesNotExist_CachesAndReturnsNull()
	{
		var result = await _systemService.GetById(-1);
		Assert.IsNull(result);
		Assert.IsTrue(_cache.ContainsKey(GameSystemService.SystemsKey));
	}

	[TestMethod]
	public async Task GetById_Exists_ReturnsSystem()
	{
		const int id = 1;
		const string code = "Test";
		_db.GameSystems.Add(new GameSystem { Id = id, Code = code });
		await _db.SaveChangesAsync();

		var result = await _systemService.GetById(id);
		Assert.IsNotNull(result);
		Assert.AreEqual(code, result.Code);
		Assert.IsTrue(_cache.ContainsKey(GameSystemService.SystemsKey));
	}

	[TestMethod]
	public async Task InUse_DoesNotExist_ReturnsFalse()
	{
		var result = await _systemService.InUse(-1);
		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task InUse_PublicationExists_ReturnsTrue()
	{
		const int systemId = 1;
		const int publicationId = 1;
		_db.GameSystems.Add(new GameSystem { Id = systemId });
		_db.Publications.Add(new Publication { Id = publicationId, SystemId = systemId });
		await _db.SaveChangesAsync();

		var result = await _systemService.InUse(systemId);
		Assert.IsTrue(result);
	}

	[TestMethod]
	public async Task InUse_SubmissionExists_ReturnsTrue()
	{
		const int systemId = 1;
		const int submissionId = 1;
		_db.GameSystems.Add(new GameSystem { Id = systemId });
		_db.Submissions.Add(new Submission { Id = submissionId, SystemId = systemId });
		await _db.SaveChangesAsync();

		var result = await _systemService.InUse(systemId);
		Assert.IsTrue(result);
	}

	[TestMethod]
	public async Task InUse_GameExists_ReturnsTrue()
	{
		const int systemId = 1;
		const int gameId = 1;
		_db.GameSystems.Add(new GameSystem { Id = systemId });
		_db.Games.Add(new Game { Id = gameId, SystemId = systemId });
		await _db.SaveChangesAsync();

		var result = await _systemService.InUse(systemId);
		Assert.IsTrue(result);
	}

	[TestMethod]
	public async Task InUse_UserFileExists_ReturnsTrue()
	{
		const int systemId = 1;
		const int userFileId = 1;
		_db.GameSystems.Add(new GameSystem { Id = systemId });
		_db.UserFiles.Add(new UserFile { Id = userFileId, SystemId = systemId });
		await _db.SaveChangesAsync();

		var result = await _systemService.InUse(systemId);
		Assert.IsTrue(result);
	}

	[TestMethod]
	public async Task NextId_NoEntries_Returns0()
	{
		var actual = await _systemService.NextId();
		Assert.AreEqual(0, actual);
	}

	[TestMethod]
	public async Task NextId_Entries_ReturnsNext()
	{
		_db.GameSystems.Add(new GameSystem { Id = 1 });
		_db.GameSystems.Add(new GameSystem { Id = 2 });
		await _db.SaveChangesAsync();

		var actual = await _systemService.NextId();
		Assert.AreEqual(3, actual);
	}

	[TestMethod]
	public async Task NextId_Gaps_ReturnsNext()
	{
		_db.GameSystems.Add(new GameSystem { Id = 1 });
		_db.GameSystems.Add(new GameSystem { Id = 3 });
		await _db.SaveChangesAsync();

		var actual = await _systemService.NextId();
		Assert.AreEqual(4, actual);
	}

	[TestMethod]
	public async Task Add_Success_FlushesCaches()
	{
		const int id = 1;
		const string code = "Test";
		const string displayName = "Display";

		var result = await _systemService.Add(id, code, displayName);

		Assert.AreEqual(SystemEditResult.Success, result);
		Assert.AreEqual(1, _db.GameSystems.Count());
		var system = _db.GameSystems.Single();
		Assert.AreEqual(code, system.Code);
		Assert.AreEqual(displayName, system.DisplayName);
		Assert.IsFalse(_cache.ContainsKey(GameSystemService.SystemsKey));
	}

	[TestMethod]
	public async Task Add_DuplicateIdError_DoesNotFlushCache()
	{
		const int id = 1;
		_db.GameSystems.Add(new GameSystem { Id = id, Code = "Test" });
		_cache.Set(GameSystemService.SystemsKey, new object());
		await _db.SaveChangesAsync();

		var result = await _systemService.Add(id, "New Code", "DisplayName");
		Assert.AreEqual(SystemEditResult.DuplicateId, result);
		Assert.IsTrue(_cache.ContainsKey(GameSystemService.SystemsKey));
	}

	[TestMethod]
	public async Task Add_DuplicateCodeError_DoesNotFlushCache()
	{
		const string code = "Test";
		_db.GameSystems.Add(new GameSystem { Id = 1, Code = code });
		_cache.Set(GameSystemService.SystemsKey, new object());
		await _db.SaveChangesAsync();
		_db.CreateUpdateConflict();

		var result = await _systemService.Add(2, code, "DisplayName");
		Assert.AreEqual(SystemEditResult.DuplicateCode, result);
		Assert.IsTrue(_cache.ContainsKey(GameSystemService.SystemsKey));
	}

	[TestMethod]
	public async Task Add_ConcurrencyError_DoesNotFlushCache()
	{
		const int id = 1;
		const string code = "Test";
		_db.GameSystems.Add(new GameSystem { Id = id, Code = code });
		_cache.Set(GameSystemService.SystemsKey, new object());
		await _db.SaveChangesAsync();
		_db.CreateConcurrentUpdateConflict();

		var result = await _systemService.Add(id + 1, "New Code", "DisplayName");
		Assert.AreEqual(SystemEditResult.Fail, result);
		Assert.IsTrue(_cache.ContainsKey(GameSystemService.SystemsKey));
	}

	[TestMethod]
	public async Task Edit_Success_FlushesCaches()
	{
		const int id = 1;
		const string newCode = "Test";
		const string newDisplayName = "Display";
		_db.GameSystems.Add(new GameSystem { Id = id });
		await _db.SaveChangesAsync();

		var result = await _systemService.Edit(id, newCode, newDisplayName);

		Assert.AreEqual(SystemEditResult.Success, result);
		Assert.AreEqual(1, _db.GameSystems.Count());
		var system = _db.GameSystems.Single();
		Assert.AreEqual(newCode, system.Code);
		Assert.AreEqual(newDisplayName, system.DisplayName);
		Assert.IsFalse(_cache.ContainsKey(GameSystemService.SystemsKey));
	}

	[TestMethod]
	public async Task Edit_NotFound_DoesNotFlushCache()
	{
		const int id = 1;
		_db.GameSystems.Add(new GameSystem { Id = id });
		await _db.SaveChangesAsync();
		_cache.Set(GameSystemService.SystemsKey, new object());

		var result = await _systemService.Edit(id + 1, "Test", "Test");

		Assert.AreEqual(SystemEditResult.NotFound, result);
		Assert.IsTrue(_cache.ContainsKey(GameSystemService.SystemsKey));
	}

	[TestMethod]
	public async Task Edit_DuplicateError_DoesNotFlushCache()
	{
		const int id = 1;
		const string code = "Test";
		_db.GameSystems.Add(new GameSystem { Id = id, Code = code });
		_cache.Set(GameSystemService.SystemsKey, new object());
		await _db.SaveChangesAsync();
		_db.CreateUpdateConflict();

		var result = await _systemService.Edit(id, code, "DisplayName");
		Assert.AreEqual(SystemEditResult.DuplicateCode, result);
		Assert.IsTrue(_cache.ContainsKey(GameSystemService.SystemsKey));
	}

	[TestMethod]
	public async Task Edit_ConcurrencyError_DoesNotFlushCache()
	{
		const int id = 1;
		const string code = "Test";
		_db.GameSystems.Add(new GameSystem { Id = id, Code = code });
		_cache.Set(GameSystemService.SystemsKey, new object());
		await _db.SaveChangesAsync();
		_db.CreateConcurrentUpdateConflict();

		var result = await _systemService.Edit(id, code, "DisplayName");
		Assert.AreEqual(SystemEditResult.Fail, result);
		Assert.IsTrue(_cache.ContainsKey(GameSystemService.SystemsKey));
	}

	[TestMethod]
	public async Task Delete_Success_FlushesCache()
	{
		const int id = 1;
		const string code = "Test";
		var system = new GameSystem { Id = id, Code = code };
		_db.GameSystems.Add(system);
		await _db.SaveChangesAsync();

		var result = await _systemService.Delete(id);
		Assert.AreEqual(SystemDeleteResult.Success, result);
		Assert.AreEqual(0, _db.GameSystems.Count());
		Assert.IsFalse(_cache.ContainsKey(GameSystemService.SystemsKey));
	}

	[TestMethod]
	public async Task Delete_InUse_DoesNotFlushesCache()
	{
		const int systemId = 1;
		const int publicationId = 1;
		var system = new GameSystem { Id = systemId, Code = "Test" };
		_db.GameSystems.Add(system);
		_cache.Set(GameSystemService.SystemsKey, new object());
		_db.Publications.Add(new Publication { Id = publicationId, SystemId = systemId });
		await _db.SaveChangesAsync();

		var result = await _systemService.Delete(systemId);
		Assert.AreEqual(SystemDeleteResult.InUse, result);
		Assert.IsTrue(_cache.ContainsKey(GameSystemService.SystemsKey));
	}
}
