using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

using TASVideos.Data.Entity;

namespace TASVideos.TagHelpers
{
	[HtmlTargetElement("timezone-convert", TagStructure = TagStructure.WithoutEndTag)]
	public class TimeZoneConvert : TagHelper
	{
		private readonly ClaimsPrincipal _claimsPrincipal;
		private readonly UserManager<User> _userManager;

		public TimeZoneConvert(
			ClaimsPrincipal claimsPrincipal,
			UserManager<User> userManager)
		{
			_claimsPrincipal = claimsPrincipal;
			_userManager = userManager;
		}

		public ModelExpression AspFor { get; set; }

		public DateTime ConvertedDateTime => (DateTime)AspFor.Model;

		public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
		{
			ValidateExpression();

			var user = await _userManager.GetUserAsync(_claimsPrincipal);

			var dateTime = ConvertedDateTime;
			if (user != null)
			{
				var userTimeZone = TimeZoneInfo.FindSystemTimeZoneById(user.TimeZoneId);
				dateTime = TimeZoneInfo.ConvertTimeFromUtc(dateTime, userTimeZone);
			}

			output.TagName = "span";
			output.TagMode = TagMode.StartTagAndEndTag;
			output.Content.AppendHtml(dateTime.ToString(CultureInfo.CurrentCulture));
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
