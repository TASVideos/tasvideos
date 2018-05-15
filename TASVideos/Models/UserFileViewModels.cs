using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TASVideos.Models
{
	public class UserMovieListViewModel
	{
		public IEnumerable<Entry> Entries { get; set; } = new List<Entry>();

		public class Entry
		{
			public long Id { get; set; }

			[Display(Name = "By")]
			public string Author { get; set; }

			[Display(Name = "Uploaded")]
			public DateTime Uploaded { get; set; }

			[Display(Name = "Filename")]
			public string FileName { get; set; }

			[Display(Name = "Title")]
			public string Title { get; set; }
		}
	}

	public class UserFileViewModel
	{
		public long Id { get; set; }

		public string Title { get; set; }

		public string Description { get; set; }

		public DateTime Uploaded { get; set; }

		public string Author { get; set; }

		public int Views { get; set; }

		public int Downloads { get; set; }

		public bool Hidden { get; set; }

		public string FileName { get; set; }
		public int FileSize { get; internal set; }
	}

	public class UserMovieViewModel : UserFileViewModel
	{
		public TimeSpan Length { get; set; }

		public int Frames { get; set; }

		public int Rerecords { get; set; }
	}

	public class UserFileIndexViewModel
	{ }

	public class UserFileUserIndexViewModel
	{
		public string UserName { get; set; }

		public IEnumerable<UserFileViewModel> Files { get; set; }
	}

	public class UserFileDataViewModel
	{
		public byte[] Content { get; set; }

		public string FileName { get; set; }

		public bool Hidden { get; set; }

		public int AuthorId { get; set; }
	}
}
