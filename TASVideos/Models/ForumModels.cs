using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using TASVideos.Data;
using TASVideos.Data.Constants;

namespace TASVideos.Models
{
	public class PostPositionModel
	{
		public int Page { get; set; }
		public int TopicId { get; set; }
	}

	public class ForumPostEditModel
	{
		public int PosterId { get; set; }
		public string PosterName { get; set; }
		public DateTime CreateTimestamp { get; set; }

		public bool EnableBbCode { get; set; }
		public bool EnableHtml { get; set; }

		public int TopicId { get; set; }
		public string TopicTitle { get; set; }

		public string Subject { get; set; }
		public string Text { get; set; }
		public string RenderedText { get; set; }

		public bool IsLastPost { get; set; }
	}

	public class PollResultModel
	{
		public string TopicTitle { get; set; }
		public int TopicId { get; set; }

		public string Question { get; set; }

		public IEnumerable<VoteResult> Votes { get; set; } = new List<VoteResult>();

		public class VoteResult
		{
			public int UserId { get; set; }

			[Display(Name = "User")]
			public string UserName { get; set; }

			public int Ordinal { get; set; }

			[Display(Name = "Option")]
			public string OptionText { get; set; }

			[Display(Name = "Vote On")]
			public DateTime CreateTimestamp { get; set; }

			[Display(Name = "IP Address")]
			public string IpAddress { get; set; }
		}
	}

	public class UnansweredPostsModel
	{
		public int ForumId { get; set; }

		[Display(Name = "Forum")]
		public string ForumName { get; set; }

		public int TopicId { get; set; }

		[Display(Name = "Topic")]
		public string TopicName { get; set; }

		public int AuthorId { get; set; }

		[Display(Name = "Author")]
		public string AuthorName { get; set; }

		[Display(Name = "Posted On")]
		public DateTime PostDate { get; set; }
	}

	public class ForumEditModel
	{
		[Required]
		public string Name { get; set; }

		[Required]
		public string Description { get; set; }
	}

	public class CategoryEditModel
	{
		[Required]
		[StringLength(30)]
		public string Title { get; set; }

		public string Description { get; set; }

		public IList<ForumEditModel> Forums { get; set; } = new List<ForumEditModel>();

		public class ForumEditModel
		{
			public int Id { get; set; }

			[Required]
			[StringLength(50)]
			public string Name { get; set; }

			public string Description { get; set; }
			public int Ordinal { get; set; }
		}
	}

	public class UserPostsRequest : PagedModel
	{
		public UserPostsRequest()
		{
			PageSize = ForumConstants.PostsPerPage;
			SortDescending = true;
			SortBy = nameof(UserPostsModel.Post.CreateTimestamp);
		}
	}

	public class UserPostsModel
	{
		public int Id { get; set; }
		public string UserName { get; set; }
		public DateTime Joined { get; set; }
		public string Location { get; set; }
		public string Avatar { get; set; }
		public string Signature { get; set; }
		public string RenderedSignature { get; set; }

		public IEnumerable<string> Roles { get; set; } = new List<string>();

		public PageOf<Post> Posts { get; set; }

		public class Post
		{
			public int Id { get; set; }

			[Sortable]
			public DateTime CreateTimestamp { get; set; }
			public bool EnableBbCode { get; set; }
			public bool EnableHtml { get; set; }
			public string Text { get; set; }
			public string RenderedText { get; set; }
			public string Subject { get; set; }
			public int TopicId { get; set; }
			public string TopicTitle { get; set; }
			public int ForumId { get; set; }
			public string ForumName { get; set; }
		}
	}
}
