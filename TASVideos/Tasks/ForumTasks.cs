using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity.Forum;
using TASVideos.Models;

namespace TASVideos.Tasks
{
	public class ForumTasks
	{
		private readonly ApplicationDbContext _db;

		public ForumTasks(ApplicationDbContext db)
		{
			_db = db;
		}

		/// <summary>
		/// Returns data necessary for the Forum/Index page
		/// </summary>
		public async Task<ForumIndexModel> GetForumIndex()
		{
			return new ForumIndexModel
			{
				Categories = await _db.ForumCategories
					.Include(c => c.Forums)
					.ToListAsync()
			};
		}

		/// <summary>
		/// Returns a forum and topics for the given <see cref="id" />
		/// For the purpose of display
		/// </summary>
		public async Task<Forum> GetForumForDisplay(int id)
		{
			return await _db.Forums
				.Include(f => f.ForumTopics)
				.SingleOrDefaultAsync(f => f.Id == id);
		}
	}
}
