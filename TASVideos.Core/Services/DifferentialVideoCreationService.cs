using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.MovieParsers;
using TASVideos.MovieParsers.Result;

namespace TASVideos.Core.Services;

public interface IDifferentialVideoCreationService
{
	public Task<IParseResult> InformationAsync(string fileName, Stream stream);
	public Task<Dictionary<TimeSpan, TimeSpan>> DiffAsync(string fileName, Stream stream);
}

public  class DifferentialVideoCreationService : IDifferentialVideoCreationService
{
	private readonly IMovieParser _parser;
	private readonly ApplicationDbContext _db;
	public DifferentialVideoCreationService(IMovieParser parser, 
		ApplicationDbContext db)
	{
		_parser = parser;
		_db = db;
	}

	public async Task<IParseResult> InformationAsync(string fileName, Stream stream)
	{
		var result = await _parser.ParseFile(fileName, stream);
		switch (result.FileExtension)
		{
			case ".fm2":
				//add support for each with inheritence, infinite if for now.
			default:
				break;

		}
		return result;
	}

	/// <summary>
	/// Provides a dictionary of two timestamps. The first being the time in which the Video shows a deviation from the previous submission.
	/// the second being the duration the deviation occures.
	/// </summary>
	/// <param name="fileName"></param>
	/// <param name="stream"></param>
	/// <returns></returns>
	public async Task<Dictionary<TimeSpan, TimeSpan>> DiffAsync(string fileName, Stream stream)
	{
		var timesWeCareAbout = new Dictionary<TimeSpan, TimeSpan>();
		var newRecord = await _parser.ParseFile(fileName, stream);

		var query = await _db.Submissions.Include(s => s.Publication)
								   .Include(s => s.Publication.Files)
								   .OrderByDescending(s => s.CreateTimestamp)
								   .Where(s => s.Publication.Files.Any(f => f.Type == TASVideos.Data.Entity.FileType.MovieFile)
								            && s.Publication.MovieFileName == fileName)
								   .ToListAsync();

		if (query.Count < 2)
		{
			return timesWeCareAbout;
		}

		var previousSubmission = query[0];

		var newestSubmission = query[1];

		using var previousStream = File.Create("wwwroot/previousSubmission.txt");
		previousStream.Write(previousSubmission.MovieFile, 0, previousSubmission.MovieFile.Length);

		var oldRecord = await _parser.ParseFile(previousSubmission.Publication.MovieFileName, previousStream);


		if (oldRecord.FrameRateOverride == newRecord.FrameRateOverride) /* I guess that means we can compare? */
		{
			using var newestStream = File.Create("wwwroot/newestSubmission.txt");
			newestStream.Write(newestSubmission.MovieFile, 0, newestSubmission.MovieFile.Length);

			var newSub = File.ReadAllLines("wwwroot/newestSubmission.txt");
			var oldSub = File.ReadAllLines("wwwroot/oldestSubmission.txt");

			var movieLength = newestSubmission.Frames;

			//this is probably wrong but should be something like "the longer the movie,
			////the more frames of "difference" it takes before we care about it.
			var deviationForChangeWeCareAbout = Convert.ToInt32(movieLength * newestSubmission.SystemFrameRate?.FrameRate ?? 60 * .10);

			List<int> frames = new();

			foreach (var line in newSub)
			{
				if (!oldSub.Contains(line))
				{
					frames.Add(Array.IndexOf(newSub, line));
				}
			}

			frames = frames.OrderBy(f => f)
						   .ToList();

			for (int i= 0; i < frames.Count; i++)
			{
				if (RelevantBatch(frames, i, deviationForChangeWeCareAbout))
				{
					timesWeCareAbout.Add(new TimeSpan(0, 0, Convert.ToInt32((i - deviationForChangeWeCareAbout) * newRecord?.FrameRateOverride ?? 60)),
										 new TimeSpan(0, 0, Convert.ToInt32((i + deviationForChangeWeCareAbout) * newRecord?.FrameRateOverride ?? 60)));
				}
			}

		}
		else
		{
			//???
		}

		return timesWeCareAbout;
	}

	private bool RelevantBatch(List<int> frames, int counter, int deviation)
	{
		var result = false;
		var framesNeededToRenderDiff = Enumerable.Range(counter - deviation, counter + deviation).ToList();
		try
		{
			foreach (var frame in framesNeededToRenderDiff)
			{
				if (frames.Contains(frame))
				{
					result = true;
				}
			}
		}
		catch (Exception ex)
		{
			
		}
		return result;
	}
}
