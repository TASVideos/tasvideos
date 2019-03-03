using System;
using System.Globalization;
using System.Security.Claims;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

using TASVideos.Services;

namespace TASVideos.TagHelpers
{
	[HtmlTargetElement("timezone-convert", TagStructure = TagStructure.WithoutEndTag)]
	public class TimeZoneConvert : TagHelper
	{
		private readonly ClaimsPrincipal _claimsPrincipal;
		private readonly UserManager _userManager;

		public TimeZoneConvert(
			ClaimsPrincipal claimsPrincipal,
			UserManager userManager)
		{
			_claimsPrincipal = claimsPrincipal;
			_userManager = userManager;
		}

		public ModelExpression AspFor { get; set; }

		public bool DateOnly { get; set; }

		public DateTime ConvertedDateTime => (DateTime)AspFor.Model;

		public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
		{
			ValidateExpression();

			var user = await _userManager.GetUserAsync(_claimsPrincipal);

			var dateTime = ConvertedDateTime;
			if (DateOnly)
			{
				dateTime = dateTime.Date;
			}

			if (user != null)
			{
				try
				{
					var userTimeZone = TimeZoneInfo.FindSystemTimeZoneById(user.TimeZoneId);
					dateTime = TimeZoneInfo.ConvertTimeFromUtc(dateTime, userTimeZone);
				}
				catch
				{
					// TImeZoneInfo throws an exception if it can not find the timezone
					// Eat the exception and simply don't convert
				}
			}

			var dateStr = DateOnly
				? dateTime.ToShortDateString()
				: dateTime.ToString(CultureInfo.CurrentCulture);
			output.TagName = "span";
			output.TagMode = TagMode.StartTagAndEndTag;
			output.Content.AppendHtml(dateStr);
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
