using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents;

[WikiModule(WikiModules.MovieStatistics)]
public class MovieStatistics : ViewComponent
{
	public enum MovieStatisticComparison
	{
		None,

		// Movie data
		Length,
		Rerecords,
		RerecordsPerLength,
		DaysPublished,

		// File data
		EncodeLength,
		EncodeSize,
		EncodeRatio,
		EncodeLengthRatio,
		LongestEnding,

		// Text data
		DescriptionLength,
		SubmissionDescriptionLength,

		// Rating data
		AverageRating,
		VoteCount
	}

	public readonly Dictionary<string, MovieStatisticComparison> ParameterList = new()
	{
		[string.Empty] = MovieStatisticComparison.None,
		["length"] = MovieStatisticComparison.Length,
		["filererecords"] = MovieStatisticComparison.Rerecords,
		["rerecsPerLength"] = MovieStatisticComparison.RerecordsPerLength,
		["daysPublished"] = MovieStatisticComparison.DaysPublished,
		["alength"] = MovieStatisticComparison.EncodeLength,
		["asize"] = MovieStatisticComparison.EncodeSize,
		["encodeRatio"] = MovieStatisticComparison.EncodeRatio,
		["alengthPerLength"] = MovieStatisticComparison.EncodeLengthRatio,
		["alengthMinusLength"] = MovieStatisticComparison.LongestEnding,
		["desclen"] = MovieStatisticComparison.DescriptionLength,
		["udesclen"] = MovieStatisticComparison.SubmissionDescriptionLength,
		["averageRating"] = MovieStatisticComparison.AverageRating,
		["numberOfVotes"] = MovieStatisticComparison.VoteCount
	};

	private readonly ApplicationDbContext _db;

	public MovieStatistics(ApplicationDbContext db)
	{
		_db = db;
	}

