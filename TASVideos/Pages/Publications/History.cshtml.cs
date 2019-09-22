using System.Linq;
using System.Threading.Tasks;

using AutoMapper.QueryableExtensions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Pages.Publications.Models;
using TASVideos.Services;
using TASVideos.Services.PublicationChain;

namespace TASVideos.Pages.Publications
{
	[AllowAnonymous]
	public class HistoryModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;
		private readonly IPublicationHistory _history;

		public HistoryModel(
			ApplicationDbContext db,
			IPublicationHistory history)
		{
			_history = history;
			_db = db;
		}

		[FromRoute]
		public int Id { get; set; }

		public string Title { get; set; }
		public string GameName { get; set; }
		public int GameId { get; set; }
		public PublicationHistoryGroup History { get; set; }

		public async Task<IActionResult> OnGet()
		{
			var publication = await _db.Publications
				.Select(p => new // TODO: make a model
				{
					p.Id,
					p.Title,
					p.GameId,
					GameName = p.Game.DisplayName
				})
				.SingleOrDefaultAsync(p => p.Id == Id);

			if (publication == null)
			{
				return NotFound();
			}

			Title = publication.Title;
			GameName = publication.GameName;
			GameId = publication.GameId;
			History = await _history.ForGame(publication.GameId);

			return Page();
		}
	}
}
