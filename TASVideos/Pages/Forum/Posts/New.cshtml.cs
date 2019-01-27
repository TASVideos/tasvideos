using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;
using TASVideos.Models;
using TASVideos.Tasks;

namespace TASVideos.Pages.Forum.Posts
{
	[Authorize]
	public class NewModel : BasePageModel
	{
		private readonly UserManager<User> _userManager;
		private readonly ApplicationDbContext _db;
		
		public NewModel(
			UserManager<User> userManager,
			ApplicationDbContext db,
			UserTasks userTasks)
		: base(userTasks)
		{
			_userManager = userManager;
			_db = db;
		}

		[FromQuery]
		public PagedModel Search { get; set; }

		public PageOf<PostsSinceLastVisitModel> Posts { get; set; }

		public async Task OnGet()
		{
			var user = await _userManager.GetUserAsync(User);
			var allowRestricted = UserHas(PermissionTo.SeeRestrictedForums);
			var since = user.LastLoggedInTimeStamp ?? DateTime.UtcNow;
			Posts = await _db.ForumPosts
				.ExcludeRestricted(allowRestricted)
				.Since(since)
				.Select(p => new PostsSinceLastVisitModel
				{
					Id = p.Id,
					CreateTimestamp = p.CreateTimeStamp,
					EnableBbCode = p.EnableBbCode,
					EnableHtml = p.EnableHtml,
					Text = p.Text,
					Subject = p.Subject,
					TopicId = p.TopicId ?? 0,
					TopicTitle = p.Topic.Title,
					ForumId = p.Topic.ForumId,
					ForumName = p.Topic.Forum.Name,
					PosterId = p.PosterId,
					PosterName = p.Poster.UserName,
					PosterRoles = p.Poster.UserRoles
						.Where(ur => !ur.Role.IsDefault)
						.Select(ur => ur.Role.Name)
						.ToList(),
					PosterLocation = p.Poster.From,
					Signature = p.Poster.Signature,
					PosterAvatar = p.Poster.Avatar,
					PosterJoined = p.Poster.CreateTimeStamp,
					PosterPostCount = p.Poster.Posts.Count,
				})
				.OrderBy(p => p.CreateTimestamp)
				.PageOfAsync(_db, Search);

			foreach (var post in Posts)
			{
				post.RenderedText = RenderPost(post.Text, post.EnableBbCode, post.EnableHtml);
				post.RenderedSignature = RenderSignature(post.Signature);
			}
		}
	}
}
