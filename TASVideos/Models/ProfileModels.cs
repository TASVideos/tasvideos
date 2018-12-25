using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TASVideos.Models
{
	public class ProfileIndexModel
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

		public IEnumerable<RoleBasicDisplay> Roles { get; set; } = new List<RoleBasicDisplay>();
	}

	public class ChangePasswordModel
	{
		[Required]
		[DataType(DataType.Password)]
		[Display(Name = "Current password")]
		public string OldPassword { get; set; }

		[Required]
		[StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 12)]
		[DataType(DataType.Password)]
		[Display(Name = "New password")]
		public string NewPassword { get; set; }

		[DataType(DataType.Password)]
		[Display(Name = "Confirm new password")]
		[Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
		public string ConfirmPassword { get; set; }

		public string StatusMessage { get; set; }
	}

	public class SetPasswordModel
	{
		[Required]
		[StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 12)]
		[DataType(DataType.Password)]
		[Display(Name = "New password")]
		public string NewPassword { get; set; }

		[DataType(DataType.Password)]
		[Display(Name = "Confirm new password")]
		[Compare(nameof(NewPassword), ErrorMessage = "The new password and confirmation password do not match.")]
		public string ConfirmPassword { get; set; }

		public string StatusMessage { get; set; }
	}

	public class WatchedTopicsModel
	{
		public DateTime TopicCreateTimeStamp { get; set; }
		public bool IsNotified { get; set; }
		public int ForumId { get; set; }
		public string ForumTitle { get; set; }
		public int TopicId { get; set; }
		public string TopicTitle { get; set; }
	}
}
