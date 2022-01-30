using TASVideos.Core.Services;
using TASVideos.Data.Entity;

namespace TASVideos.Core.Tests.Services;

[TestClass]
public class TagServiceTests
{
	private readonly TestDbContext _db;
	private readonly TestCache _cache;
	private readonly TagService _tagService;

	public TagServiceTests()
	{
		_db = TestDbContext.Create();
		_cache = new TestCache();
		_tagService = new TagService(_db, _cache);
	}

	[TestMethod]
	public async Task GetAll_EmptyDb_CachesAndReturnsEmpty()
	{
		var result = await _tagService.GetAll();
		Assert.IsNotNull(result);
		Assert.AreEqual(0, result.Count);
		Assert.IsTrue(_cache.ContainsKey(TagService.TagsKey));
	}

	[TestMethod]
	public async Task GetAll_CachesAndReturnsEmpty()
	{
		_db.Add(new Tag());
		await _db.SaveChangesAsync();

		var result = await _tagService.GetAll();
		Assert.IsNotNull(result);
		Assert.AreEqual(1, result.Count);
		Assert.IsTrue(_cache.ContainsKey(TagService.TagsKey));
	}

	[TestMethod]
	public async Task GetById_DoesNotExist_CachesAndReturnsNull()
	{
		var result = await _tagService.GetById(-1);
		Assert.IsNull(result);
		Assert.IsTrue(_cache.ContainsKey(TagService.TagsKey));
	}

	[TestMethod]
	public async Task GetById_Exists_ReturnsTag()
	{
		const int id = 1;
		const string code = "Test";
		_db.Tags.Add(new Tag { Id = id, Code = code });
		await _db.SaveChangesAsync();

		var result = await _tagService.GetById(id);
		Assert.IsNotNull(result);
		Assert.AreEqual(code, result!.Code);
		Assert.IsTrue(_cache.ContainsKey(TagService.TagsKey));
	}

	[TestMethod]
	public async Task GetDiff_BasicTest()
	{
		const int id1 = 1;
		const string code1 = "Test";
		_db.Tags.Add(new Tag { Id = id1, Code = code1 });
		const int id2 = 2;
		const string code2 = "Test2";
		_db.Tags.Add(new Tag { Id = id2, Code = code2 });
		await _db.SaveChangesAsync();

		var result = await _tagService.GetDiff(new[] { id1 }, new[] { id2 });
		Assert.IsNotNull(result);
		Assert.AreEqual(1, result.Added.Count);
		Assert.AreEqual(1, result.Removed.Count);
	}

