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
		public async Task<ForumModel> GetForumForDisplay(ForumRequest paging)
		{
			var model = await _db.Forums
				.Select(f => new ForumModel
				{
					Id = f.Id,
					Name = f.Name,
					Description = f.Description
				})
				.SingleOrDefaultAsync(f => f.Id == paging.Id);

			if (model == null)
			{
				return null;
			}

			model.Topics = _db.ForumTopics
				.Where(ft => ft.ForumId == paging.Id)
				.Select(ft => new ForumModel.ForumTopicEntry
				{
					Id = ft.Id,
					Title = ft.Title,
					CreateUserName = ft.CreateUserName,
					CreateTimestamp = ft.CreateTimeStamp,
					Type = ft.Type,
					Views = ft.Views
					//PostCount = .ForumPosts.Count TODO: use this when EF core isn't worthless
				})
				.AsQueryable()
				.SortedPageOf(_db, paging);

			// TODO: use above when EF core isn't worthless
			using (_db.Database.BeginTransaction())
			{
				foreach (var topic in model.Topics)
				{
					topic.PostCount = _db.ForumPosts.Count(fp => fp.TopicId == topic.Id);
				}
			}

			return model;
		}
	}
}
