using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity;

namespace TASVideos.Core.Services;

public enum ClassEditResult { Success, Fail, NotFound, DuplicateName }
public enum ClassDeleteResult { Success, Fail, NotFound, InUse }

public interface IClassService
{
	ValueTask<ICollection<PublicationClass>> GetAll();
	ValueTask<PublicationClass?> GetById(int id);
	Task<bool> InUse(int id);
	Task<(int? id, ClassEditResult)> Add(PublicationClass publicationClass);
	Task<ClassEditResult> Edit(int id, PublicationClass publicationClass);
	Task<ClassDeleteResult> Delete(int id);
}

internal class ClassService : IClassService
{
	internal const string ClassesKey = "AllPublicationClasses";
	private readonly ApplicationDbContext _db;
	private readonly ICacheService _cache;

	public ClassService(ApplicationDbContext db, ICacheService cache)
	{
		_db = db;
		_cache = cache;
	}

	public async ValueTask<ICollection<PublicationClass>> GetAll()
	{
		if (_cache.TryGetValue(ClassesKey, out List<PublicationClass> classes))
		{
			return classes;
		}

		classes = await _db.PublicationClasses.ToListAsync();
		_cache.Set(ClassesKey, classes);
		return classes;
	}

	public async ValueTask<PublicationClass?> GetById(int id)
	{
		var classes = await GetAll();
		return classes.SingleOrDefault(t => t.Id == id);
	}

	public async Task<bool> InUse(int id)
	{
		return await _db.Publications.AnyAsync(pt => pt.PublicationClassId == id);
	}

	public async Task<(int? id, ClassEditResult)> Add(PublicationClass publicationClass)
	{
		var newId = (await _db.PublicationClasses.Select(f => f.Id).MaxAsync()) + 1;
		var entry = _db.PublicationClasses.Add(new PublicationClass
		{
			Id = newId,
			Name = publicationClass.Name,
			IconPath = publicationClass.IconPath,
			Link = publicationClass.Link,
			Weight = publicationClass.Weight
		});

		try
		{
			await _db.SaveChangesAsync();
			_cache.Remove(ClassesKey);
			return (entry.Entity.Id, ClassEditResult.Success);
		}
		catch (DbUpdateConcurrencyException)
		{
			return (null, ClassEditResult.Fail);
		}
		catch (DbUpdateException ex)
		{
			if (ex.InnerException?.Message.Contains("unique constraint") ?? false)
			{
				return (null, ClassEditResult.DuplicateName);
			}

			return (null, ClassEditResult.Fail);
		}
	}

	public async Task<ClassEditResult> Edit(int id, PublicationClass publicationClass)
	{
		var existingClass = await _db.PublicationClasses.SingleOrDefaultAsync(t => t.Id == id);
		if (existingClass is null)
		{
			return ClassEditResult.NotFound;
		}

		existingClass.Name = publicationClass.Name;
		existingClass.Link = publicationClass.Link;
		existingClass.IconPath = publicationClass.IconPath;
		existingClass.Weight = publicationClass.Weight;

		try
		{
			await _db.SaveChangesAsync();
			_cache.Remove(ClassesKey);
			return ClassEditResult.Success;
		}
		catch (DbUpdateConcurrencyException)
		{
			return ClassEditResult.Fail;
		}
		catch (DbUpdateException ex)
		{
			if (ex.InnerException?.Message.Contains("unique constraint") ?? false)
			{
				return ClassEditResult.DuplicateName;
			}

			return ClassEditResult.Fail;
		}
	}

	public async Task<ClassDeleteResult> Delete(int id)
	{
		if (await InUse(id))
		{
			return ClassDeleteResult.InUse;
		}

		try
		{
			var existingClass = await _db.PublicationClasses.SingleOrDefaultAsync(t => t.Id == id);
			if (existingClass is null)
			{
				return ClassDeleteResult.NotFound;
			}

			_db.PublicationClasses.Remove(existingClass);
			await _db.SaveChangesAsync();
			_cache.Remove(ClassesKey);
		}
		catch (DbUpdateConcurrencyException)
		{
			return ClassDeleteResult.Fail;
		}

		return ClassDeleteResult.Success;
	}
}
