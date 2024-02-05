using System.ComponentModel.DataAnnotations;
using TASVideos.Data.Entity.Exhibition;

namespace TASVideos.Pages.Exhibitions.Models;

public class EditUrlsUrlModel
{
	public int Id { get; set; }
	[Required]
	public string Url { get; set; } = "";
	public ExhibitionUrlType Type { get; set; }
	public string DisplayName { get; set; } = "";
}