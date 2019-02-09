using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TASVideos.Models;

namespace TASVideos.Pages.Profile.Models
{
	public class ProfileSettingsModel
	{
		public string Username { get; set; }

		public bool IsEmailConfirmed { get; set; }

		[Required]
		[EmailAddress]
		public string Email { get; set; }

		[Display(Name = "Time Zone")]
		public string TimeZoneId { get; set; }

		public string StatusMessage { get; set; }

		[Display(Name = "Allow Movie Ratings to be public?")]
		public bool PublicRatings { get; set; }

		[Display(Name = "Location")]
		public string From { get; set; }

		public string Signature { get; set; }

		public IEnumerable<RoleBasicDisplay> Roles { get; set; } = new List<RoleBasicDisplay>();
	}
}
