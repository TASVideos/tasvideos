using System.Net.Mime;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Models;
using TASVideos.Tasks;

namespace TASVideos.Pages.Publications
{
	[AllowAnonymous]
	public class ViewModel : BasePageModel
	{
		private readonly PublicationTasks _publicationTasks;
		private readonly RatingsTasks _ratingsTasks;

		public ViewModel(
			PublicationTasks publicationTasks,
			RatingsTasks ratingsTasks,
			UserTasks userTasks) 
			: base(userTasks)
		{
			_publicationTasks = publicationTasks;
			_ratingsTasks = ratingsTasks;
		}

		[FromRoute]
		public int Id { get; set; }

		public PublicationModel Publication { get; set; }

		public async Task<IActionResult> OnGet()
		{
			Publication = await _publicationTasks.GetPublicationForDisplay(Id);
			if (Publication == null)
			{
				return NotFound();
			}

			Publication.OverallRating = await _ratingsTasks.GetOverallRatingForPublication(Id);

			return Page();
		}

		public async Task<IActionResult> OnGetDownload()
		{
			var (fileBytes, fileName) = await _publicationTasks.GetPublicationMovieFile(Id);
			if (fileBytes.Length > 0)
			{
				return File(fileBytes, MediaTypeNames.Application.Octet, $"{fileName}.zip");
			}

			return BadRequest();
		}
	}
}
