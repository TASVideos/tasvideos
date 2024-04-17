using Microsoft.AspNetCore.Mvc.Rendering;

namespace TASVideos.Pages.Exhibitions.Models;

public class ExhibitionFormModel
{
	public enum FormType
	{
		Add,
		Edit
	}

	public FormType Type { get; set; }
	public ExhibitionAddEditModel Exhibition { get; set; } = new();
	public List<SelectListItem> AvailableGames { get; set; } = [];
	public List<SelectListItem> AvailableUsers { get; set; } = [];
}