	public async Task<IViewComponentResult> InvokeAsync(string? comp, int? minAge, int? minVotes, int? top)
	{
		comp ??= string.Empty;
		int count = top ?? 10;

		// these are only used for rating statistics
		int minimumVotes = minVotes ?? 1;
		DateTime minimumAgeTime = DateTime.UtcNow.AddDays(-(minAge ?? 0));

		bool reverse = comp.StartsWith("-");
		if (reverse)
		{
			comp = comp[1..];
		}

		var comparisonMetric = ParameterList.GetValueOrDefault(comp);
		string fieldHeader;

		List<MovieStatisticsModel.MovieStatisticsEntry> movieList;
		IQueryable<Publication> query = _db.Publications.ThatAreCurrent();
		switch (comparisonMetric)
		{
			case MovieStatisticComparison.None:
				var generalModel = new MovieGeneralStatisticsModel
				{
					PublishedMovieCount = await _db.Publications.ThatAreCurrent().CountAsync(),
					TotalMovieCount = await _db.Publications.CountAsync(),
					SubmissionCount = await _db.Submissions.CountAsync(),
					AverageRerecordCount = (int)await _db.Publications.AverageAsync(p => p.RerecordCount),
				};
				return View("General", generalModel);

			default:
			case MovieStatisticComparison.EncodeLength:
			case MovieStatisticComparison.EncodeSize:
			case MovieStatisticComparison.EncodeRatio:
			case MovieStatisticComparison.EncodeLengthRatio:
			case MovieStatisticComparison.LongestEnding:

				// currently unable to fetch encode data
				return View(
					new MovieStatisticsModel
					{
						ErrorMessage = "Could not display statistics for given parameter: " + comp
					});

			case MovieStatisticComparison.Length:
				fieldHeader = "Length";
				movieList = await query
					.Where(p => p.System != null && p.SystemFrameRate != null)
					.Select(p =>
					(MovieStatisticsModel.MovieStatisticsEntry)new MovieStatisticsModel.MovieStatisticsTimeSpanEntry
					{
						Id = p.Id,
						Title = p.Title,

						// the hackiest of workarounds but just calling Time() makes it explode for hardly fathomable reasons
						TimeSpanValue = TimeSpan.FromMilliseconds(Math.Round(p.Frames / p.SystemFrameRate!.FrameRate * 100, MidpointRounding.AwayFromZero) * 10)
					})
					.ToListAsync();
				break;

			case MovieStatisticComparison.Rerecords:
				fieldHeader = "Rerecords";
				movieList = await query
					.Where(p => p.RerecordCount > 0)
					.Select(p =>
					(MovieStatisticsModel.MovieStatisticsEntry)new MovieStatisticsModel.MovieStatisticsIntEntry
					{
						Id = p.Id,
						Title = p.Title,
						IntValue = p.RerecordCount
					})
					.ToListAsync();
				break;

			case MovieStatisticComparison.RerecordsPerLength:
				fieldHeader = "Rerecords per frame";
				movieList = await query
					.Where(p => p.RerecordCount > 0)
					.Select(p =>
					(MovieStatisticsModel.MovieStatisticsEntry)new MovieStatisticsModel.MovieStatisticsFloatEntry
					{
						Id = p.Id,
						Title = p.Title,

						// this should use time instead of frames, but time is a massive pain to properly fetch currently
						FloatValue = (float)p.RerecordCount / p.Frames
					})
					.ToListAsync();
				break;

			case MovieStatisticComparison.DaysPublished:
				fieldHeader = "Days";
				movieList = await query
					.Select(p =>
					(MovieStatisticsModel.MovieStatisticsEntry)new MovieStatisticsModel.MovieStatisticsIntEntry
					{
						Id = p.Id,
						Title = p.Title,
						IntValue = (int)Math.Round((DateTime.UtcNow - p.CreateTimestamp).TotalDays)
					})
					.ToListAsync();
				break;
			case MovieStatisticComparison.DescriptionLength:
				fieldHeader = "Characters";
				movieList = await query
					.Where(p => p.WikiContent != null)
					.Select(p =>
					(MovieStatisticsModel.MovieStatisticsEntry)new MovieStatisticsModel.MovieStatisticsIntEntry
					{
						Id = p.Id,
						Title = p.Title,
						IntValue = p.WikiContent!.Markup.Length
					})
					.ToListAsync();
				break;

			case MovieStatisticComparison.SubmissionDescriptionLength:
				fieldHeader = "Characters";
				movieList = await query
					.Where(p => p.Submission != null && p.Submission.WikiContent != null)
					.Select(p =>
					(MovieStatisticsModel.MovieStatisticsEntry)new MovieStatisticsModel.MovieStatisticsIntEntry
					{
						Id = p.Id,
						Title = p.Title,
						IntValue = p.Submission!.WikiContent!.Markup.Length
					})
					.ToListAsync();
				break;

			case MovieStatisticComparison.AverageRating:
				fieldHeader = "Rating";
				movieList = await query
					.Where(p => p.PublicationRatings.Count >= minimumVotes)
					.Select(p =>
					(MovieStatisticsModel.MovieStatisticsEntry)new MovieStatisticsModel.MovieStatisticsFloatEntry
					{
						Id = p.Id,
						Title = p.Title,
						FloatValue =
						(float)Math.Round(
							(float)p.PublicationRatings.Average(r => r.Value))
					})
					.ToListAsync();
				break;

			case MovieStatisticComparison.VoteCount:
				fieldHeader = "Ratings";
				movieList = await query
					.Where(p => p.CreateTimestamp <= minimumAgeTime)
					.Select(p =>
					(MovieStatisticsModel.MovieStatisticsEntry)new MovieStatisticsModel.MovieStatisticsFloatEntry
					{
						Id = p.Id,
						Title = p.Title,
						FloatValue = p.PublicationRatings.Count
					})
					.ToListAsync();
				break;
		}

		var orderedList = reverse
						? movieList.OrderByDescending(p => p.Comparable).Take(count).ToList()
						: movieList.OrderBy(p => p.Comparable).Take(count).ToList();

		var model = new MovieStatisticsModel
		{
			MovieList = orderedList,
			FieldHeader = fieldHeader
		};

		return View("Default", model);
	}
}
