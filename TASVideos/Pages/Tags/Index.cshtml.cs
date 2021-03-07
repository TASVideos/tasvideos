using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Data.Entity;
using TASVideos.Services;

namespace TASVideos.Pages.Tags
{
	[RequirePermission(PermissionTo.TagMaintenance)]
	public class IndexModel : BasePageModel
	{
		private readonly ITagService _tagService;

		public IndexModel(ITagService tagService)
		{
			_tagService = tagService;
		}

		[TempData]
		public string? Message { get; set; }

		[TempData]
		public string? MessageType { get; set; }

		public bool ShowMessage => !string.IsNullOrWhiteSpace(Message);

		public IEnumerable<Tag> Tags { get; set; } = new List<Tag>();

		public async Task OnGet()
		{
			Tags = (await _tagService.GetAll())
				.OrderBy(t => t.Code)
				.ToList();
		}
	}
}
