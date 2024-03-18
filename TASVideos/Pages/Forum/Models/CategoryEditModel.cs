using System.ComponentModel.DataAnnotations;

namespace TASVideos.Pages.Forum.Models;

public class CategoryEditModel
{
	[StringLength(30)]
	public string Title { get; set; } = "";

	public string? Description { get; set; }

	public IList<ForumEditModel> Forums { get; set; } = [];

	public class ForumEditModel
	{
		public int Id { get; set; }

		[StringLength(50)]
		public string Name { get; set; } = "";

		[StringLength(1000)]
		public string? Description { get; set; }

		public int Ordinal { get; set; }
	}
}
