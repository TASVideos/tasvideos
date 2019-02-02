using System.ComponentModel.DataAnnotations;

namespace TASVideos.Pages.Publications.Models
{
	public class AuthorListEntry
	{
		public int Id { get; set; }

		[Display(Name = "Author")]
		public string UserName { get; set; }

		[Display(Name = "Active Movies")]
		public int ActivePublicationCount { get; set; }

		[Display(Name = "Obsolete Movies")]
		public int ObsoletePublicationCount { get; set; }
	}
}