	[TestMethod]
	public async Task InUse_DoesNotExist_ReturnsFalse()
	{
		var result = await _tagService.InUse(-1);
		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task InUse_Exists_ReturnsFalse()
	{
		const int tagId = 1;
		const int publicationId = 1;
		_db.Tags.Add(new Tag { Id = tagId });
		_db.Publications.Add(new Publication { Id = publicationId });
		_db.PublicationTags.Add(new PublicationTag { PublicationId = publicationId, TagId = tagId });
		await _db.SaveChangesAsync();

		var result = await _tagService.InUse(tagId);
		Assert.IsTrue(result);
	}

	[TestMethod]
	public async Task Add_Success_FlushesCaches()
	{
		const string code = "Test";
		const string displayName = "Display";

		var (_, result) = await _tagService.Add(code, displayName);

		Assert.AreEqual(TagEditResult.Success, result);
		Assert.AreEqual(1, _db.Tags.Count());
		var tag = _db.Tags.Single();
		Assert.AreEqual(code, tag.Code);
		Assert.AreEqual(displayName, tag.DisplayName);
		Assert.IsFalse(_cache.ContainsKey(TagService.TagsKey));
	}

	[TestMethod]
	public async Task Add_DuplicateError_DoesNotFlushCache()
	{
		const string code = "Test";
		_db.Tags.Add(new Tag { Code = code });
		_cache.Set(TagService.TagsKey, new object());
		await _db.SaveChangesAsync();
		_db.CreateUpdateConflict();

		var (_, result) = await _tagService.Add(code, "DisplayName");
		Assert.AreEqual(TagEditResult.DuplicateCode, result);
		Assert.IsTrue(_cache.ContainsKey(TagService.TagsKey));
	}

	[TestMethod]
	public async Task Add_ConcurrencyError_DoesNotFlushCache()
	{
		const string code = "Test";
		_db.Tags.Add(new Tag { Code = code });
		_cache.Set(TagService.TagsKey, new object());
		await _db.SaveChangesAsync();
		_db.CreateConcurrentUpdateConflict();

		var (_, result) = await _tagService.Add(code, "DisplayName");
		Assert.AreEqual(TagEditResult.Fail, result);
		Assert.IsTrue(_cache.ContainsKey(TagService.TagsKey));
	}

	[TestMethod]
	public async Task Edit_Success_FlushesCaches()
	{
		const int id = 1;
		const string newCode = "Test";
		const string newDisplayName = "Display";
		_db.Tags.Add(new Tag { Id = id });
		await _db.SaveChangesAsync();

		var result = await _tagService.Edit(id, newCode, newDisplayName);

		Assert.AreEqual(TagEditResult.Success, result);
		Assert.AreEqual(1, _db.Tags.Count());
		var tag = _db.Tags.Single();
		Assert.AreEqual(newCode, tag.Code);
		Assert.AreEqual(newDisplayName, tag.DisplayName);
		Assert.IsFalse(_cache.ContainsKey(TagService.TagsKey));
	}

	[TestMethod]
	public async Task Edit_NotFound_DoesNotFlushCache()
	{
		var id = 1;
		_db.Tags.Add(new Tag { Id = id });
		await _db.SaveChangesAsync();
		_cache.Set(TagService.TagsKey, new object());

		var result = await _tagService.Edit(id + 1, "Test", "Test");

		Assert.AreEqual(TagEditResult.NotFound, result);
		Assert.IsTrue(_cache.ContainsKey(TagService.TagsKey));
	}

	[TestMethod]
	public async Task Edit_DuplicateError_DoesNotFlushCache()
	{
		const int id = 1;
		const string code = "Test";
		_db.Tags.Add(new Tag { Id = id, Code = code });
		_cache.Set(TagService.TagsKey, new object());
		await _db.SaveChangesAsync();
		_db.CreateUpdateConflict();

		var result = await _tagService.Edit(id, code, "DisplayName");
		Assert.AreEqual(TagEditResult.DuplicateCode, result);
		Assert.IsTrue(_cache.ContainsKey(TagService.TagsKey));
	}

	[TestMethod]
	public async Task Edit_ConcurrencyError_DoesNotFlushCache()
	{
		const int id = 1;
		const string code = "Test";
		_db.Tags.Add(new Tag { Id = id, Code = code });
		_cache.Set(TagService.TagsKey, new object());
		await _db.SaveChangesAsync();
		_db.CreateConcurrentUpdateConflict();

		var result = await _tagService.Edit(id, code, "DisplayName");
		Assert.AreEqual(TagEditResult.Fail, result);
		Assert.IsTrue(_cache.ContainsKey(TagService.TagsKey));
	}

	[TestMethod]
	public async Task Delete_Success_FlushesCache()
	{
		const int id = 1;
		var tag = new Tag { Id = id };
		_db.Tags.Add(tag);
		await _db.SaveChangesAsync();

		var result = await _tagService.Delete(id);
		Assert.AreEqual(TagDeleteResult.Success, result);
		Assert.IsFalse(_cache.ContainsKey(TagService.TagsKey));
	}

	[TestMethod]
	public async Task Delete_NotFound_DoesNotFlushCache()
	{
		_cache.Set(TagService.TagsKey, new object());

		var result = await _tagService.Delete(-1);
		Assert.AreEqual(TagDeleteResult.NotFound, result);
		Assert.IsTrue(_cache.ContainsKey(TagService.TagsKey));
	}

	[TestMethod]
	public async Task Delete_InUse_FlushesNotCache()
	{
		const int tagId = 1;
		const int publicationId = 1;
		var tag = new Tag { Id = tagId };
		_db.Tags.Add(tag);
		_cache.Set(TagService.TagsKey, new object());
		_db.Publications.Add(new Publication { Id = publicationId });
		_db.PublicationTags.Add(new PublicationTag { PublicationId = publicationId, TagId = tagId });
		await _db.SaveChangesAsync();

		var result = await _tagService.Delete(tagId);
		Assert.AreEqual(TagDeleteResult.InUse, result);
		Assert.IsTrue(_cache.ContainsKey(TagService.TagsKey));
	}

	[TestMethod]
	public async Task Delete_ConcurrencyError_FlushesNotCache()
	{
		const int id = 1;
		_db.Tags.Add(new Tag { Id = 1 });
		await _db.SaveChangesAsync();
		_cache.Set(TagService.TagsKey, new object());
		_db.CreateConcurrentUpdateConflict();

		var result = await _tagService.Delete(id);
		Assert.AreEqual(TagDeleteResult.Fail, result);
		Assert.IsTrue(_cache.ContainsKey(TagService.TagsKey));
	}
}
