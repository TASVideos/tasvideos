using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TASVideos.Pages.Users.Models
{
	public class UserEditModel
	{
		[DisplayName("User Name")]
		public string UserName { get; set; }

		[DisplayName("Time Zone")]
		public string TimezoneId { get; set; }

		[Display(Name = "Location")]
		public string From { get; set; }

		[DisplayName("Selected Roles")]
		public IEnumerable<int> SelectedRoles { get; set; } = new List<int>();

		[DisplayName("Account Created On")]
		public DateTime CreateTimeStamp { get; set; }

		[DisplayName("User Last Logged In")]
		[DisplayFormat(NullDisplayText = "Never")]
		public DateTime? LastLoggedInTimeStamp { get; set; }

		[EmailAddress]
		public string Email { get; set; }

		public bool EmailConfirmed { get; set; }

		[Display(Name = "Locked Status")]
		public bool IsLockedOut { get; set; }

		public string Signature { get; set; }
		public string Avatar { get; set; }

		public string OriginalUserName => UserName;
	}
}
