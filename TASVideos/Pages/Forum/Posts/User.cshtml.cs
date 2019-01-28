using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;
using TASVideos.Models;
using TASVideos.Services;

namespace TASVideos.Pages.Forum.Posts
{
	[AllowAnonymous]
	public class UserModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;
		private readonly IAwardsCache _awards;

		public UserModel(
			ApplicationDbContext db,
			IAwardsCache awards)
		{
			_db = db;
			_awards = awards;
		}

		[FromRoute]
		public string UserName { get; set; }

		[FromQuery]
		public UserPostsRequest Search { get; set; }

		public UserPostsModel UserPosts { get; set; }

		public IEnumerable<AwardEntryDto> Awards { get; set; } = new List<AwardEntryDto>(); 

		public async Task<IActionResult> OnGet()
		{
			UserPosts = await _db.Users
				.Where(u => u.UserName == UserName)
				.Select(u => new UserPostsModel
				{
					Id = u.Id,
					UserName = u.UserName,
					Joined = u.CreateTimeStamp,
					Location = u.From,
					Avatar = u.Avatar,
					Signature = u.Signature,
					Roles = u.UserRoles
						.Where(ur => !ur.Role.IsDefault)
						.Select(ur => ur.Role.Name)
						.ToList()
				})
				.SingleOrDefaultAsync();

			if (UserPosts == null)
			{
				return NotFound();
			}

			Awards = await _awards.AwardsForUser(UserPosts.Id);

			bool seeRestricted = User.Has(PermissionTo.SeeRestrictedForums);
			UserPosts.Posts = _db.ForumPosts
				.CreatedBy(UserName)
				.ExcludeRestricted(seeRestricted)
				.Select(p => new UserPostsModel.Post
				{
					Id = p.Id,
					CreateTimestamp = p.CreateTimeStamp,
					EnableHtml = p.EnableHtml,
					EnableBbCode = p.EnableBbCode,
					Text = p.Text,
					Subject = p.Subject,
					TopicId = p.TopicId ?? 0,
					TopicTitle = p.Topic.Title,
					ForumId = p.Topic.ForumId,
					ForumName = p.Topic.Forum.Name
				})
				.SortedPageOf(_db, Search);

			UserPosts.RenderedSignature = RenderSignature(UserPosts.Signature); 
			foreach (var post in UserPosts.Posts)
			{
				post.RenderedText = RenderPost(post.Text, post.EnableBbCode, post.EnableHtml);
			}

			return Page();
		}
	}
}
