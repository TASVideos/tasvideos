using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity;

namespace TASVideos.Core.Services
{
	public enum TierEditResult { Success, Fail, NotFound, DuplicateName }
	public enum TierDeleteResult { Success, Fail, NotFound, InUse }

	public interface ITierService
	{
		ValueTask<ICollection<Tier>> GetAll();
		ValueTask<Tier?> GetById(int id);
		Task<bool> InUse(int id);
		Task<(int? id, TierEditResult)> Add(Tier tier);
		Task<TierEditResult> Edit(int id, Tier tier);
		Task<TierDeleteResult> Delete(int id);
	}

	internal class TierService : ITierService
	{
		internal const string TiersKey = "AllTiers";
		private readonly ApplicationDbContext _db;
		private readonly ICacheService _cache;

		public TierService(ApplicationDbContext db, ICacheService cache)
		{
			_db = db;
			_cache = cache;
		}

		public async ValueTask<ICollection<Tier>> GetAll()
		{
			if (_cache.TryGetValue(TiersKey, out List<Tier> tiers))
			{
				return tiers;
			}

			tiers = await _db.Tiers.ToListAsync();
			_cache.Set(TiersKey, tiers);
			return tiers;
		}

		public async ValueTask<Tier?> GetById(int id)
		{
			var tiers = await GetAll();
			return tiers.SingleOrDefault(t => t.Id == id);
		}

		public async Task<bool> InUse(int id)
		{
			return await _db.Publications.AnyAsync(pt => pt.TierId == id);
		}

		public async Task<(int? id, TierEditResult)> Add(Tier tier)
		{
			var newId = (await _db.Tiers.Select(f => f.Id).MaxAsync()) + 1;
			var entry = _db.Tiers.Add(new Tier
			{
				Id = newId,
				Name = tier.Name,
				IconPath = tier.IconPath,
				Link = tier.Link,
				Weight = tier.Weight
			});

			try
			{
				await _db.SaveChangesAsync();
				_cache.Remove(TiersKey);
				return (entry.Entity.Id, TierEditResult.Success);
			}
			catch (DbUpdateConcurrencyException)
			{
				return (null, TierEditResult.Fail);
			}
			catch (DbUpdateException ex)
			{
				if (ex.InnerException?.Message.Contains("unique constraint") ?? false)
				{
					return (null, TierEditResult.DuplicateName);
				}

				return (null, TierEditResult.Fail);
			}
		}

		public async Task<TierEditResult> Edit(int id, Tier tier)
		{
			var existingTier = await _db.Tiers.SingleOrDefaultAsync(t => t.Id == id);
			if (existingTier == null)
			{
				return TierEditResult.NotFound;
			}

			existingTier.Name = tier.Name;
			existingTier.Link = tier.Link;
			existingTier.IconPath = tier.IconPath;
			existingTier.Weight = tier.Weight;

			try
			{
				await _db.SaveChangesAsync();
				_cache.Remove(TiersKey);
				return TierEditResult.Success;
			}
			catch (DbUpdateConcurrencyException)
			{
				return TierEditResult.Fail;
			}
			catch (DbUpdateException ex)
			{
				if (ex.InnerException?.Message.Contains("unique constraint") ?? false)
				{
					return TierEditResult.DuplicateName;
				}

				return TierEditResult.Fail;
			}
		}

		public async Task<TierDeleteResult> Delete(int id)
		{
			if (await InUse(id))
			{
				return TierDeleteResult.InUse;
			}

			try
			{
				var tier = await _db.Tiers.SingleOrDefaultAsync(t => t.Id == id);
				if (tier == null)
				{
					return TierDeleteResult.NotFound;
				}

				_db.Tiers.Remove(tier);
				await _db.SaveChangesAsync();
				_cache.Remove(TiersKey);
			}
			catch (DbUpdateConcurrencyException)
			{
				return TierDeleteResult.Fail;
			}

			return TierDeleteResult.Success;
		}
	}
}
