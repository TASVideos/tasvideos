using System.ComponentModel.DataAnnotations;

namespace TASVideos.Models.ValidationAttributes;

// https://forums.asp.net/t/2000494.aspx?How+do+I+require+a+checkbox+to+be+checked+
[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public class MustBeTrueAttribute : ValidationAttribute
{
	public override bool IsValid(object? value)
	{
		return value is true;
	}
}
