using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using TASVideos.Models;
using TASVideos.Tasks;

namespace TASVideos.Pages.Publications
{
	[AllowAnonymous]
	public class IndexModel : BasePageModel
	{
		private readonly PublicationTasks _publicationTasks;
		private readonly RatingsTasks _ratingsTasks;

		public IndexModel(
			PublicationTasks publicationTasks,
			RatingsTasks ratingsTasks,
			UserTasks userTasks) 
			: base(userTasks)
		{
			_publicationTasks = publicationTasks;
			_ratingsTasks = ratingsTasks;
		}

		[FromRoute]
		public string Query { get; set; }

		public IEnumerable<PublicationModel> Movies { get; set; } = new List<PublicationModel>();

		public async Task<IActionResult> OnGet()
		{
			var tokenLookup = await _publicationTasks.GetMovieTokenData();

			var tokens = Query
				.Split(new[] { "-" }, StringSplitOptions.RemoveEmptyEntries)
				.Select(s => s.Trim(' '))
				.Select(s => s.ToLower())
				.ToList();

			var searchModel = new PublicationSearchModel
			{
				Tiers = tokenLookup.Tiers.Where(t => tokens.Contains(t)),
				SystemCodes = tokenLookup.SystemCodes.Where(s => tokens.Contains(s)),
				ShowObsoleted = tokens.Contains("obs"),
				Years = tokenLookup.Years.Where(y => tokens.Contains("y" + y)),
				Tags = tokenLookup.Tags.Where(t => tokens.Contains(t)),
				Genres = tokenLookup.Genres.Where(g => tokens.Contains(g)),
				Flags = tokenLookup.Flags.Where(f => tokens.Contains(f)),
				MovieIds = tokens
					.Where(t => t.EndsWith('m'))
					.Where(t => int.TryParse(t.Substring(0, t.Length - 1), out int unused))
					.Select(t => int.Parse(t.Substring(0, t.Length - 1)))
					.ToList(),
				Authors = tokens
					.Where(t => t.ToLower().Contains("author"))
					.Select(t => t.ToLower().Replace("author", ""))
					.Select(t => int.TryParse(t, out var temp) ? temp : (int?)null)
					.Where(t => t.HasValue)
					.Select(t => t.Value)
					.ToList()
			};

			// If no valid filter criteria, don't attempt to generate a list (else it would be all movies for what is most likely a malformed URL)
			if (searchModel.IsEmpty)
			{
				return Redirect("Movies");
			}

			Movies = (await _publicationTasks
				.GetMovieList(searchModel))
				.ToList();

			var ratings = await _ratingsTasks.GetOverallRatingsForPublications(Movies.Select(m => m.Id));

			foreach (var rating in ratings)
			{
				Movies.First(m => m.Id == rating.Key).OverallRating = rating.Value;
			}

			return Page();
		}
	}
}
