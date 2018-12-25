using System.ComponentModel.DataAnnotations;

namespace TASVideos.Models
{
	public class LoginModel
	{
		[Required]
		[Display(Name = "User Name")]
		public string UserName { get; set; }

		[Required]
		[DataType(DataType.Password)]
		public string Password { get; set; }

		[Display(Name = "Remember me?")]
		public bool RememberMe { get; set; }
	}

	public class RegisterModel
	{
		[Display(Name = "Time Zone")]
		public string SelectedTimeZone { get; set; }

		[Required]
		[StringLength(256)]
		[Display(Name = "User Name")]
		public string UserName { get; set; }

		[Required]
		[EmailAddress]
		[Display(Name = "Email")]
		public string Email { get; set; }

		[Required]
		[StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 12)]
		[DataType(DataType.Password)]
		[Display(Name = "Password")]
		public string Password { get; set; }

		[DataType(DataType.Password)]
		[Display(Name = "Confirm password")]
		[Compare(nameof(Password), ErrorMessage = "The password and confirmation password do not match.")]
		public string ConfirmPassword { get; set; }

		[Display(Name = "Location")]
		[StringLength(256)]
		public string From { get; set; }
	}

	public class ResetPasswordModel
	{
		[Required]
		[EmailAddress]
		public string Email { get; set; }

		[Required]
		[StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 12)]
		[DataType(DataType.Password)]
		public string Password { get; set; }

		[DataType(DataType.Password)]
		[Display(Name = "Confirm password")]
		[Compare(nameof(Password), ErrorMessage = "The password and confirmation password do not match.")]
		public string ConfirmPassword { get; set; }

		public string Code { get; set; }
	}

	public class ForgotPasswordModel
	{
		[Required]
		[EmailAddress]
		public string Email { get; set; }
	}
}
