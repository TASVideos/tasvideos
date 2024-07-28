using TASVideos.MovieParsers;

namespace TASVideos.Core.Services;

public interface IMovieFormatDeprecator
{
	Task<IReadOnlyDictionary<string, DeprecatedMovieFormat?>> GetAll();
	bool IsMovieExtension(string extension);
	Task<bool> IsDeprecated(string extension);
	Task<bool> Deprecate(string extension);
	Task<bool> Allow(string extension);
}

public class MovieFormatDeprecator(ApplicationDbContext db, IMovieParser parser) : IMovieFormatDeprecator
{
	public async Task<IReadOnlyDictionary<string, DeprecatedMovieFormat?>> GetAll()
	{
		var deprecatedFormats = await db.DeprecatedMovieFormats.ToListAsync();
		var supportedMovieExtensions = parser.SupportedMovieExtensions.ToList();

		return (from ext in supportedMovieExtensions
				join d in deprecatedFormats on ext equals d.FileExtension into dd
				from d in dd.DefaultIfEmpty()
				orderby ext
				select new { ext, d })
			.ToDictionary(tkey => tkey.ext, tvalue => (DeprecatedMovieFormat?)tvalue.d);
	}

	public bool IsMovieExtension(string extension) => parser.SupportedMovieExtensions.Any(s => s == extension);

	public async Task<bool> IsDeprecated(string extension)
		=> await db.DeprecatedMovieFormats.AnyAsync(d => d.FileExtension == extension && d.Deprecated);

	public async Task<bool> Deprecate(string extension)
	{
		if (!IsMovieExtension(extension))
		{
			return false;
		}

		var format = await db.DeprecatedMovieFormats
			.SingleOrDefaultAsync(f => f.FileExtension == extension);

		if (format is null)
		{
			db.DeprecatedMovieFormats.Add(new DeprecatedMovieFormat
			{
				FileExtension = extension,
				Deprecated = true
			});
		}
		else
		{
			format.Deprecated = true;
		}

		try
		{
			await db.SaveChangesAsync();
			return true;
		}
		catch (DbUpdateException)
		{
			return false;
		}
	}

	public async Task<bool> Allow(string extension)
	{
		if (!IsMovieExtension(extension))
		{
			return false;
		}

		var format = await db.DeprecatedMovieFormats
			.SingleOrDefaultAsync(f => f.FileExtension == extension);

		// If record does not exist, no work is needed to allow it
		if (format is null)
		{
			return true;
		}

		format.Deprecated = false;
		try
		{
			await db.SaveChangesAsync();
			return true;
		}
		catch (DbUpdateException)
		{
			return false;
		}
	}
}
