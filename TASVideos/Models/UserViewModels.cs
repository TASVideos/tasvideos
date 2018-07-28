using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

using TASVideos.Data;

namespace TASVideos.Models
{
	/// <summary>
	/// Represents a <seealso cref="TASVideos.Data.Entity.User"/> entry list of users
	/// </summary>
	public class UserListViewModel
    {
		[Sortable]
		public int Id { get; set; }

		[DisplayName("User Name")]
		[Sortable]
		public string UserName { get; set; }

		[DisplayName("Role")]
		public IEnumerable<string> Roles { get; set; } = new List<string>();

		[DisplayName("Created")]
		[Sortable]
		public DateTime CreateTimeStamp { get; set; }

		// Dummy to generate column header
		public object Actions { get; set; }
	}

	/// <summary>
	/// Represents a <see cref="TASVideos.Data.Entity.User"/> for the purpose of viewing
	/// </summary>
	public class UserDetailsViewModel
	{
		public int Id { get; set; }

		[DisplayName("Account Created On")]
		public DateTime CreateTimeStamp { get; set; }

		[DisplayName("User Last Logged In")]
		[DisplayFormat(NullDisplayText = "Never")]
		public DateTime? LastLoggedInTimeStamp { get; set; }

		[DisplayName("User Name")]
		public string UserName { get; set; }

		[DisplayName("Lock out Status")]
		public bool IsLockedOut { get; set; }

		public string Email { get; set; }

		[DisplayName("Email Confirmed")]
		public bool EmailConfirmed { get; set; }

		[DisplayName("Time Zone")]
		public string TimezoneId { get; set; }

		[DisplayName("Ratings Public?")]
		public bool PublicRatings { get; set; }

		[Display(Name = "Location")]
		[DisplayFormat(NullDisplayText = "Not Set")]
		public string From { get; set; }

		[DisplayName("Current Roles")]
		public IEnumerable<string> Roles { get; set; } = new List<string>();
	}

	/// <summary>
	/// Represents a <see cref="TASVideos.Data.Entity.User"/> for the purpose of editing
	/// </summary>
	public class UserEditViewModel : UserEditPostViewModel
	{
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

		public string OriginalUserName => UserName;

		[DisplayName("Available Roles")]
		public IEnumerable<SelectListItem> AvailableRoles { get; set; } = new List<SelectListItem>();
	}

	/// <summary>
	/// Just the fields that can be posted from the <see cref="TASVideos.Data.Entity.User" /> edit page
	/// </summary>
	public class UserEditPostViewModel
	{
		public int Id { get; set; }

		[DisplayName("User Name")]
		public string UserName { get; set; }

		[DisplayName("Time Zone")]
		public string TimezoneId { get; set; }

		[Display(Name = "Location")]
		public string From { get; set; }

		[DisplayName("Selected Roles")]
		public IEnumerable<int> SelectedRoles { get; set; } = new List<int>();
	}

	/// <summary>
	/// Represents a summary of a given user intended to be displayed in places such as their homepage
	/// </summary>
	public class UserSummaryModel
	{
		public int Id { get; set; }
		public string UserName { get; set; }
		public int EditCount { get; set; }
		public int MovieCount { get; set; }
		public int SubmissionCount { get; set; }
		public IEnumerable<string> Awards { get; set; } = new List<string>();
		public int AwardsWon { get; set; }
	}

	/// <summary>
	/// Represents a user with publicly available information
	/// Intended for the User/Profile page
	/// </summary>
	public class UserProfileModel
	{
		public int Id { get; set; }
		public string UserName { get; set; }

		[Display(Name = "Joined On:")]
		public DateTime JoinDate { get; set; }

		[Display(Name = "Last Logged In: ")]
		[DisplayFormat(NullDisplayText = "Never")]
		public DateTime? LastLoggedInTimeStamp { get; set; }

		[Display(Name = "Total posts:")]
		public int PostCount { get; set; }

		[Display(Name = "Avatar")]
		public string Avatar { get; set; }

		[Display(Name = "Location:")]
		public string Location { get; set; }

		[Display(Name = "Signature")]
		public string Signature { get; set; }

		public int PublicationActiveCount { get; set; }
		public int PublicationObsoleteCount { get; set; }
		public bool AnyPublications => PublicationActiveCount + PublicationObsoleteCount > 0;
		public IEnumerable<string> PublishedSystems { get; set; } = new List<string>();

		public int SubmissionCount { get; set; }
		
		public IEnumerable<RoleBasicDisplay> Roles { get; set; } = new List<RoleBasicDisplay>();
		public IEnumerable<AwardDisplayModel> Awards { get; set; } = new List<AwardDisplayModel>();
	}
}
