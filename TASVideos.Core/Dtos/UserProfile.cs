using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using TASVideos.Data.Entity;

// TODO: Create a view model to separate presentation concerns (Display attributes)
namespace TASVideos.Core.Services
{
	/// <summary>
	/// Represents a user with publicly available information
	/// Intended for the User/Profile page.
	/// </summary>
	public class UserProfile
	{
		public int Id { get; set; }
		public string UserName { get; set; } = "";

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
		public string? Avatar { get; set; }

		[Display(Name = "Location:")]
		public string? Location { get; set; }

		[Display(Name = "Signature")]
		public string? Signature { get; set; }

		[Display(Name = "Ratings Public?")]
		public bool PublicRatings { get; set; }

		[Display(Name = "TimeZone:")]
		public string? TimeZone { get; set; }

		// Private info
		[Display(Name = "Email:")]
		public string? Email { get; set; }

		[DisplayName("Email Confirmed")]
		public bool EmailConfirmed { get; set; }

		[DisplayName("Locked Out Status")]
		public bool IsLockedOut { get; set; }

		public int PublicationActiveCount { get; set; }
		public int PublicationObsoleteCount { get; set; }
		public bool AnyPublications => PublicationActiveCount + PublicationObsoleteCount > 0;
		public IEnumerable<string> PublishedSystems { get; set; } = new List<string>();

		public WikiEdit WikiEdits { get; set; } = new ();

		public IEnumerable<RoleDto> Roles { get; set; } = new List<RoleDto>();
		public IEnumerable<AwardAssignmentSummary> Awards { get; set; } = new List<AwardAssignmentSummary>();
		public IEnumerable<SubmissionEntry> Submissions { get; set; } = new List<SubmissionEntry>();

		public RatingSummary Ratings { get; set; } = new ();
		public UserFileSummary UserFiles { get; set; } = new ();

		public int SubmissionCount => Submissions.Sum(s => s.Count);

		public class SubmissionEntry
		{
			public SubmissionStatus Status { get; set; }
			public int Count { get; set; }
		}

		public class WikiEdit
		{
			public int TotalEdits { get; set; }
			public DateTime? FirstEdit { get; set; }
			public DateTime? LastEdit { get; set; }

			public DateTime FirstEditDateTime => FirstEdit ?? DateTime.UtcNow;
			public DateTime LastEditDateTime => LastEdit ?? DateTime.UtcNow;
		}

		public class RatingSummary
		{
			// TODO: obsolete vs non-obsolete
			// TODO: percentage of total non-obsolete
			public int TotalMoviesRated { get; set; }
		}

		public class UserFileSummary
		{
			public int Total { get; set; }
			public IEnumerable<string> Systems { get; set; } = new List<string>();
		}
	}
}
