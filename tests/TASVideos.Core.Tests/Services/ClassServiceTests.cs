using TASVideos.Core.Services;
using TASVideos.Data.Entity;

namespace TASVideos.Core.Tests.Services;

[TestClass]
public class ClassServiceTests
{
	private readonly TestDbContext _db;
	private readonly TestCache _cache;
	private readonly ClassService _classService;

	public ClassServiceTests()
	{
		_db = TestDbContext.Create();
		_cache = new TestCache();
		_classService = new ClassService(_db, _cache);
	}

	[TestMethod]
	public async Task GetAll_EmptyDb_CachesAndReturnsEmpty()
	{
		var result = await _classService.GetAll();
		Assert.IsNotNull(result);
		Assert.AreEqual(0, result.Count);
		Assert.IsTrue(_cache.ContainsKey(ClassService.ClassesKey));
	}

	[TestMethod]
	public async Task GetAll_CachesAndReturnsEmpty()
	{
		_db.Add(new PublicationClass());
		await _db.SaveChangesAsync();

		var result = await _classService.GetAll();
		Assert.IsNotNull(result);
		Assert.AreEqual(1, result.Count);
		Assert.IsTrue(_cache.ContainsKey(ClassService.ClassesKey));
	}

	[TestMethod]
	public async Task GetById_DoesNotExist_CachesAndReturnsNull()
	{
		var result = await _classService.GetById(-1);
		Assert.IsNull(result);
		Assert.IsTrue(_cache.ContainsKey(ClassService.ClassesKey));
	}

	[TestMethod]
	public async Task GetById_Exists_ReturnsFlag()
	{
		const int id = 1;
		const string name = "Test";
		_db.PublicationClasses.Add(new PublicationClass { Id = id, Name = name });
		await _db.SaveChangesAsync();

		var result = await _classService.GetById(id);
		Assert.IsNotNull(result);
		Assert.AreEqual(name, result.Name);
		Assert.IsTrue(_cache.ContainsKey(ClassService.ClassesKey));
	}

