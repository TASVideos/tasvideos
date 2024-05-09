namespace TASVideos.ModelBinding;

public class ValidWikiPageNameAttribute : ValidationAttribute
{
	public ValidWikiPageNameAttribute()
	{
		ErrorMessage = "Invalid Wiki Page name.";
	}

	public override bool IsValid(object? value)
	{
		return value is string str && WikiHelper.IsValidWikiPageName(str);
	}
}
