using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using TASVideos.Data.Entity;
using TASVideos.Models;
using TASVideos.Tasks;

namespace TASVideos.Pages.Ratings
{
	[AllowAnonymous]
	public class IndexModel : BasePageModel
	{
		private readonly RatingsTasks _ratingsTasks;

		public IndexModel(
			RatingsTasks ratings,
			UserTasks userTasks) 
			: base(userTasks)
		{
			_ratingsTasks = ratings;
		}

		[FromRoute]
		public int Id { get; set; }

		public PublicationRatingsModel Publication { get; set; } = new PublicationRatingsModel();

		public IEnumerable<PublicationRatingsModel.RatingEntry> VisibleRatings => UserHas(PermissionTo.SeePrivateRatings)
			? Publication.Ratings
			: Publication.Ratings.Where(r => r.IsPublic);

		public async Task<IActionResult> OnGet()
		{
			Publication = await _ratingsTasks.GetRatingsForPublication(Id);
			if (Publication == null)
			{
				return NotFound();
			}

			return Page();
		}
	}
}
