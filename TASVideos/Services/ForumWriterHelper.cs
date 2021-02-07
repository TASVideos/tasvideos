using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.ForumEngine;

namespace TASVideos.Services
{
	public class ForumWriterHelper : IWriterHelper
	{
		private readonly ApplicationDbContext _db;

		public ForumWriterHelper(ApplicationDbContext db)
		{
			_db = db;
		}

		public async Task<string?> GetMovieTitle(int id) => (await _db.Publications.FirstOrDefaultAsync(p => p.Id == id))?.Title;
		public async Task<string?> GetSubmissionTitle(int id) => (await _db.Submissions.FirstOrDefaultAsync(s => s.Id == id))?.Title;
	}
}
