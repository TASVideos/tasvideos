using System.Globalization;

namespace TASVideos.ModelBinding;

public sealed class RatingString : ValidationAttribute
{
	public static double? AsRatingDouble(string? ratingString)
	{
		if (string.IsNullOrWhiteSpace(ratingString))
		{
			return null;
		}

		NumberFormatInfo customFormat = new CultureInfo("en-US").NumberFormat;
		customFormat.NumberDecimalSeparator = ".";
		var result = double.TryParse(ratingString, NumberStyles.AllowDecimalPoint, customFormat, out double ratingNumber);
		if (!result)
		{
			customFormat.NumberDecimalSeparator = ",";
			result = double.TryParse(ratingString, NumberStyles.AllowDecimalPoint, customFormat, out ratingNumber);
			if (!result)
			{
				return null;
			}
		}

		ratingNumber = Math.Round(ratingNumber, 1, MidpointRounding.AwayFromZero);

		return ratingNumber;
	}

	public override bool IsValid(object? value)
	{
		var ratingString = value as string;
		if (string.IsNullOrWhiteSpace(ratingString))
		{
			return true;
		}

		var ratingNumber = AsRatingDouble(ratingString);
		return ratingNumber is >= 0.0 and <= 10.0;
	}
}
