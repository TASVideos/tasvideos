using System.ComponentModel.DataAnnotations;
using TASVideos.RazorPages.Extensions;

namespace TASVideos.RazorPages.Models
{
	public class ValidWikiPageNameAttribute : ValidationAttribute
	{
		public ValidWikiPageNameAttribute()
		{
			ErrorMessage = "Invalid Wiki Page name.";
		}

		public override bool IsValid(object? value)
		{
			if (value is string str)
			{
				return WikiHelper.IsValidWikiPageName(str);
			}

			return false;
		}
	}
}
