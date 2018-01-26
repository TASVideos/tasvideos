using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Models;

namespace TASVideos.Tasks
{
	public class PublicationTasks
	{
		private readonly ApplicationDbContext _db;

		public PublicationTasks(ApplicationDbContext db)
		{
			_db = db;
		}

		/// <summary>
		/// Gets a publication with the given <see cref="id" /> for the purpose of display
		/// If no publication with the given id is found then null is returned
		/// </summary>
		public async Task<PublicationViewModel> GetPublicationForDisplay(int id)
		{
			return await _db.Publications
				.Select(p => new PublicationViewModel
				{
					Id = p.Id,
					Title = p.Title,
					Screenshot = p.Files.Single(f => f.Type == FileType.Screenshot).Path
				})
				.SingleOrDefaultAsync(p => p.Id == id);
		}

		/// <summary>
		/// Gets the title of a movie with the given id
		/// If the movie is not found, null is returned
		/// </summary>
		public async Task<string> GetTitle(int id)
		{
			return (await _db.Publications.SingleOrDefaultAsync(s => s.Id == id))?.Title;
		}
	}
}
