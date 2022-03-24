using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity;

namespace TASVideos.Core.Services;

public enum TagEditResult { Success, Fail, NotFound, DuplicateCode }
public enum TagDeleteResult { Success, Fail, NotFound, InUse }

public interface ITagService
{
	ValueTask<ICollection<Tag>> GetAll();
	ValueTask<Tag?> GetById(int id);
	ValueTask<ListDiff> GetDiff(IEnumerable<int> currentIds, IEnumerable<int> newIds);
	Task<bool> InUse(int id);
	Task<(int? id, TagEditResult result)> Add(string code, string displayName);
	Task<TagEditResult> Edit(int id, string code, string displayName);
	Task<TagDeleteResult> Delete(int id);
}

internal class TagService : ITagService
{
	internal const string TagsKey = "AllTags";
	private readonly ApplicationDbContext _db;
	private readonly ICacheService _cache;

	public TagService(ApplicationDbContext db, ICacheService cache)
	{
		_db = db;
		_cache = cache;
	}

	public async ValueTask<ICollection<Tag>> GetAll()
	{
		if (_cache.TryGetValue(TagsKey, out List<Tag> tags))
		{
			return tags;
		}

		tags = await _db.Tags.ToListAsync();
		_cache.Set(TagsKey, tags);
		return tags;
	}

	public async ValueTask<Tag?> GetById(int id)
	{
		var tags = await GetAll();
		return tags.SingleOrDefault(t => t.Id == id);
	}

	public async ValueTask<ListDiff> GetDiff(IEnumerable<int> currentIds, IEnumerable<int> newIds)
	{
		var tags = await GetAll();

		var currentTags = tags
			.Where(t => currentIds.Contains(t.Id))
			.Select(t => t.Code)
			.ToList();
		var newTags = tags
			.Where(t => newIds.Contains(t.Id))
			.Select(t => t.Code)
			.ToList();

		return new ListDiff(currentTags, newTags);
	}

	public async Task<bool> InUse(int id)
	{
		return await _db.PublicationTags.AnyAsync(pt => pt.TagId == id);
	}

	public async Task<(int? id, TagEditResult result)> Add(string code, string displayName)
	{
		var entry = _db.Tags.Add(new Tag
		{
			Code = code,
			DisplayName = displayName
		});

		try
		{
			await _db.SaveChangesAsync();
			_cache.Remove(TagsKey);
			return (entry.Entity.Id, TagEditResult.Success);
		}
		catch (DbUpdateConcurrencyException)
		{
			return (null, TagEditResult.Fail);
		}
		catch (DbUpdateException ex)
		{
			if (ex.InnerException?.Message.Contains("unique constraint") ?? false)
			{
				return (null, TagEditResult.DuplicateCode);
			}

			return (null, TagEditResult.Fail);
		}
	}

	public async Task<TagEditResult> Edit(int id, string code, string displayName)
	{
		var tag = await _db.Tags.SingleOrDefaultAsync(t => t.Id == id);
		if (tag is null)
		{
			return TagEditResult.NotFound;
		}

		tag.Code = code;
		tag.DisplayName = displayName;

		try
		{
			await _db.SaveChangesAsync();
			_cache.Remove(TagsKey);
			return TagEditResult.Success;
		}
		catch (DbUpdateConcurrencyException)
		{
			return TagEditResult.Fail;
		}
		catch (DbUpdateException ex)
		{
			if (ex.InnerException?.Message.Contains("unique constraint") ?? false)
			{
				return TagEditResult.DuplicateCode;
			}

			return TagEditResult.Fail;
		}
	}

	public async Task<TagDeleteResult> Delete(int id)
	{
		if (await InUse(id))
		{
			return TagDeleteResult.InUse;
		}

		try
		{
			var tag = await _db.Tags.SingleOrDefaultAsync(t => t.Id == id);
			if (tag is null)
			{
				return TagDeleteResult.NotFound;
			}

			_db.Tags.Remove(tag);
			await _db.SaveChangesAsync();
			_cache.Remove(TagsKey);
		}
		catch (DbUpdateConcurrencyException)
		{
			return TagDeleteResult.Fail;
		}

		return TagDeleteResult.Success;
	}
}
