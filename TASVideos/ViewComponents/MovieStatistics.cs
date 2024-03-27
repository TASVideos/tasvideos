using TASVideos.WikiEngine;
using static TASVideos.ViewComponents.MovieStatisticsModel;

namespace TASVideos.ViewComponents;

[WikiModule(WikiModules.MovieStatistics)]
public class MovieStatistics(ApplicationDbContext db) : ViewComponent
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

		IQueryable<Publication> query = db.Publications.ThatAreCurrent();
		IQueryable<MovieStatisticsEntry> statQuery;

		switch (comparisonMetric)
		{
			case MovieStatisticComparison.None:
				var generalModel = new MovieGeneralStatisticsModel
				{
					PublishedMovieCount = await db.Publications.ThatAreCurrent().CountAsync(),
					TotalMovieCount = await db.Publications.CountAsync(),
					SubmissionCount = await db.Submissions.CountAsync(),
					AverageRerecordCount = (int)await db.Publications.AverageAsync(p => p.RerecordCount),
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
				statQuery = query
					.Where(p => p.System != null && p.SystemFrameRate != null)
					.OrderBy(p => p.Frames / p.SystemFrameRate!.FrameRate, reverse)
					.Select(p => new MovieStatisticsEntry
					{
						Id = p.Id,
						Title = p.Title,

						// the hackiest of workarounds but just calling Time() makes it explode for hardly fathomable reasons
						Value = TimeSpan.FromMilliseconds(Math.Round(p.Frames / p.SystemFrameRate!.FrameRate * 100, MidpointRounding.AwayFromZero) * 10)
					});
				break;

			case MovieStatisticComparison.Rerecords:
				fieldHeader = "Rerecords";
				statQuery = query
					.Where(p => p.RerecordCount > 0)
					.OrderBy(p => p.RerecordCount, reverse)
					.Select(p => new MovieStatisticsEntry
					{
						Id = p.Id,
						Title = p.Title,
						Value = p.RerecordCount
					});
				break;
			case MovieStatisticComparison.RerecordsPerLength:
				fieldHeader = "Rerecords per frame";
				statQuery = query
					.Where(p => p.RerecordCount > 0)
					.OrderBy(p => (double)p.RerecordCount / p.Frames, reverse)
					.Select(p => new MovieStatisticsEntry
					{
						Id = p.Id,
						Title = p.Title,

						// this should use time instead of frames, but time is a massive pain to properly fetch currently
						Value = (double)p.RerecordCount / p.Frames
					});
				break;
			case MovieStatisticComparison.DaysPublished:
				fieldHeader = "Days";
				statQuery = query
					.OrderBy(p => (DateTime.UtcNow - p.CreateTimestamp).TotalDays, reverse)
					.Select(p => new MovieStatisticsEntry
					{
						Id = p.Id,
						Title = p.Title,
						Value = (int)Math.Round((DateTime.UtcNow - p.CreateTimestamp).TotalDays)
					});
				break;
			case MovieStatisticComparison.DescriptionLength:
				fieldHeader = "Characters";
				statQuery = query
					.Join(
						db.WikiPages.ThatAreNotDeleted().ThatAreCurrent(),
						p => LinkConstants.PublicationWikiPage + p.Id,
						wp => wp.PageName,
						(p, wp) => new { p, wp })
					.OrderBy(join => join.wp.Markup.Length, reverse)
					.Select(join => new MovieStatisticsEntry
					{
						Id = join.p.Id,
						Title = join.p.Title,
						Value = join.wp.Markup.Length
					});
				break;
			case MovieStatisticComparison.SubmissionDescriptionLength:
				fieldHeader = "Characters";
				statQuery = query
					.Where(p => p.Submission != null)
					.Join(
						db.WikiPages.ThatAreNotDeleted().ThatAreCurrent(),
						p => LinkConstants.SubmissionWikiPage + p.SubmissionId,
						wp => wp.PageName,
						(p, wp) => new { p, wp })
					.OrderBy(join => join.wp.Markup.Length, reverse)
					.Select(join => new MovieStatisticsEntry
					{
						Id = join.p.Id,
						Title = join.p.Title,
						Value = join.wp.Markup.Length
					});
				break;
			case MovieStatisticComparison.AverageRating:
				fieldHeader = "Rating";
				statQuery = query
					.Where(p => p.PublicationRatings.Count >= minimumVotes)
					.OrderBy(p => p.PublicationRatings.Average(r => r.Value), reverse)
					.Select(p => new MovieStatisticsEntry
					{
						Id = p.Id,
						Title = p.Title,
						Value = Math.Round(p.PublicationRatings.Average(r => r.Value), 2, MidpointRounding.AwayFromZero)
					});
				break;
			case MovieStatisticComparison.VoteCount:
				fieldHeader = "Ratings";
				statQuery = query
					.Where(p => p.CreateTimestamp <= minimumAgeTime)
					.OrderBy(p => p.PublicationRatings.Count, reverse)
					.Select(p => new MovieStatisticsEntry
					{
						Id = p.Id,
						Title = p.Title,
						Value = p.PublicationRatings.Count
					});
				break;
		}

		List<MovieStatisticsEntry> movieList = await statQuery.Take(count).ToListAsync();

		var model = new MovieStatisticsModel
		{
			MovieList = movieList,
			FieldHeader = fieldHeader
		};

		return View("Default", model);
	}
}
