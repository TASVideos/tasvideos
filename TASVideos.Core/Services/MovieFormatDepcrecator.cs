using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity;
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

public class MovieFormatDeprecator : IMovieFormatDeprecator
{
	private readonly ApplicationDbContext _db;
	private readonly IMovieParser _parser;

	public MovieFormatDeprecator(ApplicationDbContext db, IMovieParser parser)
	{
		_db = db;
		_parser = parser;
	}

	public async Task<IReadOnlyDictionary<string, DeprecatedMovieFormat?>> GetAll()
	{
		var deprecatedFormats = await _db.DeprecatedMovieFormats.ToListAsync();
		var supportedMovieExtensions = _parser.SupportedMovieExtensions.ToList();

		return (from ext in supportedMovieExtensions
				join d in deprecatedFormats on ext equals d.FileExtension into dd
				from d in dd.DefaultIfEmpty()
				orderby ext
				select new { ext, d })
			.ToDictionary(tkey => tkey.ext, tvalue => (DeprecatedMovieFormat?)tvalue.d);
	}

	public bool IsMovieExtension(string extension)
	{
		return _parser.SupportedMovieExtensions.Any(s => s == extension);
	}

	public async Task<bool> IsDeprecated(string extension)
	{
		var entry = await _db.DeprecatedMovieFormats.SingleOrDefaultAsync(d => d.FileExtension == extension);
		return entry?.Deprecated ?? false;
	}

	public async Task<bool> Deprecate(string extension)
	{
		if (!IsMovieExtension(extension))
		{
			return false;
		}

		var format = await _db.DeprecatedMovieFormats
			.SingleOrDefaultAsync(f => f.FileExtension == extension);

		if (format is null)
		{
			_db.DeprecatedMovieFormats.Add(new DeprecatedMovieFormat
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
			await _db.SaveChangesAsync();
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

		var format = await _db.DeprecatedMovieFormats
			.SingleOrDefaultAsync(f => f.FileExtension == extension);

		// If record does not exist, no work is needed to allow it
		if (format != null)
		{
			format.Deprecated = false;
			try
			{
				await _db.SaveChangesAsync();
				return true;
			}
			catch (DbUpdateException)
			{
				return false;
			}
		}

		return true;
	}
}
