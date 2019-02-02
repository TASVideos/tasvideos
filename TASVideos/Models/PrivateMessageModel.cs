using System;

namespace TASVideos.Models
{
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