	[TestMethod]
	public async Task InUse_DoesNotExist_ReturnsFalse()
	{
		var result = await _classService.InUse(-1);
		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task InUse_Exists_ReturnsFalse()
	{
		const int classId = 1;
		const int publicationId = 1;
		_db.PublicationClasses.Add(new PublicationClass { Id = classId });
		_db.Publications.Add(new Publication { Id = publicationId, PublicationClassId = classId });
		await _db.SaveChangesAsync();

		var result = await _classService.InUse(classId);
		Assert.IsTrue(result);
	}

	[TestMethod]
	public async Task Add_Success_FlushesCaches()
	{
		const int identity = 1;
		_db.PublicationClasses.Add(new PublicationClass { Id = identity });
		await _db.SaveChangesAsync();

		var publicationClass = new PublicationClass
		{
			Id = identity + 10,
			Name = "Name",
			IconPath = "IconPath",
			Link = "Link",
			Weight = 1
		};

		var (id, result) = await _classService.Add(publicationClass);

		Assert.AreEqual(ClassEditResult.Success, result);
		Assert.AreEqual(identity + 1, id);
		Assert.AreEqual(2, _db.PublicationClasses.Count());
		var savedClass = _db.PublicationClasses.Last();
		Assert.AreEqual(identity + 1, savedClass.Id);
		Assert.AreEqual(publicationClass.Name, savedClass.Name);
		Assert.AreEqual(publicationClass.IconPath, savedClass.IconPath);
		Assert.AreEqual(publicationClass.Link, savedClass.Link);
		Assert.AreEqual(publicationClass.Weight, savedClass.Weight);
		Assert.IsFalse(_cache.ContainsKey(ClassService.ClassesKey));
	}

	[TestMethod]
	public async Task Add_DuplicateError_DoesNotFlushCache()
	{
		const string name = "Test";
		_db.PublicationClasses.Add(new PublicationClass { Id = 1, Name = name });
		_cache.Set(ClassService.ClassesKey, new object());
		await _db.SaveChangesAsync();
		_db.CreateUpdateConflict();

		var (id, result) = await _classService.Add(new PublicationClass { Id = 2, Name = name });
		Assert.AreEqual(ClassEditResult.DuplicateName, result);
		Assert.IsNull(id);
		Assert.IsTrue(_cache.ContainsKey(ClassService.ClassesKey));
	}

	[TestMethod]
	public async Task Add_ConcurrencyError_DoesNotFlushCache()
	{
		_db.PublicationClasses.Add(new PublicationClass { Id = 1, Name = "name1" });
		_cache.Set(ClassService.ClassesKey, new object());
		await _db.SaveChangesAsync();
		_db.CreateConcurrentUpdateConflict();

		var (id, result) = await _classService.Add(new PublicationClass { Id = 2, Name = "name2" });
		Assert.AreEqual(ClassEditResult.Fail, result);
		Assert.IsNull(id);
		Assert.IsTrue(_cache.ContainsKey(ClassService.ClassesKey));
	}

	[TestMethod]
	public async Task Edit_Success_FlushesCaches()
	{
		const int id = 1;
		const string newName = "Test";
		_db.PublicationClasses.Add(new PublicationClass { Id = id });
		await _db.SaveChangesAsync();

		var result = await _classService.Edit(id, new PublicationClass { Name = newName });

		Assert.AreEqual(ClassEditResult.Success, result);
		Assert.AreEqual(1, _db.PublicationClasses.Count());
		var publicationClass = _db.PublicationClasses.Single();
		Assert.AreEqual(newName, publicationClass.Name);
		Assert.IsFalse(_cache.ContainsKey(ClassService.ClassesKey));
	}

	[TestMethod]
	public async Task Edit_NotFound_DoesNotFlushCache()
	{
		var id = 1;
		_db.PublicationClasses.Add(new PublicationClass { Id = id });
		await _db.SaveChangesAsync();
		_cache.Set(ClassService.ClassesKey, new object());

		var result = await _classService.Edit(id + 1, new PublicationClass());

		Assert.AreEqual(ClassEditResult.NotFound, result);
		Assert.IsTrue(_cache.ContainsKey(ClassService.ClassesKey));
	}

	[TestMethod]
	public async Task Edit_DuplicateError_DoesNotFlushCache()
	{
		const int id = 1;
		const string name = "Test";
		_db.PublicationClasses.Add(new PublicationClass { Id = id, Name = name });
		_cache.Set(ClassService.ClassesKey, new object());
		await _db.SaveChangesAsync();
		_db.CreateUpdateConflict();

		var result = await _classService.Edit(id, new PublicationClass());
		Assert.AreEqual(ClassEditResult.DuplicateName, result);
		Assert.IsTrue(_cache.ContainsKey(ClassService.ClassesKey));
	}

	[TestMethod]
	public async Task Edit_ConcurrencyError_DoesNotFlushCache()
	{
		const int id = 1;
		const string name = "Test";
		_db.PublicationClasses.Add(new PublicationClass { Id = id, Name = name });
		_cache.Set(ClassService.ClassesKey, new object());
		await _db.SaveChangesAsync();
		_db.CreateConcurrentUpdateConflict();

		var result = await _classService.Edit(id, new PublicationClass());
		Assert.AreEqual(ClassEditResult.Fail, result);
		Assert.IsTrue(_cache.ContainsKey(ClassService.ClassesKey));
	}

	[TestMethod]
	public async Task Delete_Success_FlushesCache()
	{
		const int id = 1;
		var publicationClass = new PublicationClass { Id = id };
		_db.PublicationClasses.Add(publicationClass);
		await _db.SaveChangesAsync();

		var result = await _classService.Delete(id);
		Assert.AreEqual(ClassDeleteResult.Success, result);
		Assert.IsFalse(_cache.ContainsKey(ClassService.ClassesKey));
	}

	[TestMethod]
	public async Task Delete_NotFound_DoesNotFlushCache()
	{
		_cache.Set(ClassService.ClassesKey, new object());

		var result = await _classService.Delete(-1);
		Assert.AreEqual(ClassDeleteResult.NotFound, result);
		Assert.IsTrue(_cache.ContainsKey(ClassService.ClassesKey));
	}

	[TestMethod]
	public async Task Delete_InUse_FlushesNotCache()
	{
		const int classId = 1;
		const int publicationId = 1;
		var publicationClass = new PublicationClass { Id = classId };
		_db.PublicationClasses.Add(publicationClass);
		_cache.Set(ClassService.ClassesKey, new object());
		_db.Publications.Add(new Publication { Id = publicationId, PublicationClassId = classId });
		await _db.SaveChangesAsync();

		var result = await _classService.Delete(classId);
		Assert.AreEqual(ClassDeleteResult.InUse, result);
		Assert.IsTrue(_cache.ContainsKey(ClassService.ClassesKey));
	}

	[TestMethod]
	public async Task Delete_ConcurrencyError_FlushesNotCache()
	{
		const int id = 1;
		_db.PublicationClasses.Add(new PublicationClass { Id = 1 });
		await _db.SaveChangesAsync();
		_cache.Set(ClassService.ClassesKey, new object());
		_db.CreateConcurrentUpdateConflict();

		var result = await _classService.Delete(id);
		Assert.AreEqual(ClassDeleteResult.Fail, result);
		Assert.IsTrue(_cache.ContainsKey(ClassService.ClassesKey));
	}
}
