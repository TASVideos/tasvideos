using System;
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
		/// Returns a forum and topics for the given id
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
					Views = ft.Views,
					PostCount = ft.ForumPosts.Count,
					LastPost = ft.ForumPosts.Any() ? ft.ForumPosts.Max(fp => fp.CreateTimeStamp) : (DateTime?)null
				})
				.OrderByDescending(ft => ft.Type == ForumTopicType.Sticky)
				.ThenByDescending(ft => ft.Type == ForumTopicType.Announcement)
				.ThenByDescending(ft => ft.LastPost)
				.PageOf(_db, paging);

			return model;
		}

		/// <summary>
		/// Displays a page of posts for the given topic
		/// </summary>
		public async Task<ForumTopicModel> GetTopicForDisplay(TopicRequest paging)
		{
			var model = await _db.ForumTopics
				.Select(t => new ForumTopicModel
				{
					Id = t.Id,
					Title = t.Title
				})
				.SingleOrDefaultAsync(t => t.Id == paging.Id);

			if (model == null)
			{
				return null;
			}

			model.Posts = _db.ForumPosts
				.Where(p => p.TopicId == paging.Id)
				.Select(p => new ForumTopicModel.ForumPostEntry
				{
					Id = p.Id,
					PosterName = p.Poster.UserName,
					PosterAvatar = p.Poster.Avatar,
					PosterLocation = p.Poster.From,
					PosterPostCount = _db.ForumPosts.Count(fp => fp.PosterId == p.PosterId),
					Text = p.Text
				})
				.SortedPageOf(_db, paging);

			return model;
		}
	}
}
