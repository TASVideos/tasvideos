using System;
using System.ComponentModel.DataAnnotations;

namespace TASVideos.Models
{
	public class InboxEntry
	{
		public int Id { get; set; }

		[Display(Name = "Subject")]
		public string Subject { get; set; }

		[Display(Name = "From")]
		public string FromUser { get; set; }

		[Display(Name = "Date")]
		public DateTime SendDate { get; set; }

		public bool IsRead { get; set; }
	}

	public class SaveboxEntry
	{
		public int Id { get; set; }

		[Display(Name = "Subject")]
		public string Subject { get; set; }

		[Display(Name = "From")]
		public string FromUser { get; set; }

		[Display(Name = "To")]
		public string ToUser { get; set; }

		[Display(Name = "Date")]
		public DateTime SendDate { get; set; }
	}

	public class SentboxEntry
	{
		public int Id { get; set; }

		[Display(Name = "Subject")]
		public string Subject { get; set; }

		[Display(Name = "To")]
		public string ToUser { get; set; }

		[Display(Name = "Date")]
		public DateTime SendDate { get; set; }

		public bool HasBeenRead { get; set; }
	}

	public class PrivateMessageModel
	{
		public string Subject { get; set; }
		public DateTime SentOn { get; set; }
		public string Text { get; set; }
		public string RenderedText { get; set; }
		public int FromUserId { get; set; }
		public string FromUserName { get; set; }

		public int ToUserId { get; set; }
		public string ToUserName { get; set; }

		public bool CanReply { get; set; }

		public bool EnableBbCode { get; set; }
		public bool EnableHtml { get; set; }
	}
}
