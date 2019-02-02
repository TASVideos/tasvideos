using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

using Microsoft.AspNetCore.Authorization;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Services;

namespace TASVideos.Models
{
	/// <summary>
	/// Represents a <seealso cref="User"/> entry list of users
	/// </summary>
	[AllowAnonymous]
	public class UserListModel
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
	/// Represents a <see cref="User"/> for the purpose of editing
	/// </summary>
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

		public string OriginalUserName => UserName;
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

		[Display(Name = "Player Points:")]
		public int PlayerPoints { get; set; }

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

		[Display(Name = "Ratings Public?")]
		public bool PublicRatings { get; set; }

		[Display(Name = "TimeZone:")]
		public string TimeZone { get; set; }

		// Private info
		[Display(Name = "Email:")]
		public string Email { get; set; }

		[DisplayName("Email Confirmed")]
		public bool EmailConfirmed { get; set; }

		[DisplayName("Locked Out Status")]
		public bool IsLockedOut { get; set; }

		public int PublicationActiveCount { get; set; }
		public int PublicationObsoleteCount { get; set; }
		public bool AnyPublications => PublicationActiveCount + PublicationObsoleteCount > 0;
		public IEnumerable<string> PublishedSystems { get; set; } = new List<string>();

		public WikiEditModel WikiEdits { get; set; } = new WikiEditModel();

		public IEnumerable<RoleBasicDisplay> Roles { get; set; } = new List<RoleBasicDisplay>();
		public IEnumerable<AwardEntryDto> Awards { get; set; } = new List<AwardEntryDto>();
		public IEnumerable<SubmissionEntry> Submissions { get; set; } = new List<SubmissionEntry>();

		public RatingModel Ratings { get; set; } = new RatingModel();
		public UserFilesModel UserFiles { get; set; } = new UserFilesModel();

		public int SubmissionCount => Submissions.Sum(s => s.Count);

		public class SubmissionEntry
		{
			public SubmissionStatus Status { get; set; }
			public int Count { get; set; }
		}

		public class WikiEditModel
		{
			public int TotalEdits { get; set; }
			public DateTime? FirstEdit { get; set; }
			public DateTime? LastEdit { get; set; }
		}

		public class RatingModel
		{
			// TODO: obsolete vs non-obsolete
			// TODO: percentage of total non-obsolete
			public int TotalMoviesRated { get; set; }
		}

		public class UserFilesModel
		{
			public int Total { get; set; }
			public IEnumerable<string> Systems { get; set; } = new List<string>();
		}
	}

	/// <summary>
	/// Represents a user's ratings
	/// </summary>
	public class UserRatingsModel
	{
		public int Id { get; set; }
		public string UserName { get; set; }
		public bool PublicRatings { get; set; }

		public IEnumerable<Rating> Ratings { get; set; } = new List<Rating>();

		public class Rating
		{
			public int PublicationId { get; set; }
			public string PublicationTitle { get; set; }
			public bool IsObsolete { get; set; }
			public double? Entertainment { get; set; }
			public double Tech { get; set; }

			public double Average => ((Entertainment ?? 0) + (Entertainment ?? 0) + Tech) / 3.0;
		}
	}
}
