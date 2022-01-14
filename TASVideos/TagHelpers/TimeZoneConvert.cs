using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using TASVideos.Common;
using TASVideos.Core.Services;

namespace TASVideos.TagHelpers
{
	[HtmlTargetElement("timezone-convert", TagStructure = TagStructure.WithoutEndTag, Attributes = "asp-for")]
	public class TimeZoneConvert : TagHelper
	{
		private static readonly IReadOnlyDictionary<string, TimeZoneInfo> Timezones = TimeZoneInfo
			.GetSystemTimeZones()
			.ToDictionary(tkey => tkey.Id);

		private readonly ClaimsPrincipal _claimsPrincipal;
		private readonly UserManager _userManager;

		public TimeZoneConvert(
			ClaimsPrincipal claimsPrincipal,
			UserManager userManager)
		{
			_claimsPrincipal = claimsPrincipal;
			_userManager = userManager;
		}

		public ModelExpression AspFor { get; set; } = null!;

		public bool DateOnly { get; set; }
		public bool RelativeTime { get; set; } = true;
		public bool InLine { get; set; }

		public DateTime ConvertedDateTime => (DateTime)AspFor.Model;

		public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
		{
			ValidateExpression();

			var user = await _userManager.GetUserAsync(_claimsPrincipal);

			var dateTime = ConvertedDateTime;

			TimeZoneInfo? userTimeZone = null;
			if (user is not null)
			{
				// Simply do not convert, if the user has no known timezone;
				if (Timezones.TryGetValue(user.TimeZoneId, out userTimeZone))
				{
					dateTime = TimeZoneInfo.ConvertTimeFromUtc(dateTime, userTimeZone);
				}

				if (userTimeZone != null)
				{
					dateTime = TimeZoneInfo.ConvertTimeFromUtc(dateTime, userTimeZone);
				}
			}

			if (userTimeZone is not null)
			{
				var offset = userTimeZone.GetUtcOffset(dateTime);
				output.Attributes.Add("title", dateTime + " UTC" + (offset < TimeSpan.Zero ? "-" : "+") + offset.ToString(@"hh\:mm"));
			}
			else
			{
				output.Attributes.Add("title", dateTime + " UTC");
			}

			string dateStr;

			TimeSpan? relativeTime = null;
			if (RelativeTime)
			{
				relativeTime = DateTime.UtcNow - ConvertedDateTime;
			}

			if (relativeTime?.Days < 30)
			{
				dateStr = ((TimeSpan)relativeTime).ToRelativeString();
			}
			else
			{
				dateStr = DateOnly
					? dateTime.ToShortDateString()
					: dateTime.ToString("g");
				if (InLine)
				{
					dateStr = "on " + dateStr;
				}
			}

			output.TagName = "span";
			output.TagMode = TagMode.StartTagAndEndTag;
			output.Content.AppendHtml(TagHelperExtensions.Text(dateStr));
		}

		private void ValidateExpression()
		{
			var type = AspFor.ModelExplorer.ModelType;
			if (!typeof(DateTime).IsAssignableFrom(type))
			{
				throw new ArgumentException($"Invalid property type {type}, {nameof(AspFor)} must be a {nameof(DateTime)}");
			}
		}
	}
}
