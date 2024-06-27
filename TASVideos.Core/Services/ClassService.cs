namespace TASVideos.Core.Services;

public enum ClassEditResult { Success, Fail, NotFound, DuplicateName }
public enum ClassDeleteResult { Success, Fail, NotFound, InUse }

public interface IClassService
{
	ValueTask<IReadOnlyCollection<PublicationClass>> GetAll();
	ValueTask<PublicationClass?> GetById(int id);
	Task<bool> InUse(int id);
	Task<(int? Id, ClassEditResult Result)> Add(PublicationClass publicationClass);
	Task<ClassEditResult> Edit(int id, PublicationClass publicationClass);
	Task<ClassDeleteResult> Delete(int id);
}

internal class ClassService(ApplicationDbContext db, ICacheService cache) : IClassService
{
	internal const string ClassesKey = "AllPublicationClasses";

	public async ValueTask<IReadOnlyCollection<PublicationClass>> GetAll()
	{
		if (cache.TryGetValue(ClassesKey, out List<PublicationClass> classes))
		{
			return classes;
		}

		classes = await db.PublicationClasses.ToListAsync();
		cache.Set(ClassesKey, classes);
		return classes;
	}

	public async ValueTask<PublicationClass?> GetById(int id)
	{
		var classes = await GetAll();
		return classes.SingleOrDefault(t => t.Id == id);
	}

	public async Task<bool> InUse(int id) => await db.Publications.AnyAsync(pt => pt.PublicationClassId == id);

	public async Task<(int? Id, ClassEditResult Result)> Add(PublicationClass publicationClass)
	{
		var newId = (await db.PublicationClasses.Select(f => f.Id).MaxAsync()) + 1;
		var entry = db.PublicationClasses.Add(new PublicationClass
		{
			Id = newId,
			Name = publicationClass.Name,
			IconPath = publicationClass.IconPath,
			Link = publicationClass.Link,
		});

		try
		{
			await db.SaveChangesAsync();
			cache.Remove(ClassesKey);
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
		var existingClass = await db.PublicationClasses.SingleOrDefaultAsync(t => t.Id == id);
		if (existingClass is null)
		{
			return ClassEditResult.NotFound;
		}

		existingClass.Name = publicationClass.Name;
		existingClass.Link = publicationClass.Link;
		existingClass.IconPath = publicationClass.IconPath;

		try
		{
			await db.SaveChangesAsync();
			cache.Remove(ClassesKey);
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
			var existingClass = await db.PublicationClasses.FindAsync(id);
			if (existingClass is null)
			{
				return ClassDeleteResult.NotFound;
			}

			db.PublicationClasses.Remove(existingClass);
			await db.SaveChangesAsync();
			cache.Remove(ClassesKey);
		}
		catch (DbUpdateConcurrencyException)
		{
			return ClassDeleteResult.Fail;
		}

		return ClassDeleteResult.Success;
	}
}
