using System.ComponentModel.DataAnnotations;

namespace TASVideos.Pages.Users.Models;

public class UserEmailEditModel
{
	public string UserName { get; init; } = "";

	[Required]
	[EmailAddress]
	public string? Email { get; init; }

	[Display(Name = "Email Confirmed")]
	public bool EmailConfirmed { get; init; }
}
