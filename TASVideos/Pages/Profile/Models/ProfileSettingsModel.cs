﻿using System.ComponentModel.DataAnnotations;
using TASVideos.Core.Services;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Profile.Models;

public class ProfileSettingsModel
{
	public string Username { get; set; } = "";

	public bool IsEmailConfirmed { get; set; }

	[Display(Name = "Current Email")]
	public string Email { get; set; } = "";

	[Display(Name = "Time Zone")]
	public string TimeZoneId { get; set; } = TimeZoneInfo.Utc.Id;

	[Display(Name = "Allow Movie Ratings to be public?")]
	public bool PublicRatings { get; set; }

	[Display(Name = "Location")]
	public string? From { get; set; }

	[StringLength(1000)]
	public string? Signature { get; set; }

	[Url]
	[Display(Name = "Avatar URL")]
	public string? Avatar { get; set; }

	[Url]
	[Display(Name = "Mood-variant avatar URL")]
	public string? MoodAvatar { get; set; }

	[Display(Name = "Preferred Pronouns")]
	public PreferredPronounTypes PreferredPronouns { get; set; }

	[Display(Name = "Email On New Private Message?")]
	public bool EmailOnPrivateMessage { get; set; }

	[Display(Name = "Automatically Watch Topics When Posting")]
	public UserPreference AutoWatchTopic { get; set; }

	public IEnumerable<RoleDto> Roles { get; set; } = new List<RoleDto>();
}
