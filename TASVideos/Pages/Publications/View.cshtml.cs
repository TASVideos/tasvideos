using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Constants;
using TASVideos.Models;
using TASVideos.Services;
using TASVideos.Tasks;

namespace TASVideos.Pages.Publications
{
	[AllowAnonymous]
	public class ViewModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;
		private readonly IPointsService _pointsCalculator;

		public ViewModel(
			ApplicationDbContext db,
			IPointsService pointsCalculator,
			UserTasks userTasks) 
			: base(userTasks)
		{
			_db = db;
			_pointsCalculator = pointsCalculator;
		}

		[FromRoute]
		public int Id { get; set; }

		public PublicationModel Publication { get; set; }

		public async Task<IActionResult> OnGet()
		{
			// TODO: AutoMapper, movie list is the same logic
			Publication = await _db.Publications
				.Select(p => new PublicationModel
				{
					Id = p.Id,
					CreateTimeStamp = p.CreateTimeStamp,
					LastUpdateTimeStamp = p.LastUpdateTimeStamp,
					LastUpdateUser = p.LastUpdateUserName,
					Title = p.Title,
					OnlineWatchingUrl = p.OnlineWatchingUrl,
					MirrorSiteUrl = p.MirrorSiteUrl,
					ObsoletedBy = p.ObsoletedById,
					MovieFileName = p.MovieFileName,
					SubmissionId = p.SubmissionId,
					TierIconPath = p.Tier.IconPath,
					// ReSharper disable once PossibleLossOfFraction
					RatingCount = p.PublicationRatings.Count / 2,
					Files = p.Files
						.Select(f => new PublicationModel.FileModel
						{
							Path = f.Path,
							Type = f.Type
						})
						.ToList(),
					Tags = p.PublicationTags
						.Select(pt => new PublicationModel.TagModel
						{
							DisplayName = pt.Tag.DisplayName,
							Code = pt.Tag.Code
						})
						.ToList(),
					GenreTags = p.Game.GameGenres
						.Select(gg => new PublicationModel.TagModel
						{
							DisplayName = gg.Genre.DisplayName,
							Code = gg.Genre.DisplayName // TODO
						}),
					Flags = p.PublicationFlags
						.Where(pf => pf.Flag.IconPath != null)
						.Select(pf => new PublicationModel.FlagModel
						{
							IconPath = pf.Flag.IconPath,
							LinkPath = pf.Flag.LinkPath,
							Name = pf.Flag.Name
						})
						.ToList()
				})
				.SingleOrDefaultAsync(p => p.Id == Id);

			if (Publication == null)
			{
				return NotFound();
			}

			var pageName = LinkConstants.SubmissionWikiPage + Publication.SubmissionId;
			Publication.TopicId = (await _db.ForumTopics
					.SingleOrDefaultAsync(t => t.PageName == pageName))
					?.Id ?? 0;

			Publication.OverallRating = (await _pointsCalculator.CalculatePublicationRatings(Id))
				.Overall;

			return Page();
		}

		public async Task<IActionResult> OnGetDownload()
		{
			var pub = await _db.Publications
				.Where(s => s.Id == Id)
				.Select(s => new { s.MovieFile, s.MovieFileName })
				.SingleOrDefaultAsync();

			if (pub == null)
			{
				return NotFound();
			}

			return File(pub.MovieFile, MediaTypeNames.Application.Octet, $"{pub.MovieFileName}.zip");
		}
	}
}
