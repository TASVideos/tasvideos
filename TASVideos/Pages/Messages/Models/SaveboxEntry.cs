using System;
using System.ComponentModel.DataAnnotations;

namespace TASVideos.Pages.Messages.Models
{
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
}
