using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TASVideos.Models
{
	public class InboxModel
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

	public class SaveboxModel
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

	public class SentboxModel
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
		public int Id { get; set; }
		public string Subject { get; set; }
		public DateTime SentOn { get; set; }
		public string Text { get; set; }
		public int FromUserId { get; set; }
		public string FromUserName { get; set; }

		public int ToUserId { get; set; }
		public string ToUserName { get; set; }

		public bool CanReply { get; set; }
	}

	public class PrivateMessageCreateModel
	{
		[Display(Name = "Subject")]
		[Required]
		[StringLength(100, MinimumLength = 3)]
		public string Subject { get; set; }

		[Required]
		[Display(Name = "Message Body")]
		[StringLength(1000, MinimumLength = 5)]
		public string Text { get; set; }

		[Required]
		[Display(Name = "Username", Description = "Enter a UserName")]
		public string ToUser { get; set; }
	}
}
