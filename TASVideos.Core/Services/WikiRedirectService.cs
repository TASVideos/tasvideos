namespace TASVideos.Core.Services;

public enum WikiRedirectAddEditResult { Success, Fail, DuplicateSource, NotFound, ChainedRedirectFrom, ChainedRedirectTo, SamePageName, InvalidPageNameFrom, InvalidPageNameTo }
public enum WikiRedirectDeleteResult { Success, Fail, NotFound }

public interface IWikiRedirectService
{
	Task<ICollection<WikiRedirect>> GetAll();
	Task<WikiRedirect?> GetById(int id);
	Task<WikiRedirectAddEditResult> Add(WikiRedirect redirect);
	Task<WikiRedirectAddEditResult> Edit(int id, WikiRedirect redirect);
	Task<WikiRedirectDeleteResult> Delete(int id);
}

internal class WikiRedirectService(ApplicationDbContext db) : IWikiRedirectService
{
	public async Task<ICollection<WikiRedirect>> GetAll()
	{
		return await db.WikiRedirects
			.OrderBy(r => r.PageNameFrom)
			.ThenBy(r => r.PageNameTo)
			.ToListAsync();
	}

	public async Task<WikiRedirect?> GetById(int id) => await db.WikiRedirects.FindAsync(id);

	public async Task<WikiRedirectAddEditResult> Add(WikiRedirect redirect)
	{
		if (!WikiHelper.IsValidWikiPageName(redirect.PageNameFrom))
		{
			return WikiRedirectAddEditResult.InvalidPageNameFrom;
		}

		if (!WikiHelper.IsValidWikiPageName(redirect.PageNameTo))
		{
			return WikiRedirectAddEditResult.InvalidPageNameTo;
		}

		if (redirect.PageNameFrom == redirect.PageNameTo)
		{
			return WikiRedirectAddEditResult.SamePageName;
		}

		if (await db.WikiRedirects.AnyAsync(r => r.PageNameFrom == redirect.PageNameTo))
		{
			return WikiRedirectAddEditResult.ChainedRedirectTo;
		}

		if (await db.WikiRedirects.AnyAsync(r => r.PageNameTo == redirect.PageNameFrom))
		{
			return WikiRedirectAddEditResult.ChainedRedirectFrom;
		}

		db.WikiRedirects.Add(new WikiRedirect
		{
			PageNameFrom = redirect.PageNameFrom,
			PageNameTo = redirect.PageNameTo
		});

		try
		{
			await db.SaveChangesAsync();
			return WikiRedirectAddEditResult.Success;
		}
		catch (DbUpdateConcurrencyException)
		{
			return WikiRedirectAddEditResult.Fail;
		}
		catch (DbUpdateException ex)
		{
			if (ex.InnerException?.Message.Contains("unique constraint") ?? false)
			{
				return WikiRedirectAddEditResult.DuplicateSource;
			}

			return WikiRedirectAddEditResult.Fail;
		}
	}

	public async Task<WikiRedirectAddEditResult> Edit(int id, WikiRedirect redirect)
	{
		var existing = await db.WikiRedirects.FindAsync(id);
		if (existing is null)
		{
			return WikiRedirectAddEditResult.NotFound;
		}

		if (!WikiHelper.IsValidWikiPageName(redirect.PageNameFrom))
		{
			return WikiRedirectAddEditResult.InvalidPageNameFrom;
		}

		if (!WikiHelper.IsValidWikiPageName(redirect.PageNameTo))
		{
			return WikiRedirectAddEditResult.InvalidPageNameTo;
		}

		if (redirect.PageNameFrom == redirect.PageNameTo)
		{
			return WikiRedirectAddEditResult.SamePageName;
		}

		if (await db.WikiRedirects.AnyAsync(r => r.Id != id && r.PageNameFrom == redirect.PageNameTo))
		{
			return WikiRedirectAddEditResult.ChainedRedirectTo;
		}

		if (await db.WikiRedirects.AnyAsync(r => r.Id != id && r.PageNameTo == redirect.PageNameFrom))
		{
			return WikiRedirectAddEditResult.ChainedRedirectFrom;
		}

		existing.PageNameFrom = redirect.PageNameFrom;
		existing.PageNameTo = redirect.PageNameTo;

		try
		{
			await db.SaveChangesAsync();
			return WikiRedirectAddEditResult.Success;
		}
		catch (DbUpdateConcurrencyException)
		{
			return WikiRedirectAddEditResult.Fail;
		}
		catch (DbUpdateException ex)
		{
			if (ex.InnerException?.Message.Contains("unique constraint") ?? false)
			{
				return WikiRedirectAddEditResult.DuplicateSource;
			}

			return WikiRedirectAddEditResult.Fail;
		}
	}

	public async Task<WikiRedirectDeleteResult> Delete(int id)
	{
		var redirect = await db.WikiRedirects.FindAsync(id);
		if (redirect is null)
		{
			return WikiRedirectDeleteResult.NotFound;
		}

		db.WikiRedirects.Remove(redirect);

		try
		{
			await db.SaveChangesAsync();
			return WikiRedirectDeleteResult.Success;
		}
		catch (DbUpdateConcurrencyException)
		{
			return WikiRedirectDeleteResult.Fail;
		}
	}
}
