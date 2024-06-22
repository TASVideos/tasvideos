namespace TASVideos.Core.Services;

public enum SystemEditResult { Success, Fail, NotFound, DuplicateCode, DuplicateId }
public enum SystemDeleteResult { Success, Fail, NotFound, InUse }

public interface IGameSystemService
{
	ValueTask<ICollection<SystemsResponse>> GetAll();
	ValueTask<SystemsResponse?> GetById(int id);
	Task<bool> InUse(int id);
	ValueTask<int> NextId();
	Task<SystemEditResult> Add(int id, string code, string displayName);
	Task<SystemEditResult> Edit(int id, string code, string displayName);
	Task<SystemDeleteResult> Delete(int id);
	Task FlushCache();
}

internal class GameSystemService(ApplicationDbContext db, ICacheService cache) : IGameSystemService
{
	internal const string SystemsKey = "AllSystems";

	public async ValueTask<ICollection<SystemsResponse>> GetAll()
	{
		if (cache.TryGetValue(SystemsKey, out List<SystemsResponse> systems))
		{
			return systems;
		}

		systems = await db.GameSystems
			.Select(s => new SystemsResponse(
				s.Id,
				s.Code,
				s.DisplayName,
				s.SystemFrameRates.Select(sf => new FrameRatesResponse(
					sf.Id,
					sf.FrameRate,
					sf.RegionCode,
					sf.Preliminary,
					sf.Obsolete))))
			.ToListAsync();
		cache.Set(SystemsKey, systems);
		return systems;
	}

	public async ValueTask<SystemsResponse?> GetById(int id)
	{
		var systems = await GetAll();
		return systems.SingleOrDefault(s => s.Id == id);
	}

	public async Task<bool> InUse(int id)
	{
		if (await db.GameVersions.AnyAsync(r => r.SystemId == id))
		{
			return true;
		}

		if (await db.Publications.AnyAsync(p => p.SystemId == id))
		{
			return true;
		}

		if (await db.Submissions.AnyAsync(s => s.SystemId == id))
		{
			return true;
		}

		if (await db.UserFiles.AnyAsync(uf => uf.SystemId == id))
		{
			return true;
		}

		return false;
	}

	public async ValueTask<int> NextId()
	{
		var systems = await GetAll();
		if (systems.Any())
		{
			return systems.Max(s => s.Id) + 1;
		}

		return 0;
	}

	public async Task<SystemEditResult> Add(int id, string code, string displayName)
	{
		var system = await db.GameSystems.SingleOrDefaultAsync(s => s.Id == id);
		if (system is not null)
		{
			return SystemEditResult.DuplicateId;
		}

		db.GameSystems.Add(new GameSystem
		{
			Id = id,
			Code = code,
			DisplayName = displayName
		});

		try
		{
			await db.SaveChangesAsync();
			cache.Remove(SystemsKey);
			return SystemEditResult.Success;
		}
		catch (DbUpdateConcurrencyException)
		{
			return SystemEditResult.Fail;
		}
		catch (DbUpdateException ex)
		{
			if (ex.InnerException?.Message.Contains("unique constraint") ?? false)
			{
				return SystemEditResult.DuplicateCode;
			}

			return SystemEditResult.Fail;
		}
	}

	public async Task<SystemEditResult> Edit(int id, string code, string displayName)
	{
		var system = await db.GameSystems.SingleOrDefaultAsync(s => s.Id == id);
		if (system is null)
		{
			return SystemEditResult.NotFound;
		}

		system.Code = code;
		system.DisplayName = displayName;

		try
		{
			await db.SaveChangesAsync();
			cache.Remove(SystemsKey);
			return SystemEditResult.Success;
		}
		catch (DbUpdateConcurrencyException)
		{
			return SystemEditResult.Fail;
		}
		catch (DbUpdateException ex)
		{
			if (ex.InnerException?.Message.Contains("unique constraint") ?? false)
			{
				return SystemEditResult.DuplicateCode;
			}

			return SystemEditResult.Fail;
		}
	}

	public async Task<SystemDeleteResult> Delete(int id)
	{
		if (await InUse(id))
		{
			return SystemDeleteResult.InUse;
		}

		try
		{
			var system = await db.GameSystems.FindAsync(id);
			if (system is null)
			{
				return SystemDeleteResult.NotFound;
			}

			db.GameSystems.Remove(system);
			await db.SaveChangesAsync();
			cache.Remove(SystemsKey);
		}
		catch (DbUpdateConcurrencyException)
		{
			return SystemDeleteResult.Fail;
		}

		return SystemDeleteResult.Success;
	}

	public async Task FlushCache()
	{
		cache.Remove(SystemsKey);
		await GetAll();
	}
}

public record SystemsResponse(int Id, string Code, string DisplayName, IEnumerable<FrameRatesResponse> SystemFrameRates);
public record FrameRatesResponse(int Id, double FrameRate, string RegionCode, bool Preliminary, bool Obsolete);
