using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace TASVideos.Models.ValidationAttributes;

public class HexNumberAttribute : ValidationAttribute
{
	public override bool IsValid(object? value)
	{
		if (value is string str)
		{
			return long.TryParse(str, NumberStyles.HexNumber, null, out long _);
		}

		return false;
	}
}
