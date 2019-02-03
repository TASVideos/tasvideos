using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TASVideos.Pages.Forum.Topics.Models
{
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
}
