using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity;

namespace TASVideos.Core.Services;

public enum FlagEditResult { Success, Fail, NotFound, DuplicateCode }
public enum FlagDeleteResult { Success, Fail, NotFound, InUse }

public interface IFlagService
{
	Task<ICollection<Flag>> GetAll();
	Task<Flag?> GetById(int id);
	Task<ListDiff> GetDiff(IEnumerable<int> currentIds, IEnumerable<int> newIds);
	Task<bool> InUse(int id);
	Task<FlagEditResult> Add(Flag flag);
	Task<FlagEditResult> Edit(int id, Flag flag);
	Task<FlagDeleteResult> Delete(int id);
}

internal class FlagService : IFlagService
{
	internal const string FlagsKey = "AllFlags";
	private readonly ApplicationDbContext _db;
	private readonly ICacheService _cache;

	public FlagService(ApplicationDbContext db, ICacheService cache)
	{
		_db = db;
		_cache = cache;
	}

	public async Task<ICollection<Flag>> GetAll()
	{
		if (_cache.TryGetValue(FlagsKey, out List<Flag> flags))
		{
			return flags;
		}

		flags = await _db.Flags.ToListAsync();
		_cache.Set(FlagsKey, flags);
		return flags;
	}

	public async Task<Flag?> GetById(int id)
	{
		var flags = await GetAll();
		return flags.SingleOrDefault(t => t.Id == id);
	}

	public async Task<ListDiff> GetDiff(IEnumerable<int> currentIds, IEnumerable<int> newIds)
	{
		var flags = await GetAll();

		var currentFlags = flags
			.Where(t => currentIds.Contains(t.Id))
			.Select(t => t.Token)
			.ToList();
		var newFlags = flags
			.Where(t => newIds.Contains(t.Id))
			.Select(t => t.Token)
			.ToList();

		return new ListDiff(currentFlags, newFlags);
	}

	public async Task<bool> InUse(int id)
	{
		return await _db.PublicationFlags.AnyAsync(pt => pt.FlagId == id);
	}

	public async Task<FlagEditResult> Add(Flag flag)
	{
		// No auto-increment, due to legacy importing, need to re-add this
		var newId = (await _db.Flags.Select(f => f.Id).MaxAsync()) + 1;

		_db.Flags.Add(new Flag
		{
			Id = newId,
			Name = flag.Name,
			IconPath = flag.IconPath,
			LinkPath = flag.LinkPath,
			Token = flag.Token,
			PermissionRestriction = flag.PermissionRestriction
		});

		try
		{
			await _db.SaveChangesAsync();
			_cache.Remove(FlagsKey);
			return FlagEditResult.Success;
		}
		catch (DbUpdateConcurrencyException)
		{
			return FlagEditResult.Fail;
		}
		catch (DbUpdateException ex)
		{
			if (ex.InnerException?.Message.Contains("unique constraint") ?? false)
			{
				return FlagEditResult.DuplicateCode;
			}

			return FlagEditResult.Fail;
		}
	}

	public async Task<FlagEditResult> Edit(int id, Flag flag)
	{
		var existingFlag = await _db.Flags.SingleOrDefaultAsync(t => t.Id == id);
		if (existingFlag is null)
		{
			return FlagEditResult.NotFound;
		}

		existingFlag.Name = flag.Name;
		existingFlag.IconPath = flag.IconPath;
		existingFlag.LinkPath = flag.LinkPath;
		existingFlag.Token = flag.Token;
		existingFlag.PermissionRestriction = flag.PermissionRestriction;

		try
		{
			await _db.SaveChangesAsync();
			_cache.Remove(FlagsKey);
			return FlagEditResult.Success;
		}
		catch (DbUpdateConcurrencyException)
		{
			return FlagEditResult.Fail;
		}
		catch (DbUpdateException ex)
		{
			if (ex.InnerException?.Message.Contains("unique constraint") ?? false)
			{
				return FlagEditResult.DuplicateCode;
			}

			return FlagEditResult.Fail;
		}
	}

	public async Task<FlagDeleteResult> Delete(int id)
	{
		if (await InUse(id))
		{
			return FlagDeleteResult.InUse;
		}

		try
		{
			var flag = await _db.Flags.SingleOrDefaultAsync(t => t.Id == id);
			if (flag is null)
			{
				return FlagDeleteResult.NotFound;
			}

			_db.Flags.Remove(flag);
			await _db.SaveChangesAsync();
			_cache.Remove(FlagsKey);
		}
		catch (DbUpdateConcurrencyException)
		{
			return FlagDeleteResult.Fail;
		}

		return FlagDeleteResult.Success;
	}
}
