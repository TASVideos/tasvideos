using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TASVideos.Models
{
	public class ProfileIndexViewModel
	{
		public string Username { get; set; }

		public bool IsEmailConfirmed { get; set; }

		[Required]
		[EmailAddress]
		public string Email { get; set; }

		[Display(Name = "Time Zone")]
		public string TimeZoneId { get; set; }

		public string StatusMessage { get; set; }

		public IEnumerable<RoleBasicDisplay> Roles { get; set; } = new List<RoleBasicDisplay>();
	}

	public class ChangePasswordViewModel
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

	public class GenerateRecoveryCodesViewModel
	{
		public string[] RecoveryCodes { get; set; }
	}

	public class SetPasswordViewModel
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
}
