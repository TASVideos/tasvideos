using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace TASVideos.Data.Entity.Forum
{
	public enum ForumPostMood
	{
		None = 0,
		Normal = 1,
		Angry = 2,
		Unhappy = 3,
		Playful = 4,
		Nuclear = 5,
		Delight = 6,
		Guru = 7,
		Hope = 8,
		Puzzled = 9,
		Happy = 10,
		Hyper = 11,
		Grief = 12,
		Bleh = 13,
		Shy = 41,
		Plot = 42,
		Assertive = 43,
		Admin = 44,
		Upset = 202,
		Xmas = 254,

		AltNormal = 1001,
		AltAngry = 1002,
		AltUnhappy = 1003,
		AltPlayful = 1004,
		AltNuclear = 1005,
		AltDelight = 1006,
		AltGuru = 1007,
		AltHope = 1008,
		AltPuzzled = 1009,
		AltHappy = 1010,
		AltHyper = 1011,
		AltGrief = 1012,
		AltBleh = 1013,
		What = 1201
	}

	public class ForumPost : BaseEntity
	{
		public int Id { get; set; }

		public int? TopicId { get; set; }
		public virtual ForumTopic Topic { get; set; }

		public int PosterId { get; set; }
		public virtual User Poster { get; set; }

		[StringLength(50)]
		public string IpAddress { get; set; }

		[StringLength(500)]
		public string Subject { get; set; }

		[Required]
		public string Text { get; set; }

		public bool EnableHtml { get; set; }
		public bool EnableBbCode { get; set; }

		public ForumPostMood PosterMood { get; set; }
	}

	public static class ForumPostQueryableExtensions
	{
		public static IQueryable<ForumPost> ExcludeRestricted(this IQueryable<ForumPost> list, bool seeRestricted)
		{
			return list.Where(f => seeRestricted || !f.Topic.Forum.Restricted);
		}

		public static IQueryable<ForumPost> ForTopic(this IQueryable<ForumPost> list, int topicId)
		{
			return list.Where(p => p.TopicId == topicId);
		}
	}
}
