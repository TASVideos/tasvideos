using Microsoft.AspNetCore.Mvc.Rendering;
using TASVideos.Data.Entity.Exhibition;

namespace TASVideos.Pages.Exhibitions.Models;

public class ExhibitionFormModel
{
	public ExhibitionAddEditModel Exhibition { get; set; } = new();
	public List<SelectListItem> AvailableGames { get; set; } = [];
	public List<SelectListItem> AvailableUsers { get; set; } = [];
	public List<ExhibitionStatus> AvailableStatuses { get; set; } = [];
}
