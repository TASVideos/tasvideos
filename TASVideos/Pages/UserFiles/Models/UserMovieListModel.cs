using System;
using System.ComponentModel.DataAnnotations;

namespace TASVideos.Pages.UserFiles.Models
{
	public class UserMovieListModel
	{
		public long Id { get; set; }

		[Display(Name = "By")]
		public string Author { get; set; }

		[Display(Name = "Uploaded")]
		public DateTime UploadedTimestamp { get; set; }

		[Display(Name = "Filename")]
		public string FileName { get; set; }

		[Display(Name = "Title")]
		public string Title { get; set; }
	}
}
