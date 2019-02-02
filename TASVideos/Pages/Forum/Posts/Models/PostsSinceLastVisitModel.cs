using System;
using System.Collections.Generic;
using TASVideos.Services;

namespace TASVideos.Pages.Forum.Posts.Models
{
	public class PostsSinceLastVisitModel
	{
		public int Id { get; set; }
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

		public int PosterId { get; set; }
		public string PosterName { get; set; }
		public string PosterAvatar { get; set; }
		public string PosterLocation { get; set; }
		public IEnumerable<string> PosterRoles { get; set; }
		public int PosterPostCount { get; set; }
		public DateTime PosterJoined { get; set; }
		public string Signature { get; set; }
		public string RenderedSignature { get; set; }

		public IEnumerable<AwardEntryDto> Awards { get; set; } = new List<AwardEntryDto>();
	}
}
