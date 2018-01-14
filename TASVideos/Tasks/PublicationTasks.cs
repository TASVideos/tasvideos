using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

namespace TASVideos.Tasks
{
	using TASVideos.Data;
	using TASVideos.Models;

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
					Title = p.Title
				})
				.SingleOrDefaultAsync(p => p.Id == id);
		}
	}
}
