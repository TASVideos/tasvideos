using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents;

[WikiModule(WikiModules.TimeSinceDate)]
public class TimeSinceDate : ViewComponent
{
	public IViewComponentResult Invoke(int d, int m, int y, string @out)
	{
		var previousDate = new DateTime(y, m, d);
		var nowDate = DateTime.UtcNow;

		string output = @out.ToLowerInvariant() switch
		{
			"days" => ((int)(nowDate - previousDate).TotalDays).ToString(CultureInfo.CurrentCulture),
			"years" => GetDifferenceInYears(previousDate, nowDate).ToString(CultureInfo.CurrentCulture),
			_ => "Error: Invalid out parameter!"
		};

		return new ContentViewComponentResult(output);
	}

	// https://stackoverflow.com/questions/4127363/date-difference-in-years-using-c-sharp
	private static int GetDifferenceInYears(DateTime startDate, DateTime endDate)
	{
		// Excel documentation says "COMPLETE calendar years in between dates"
		int years = endDate.Year - startDate.Year;

		if (startDate.Month == endDate.Month && // if the start month and the end month are the same
			endDate.Day < startDate.Day // AND the end day is less than the start day
			|| endDate.Month < startDate.Month) // OR if the end month is less than the start month
		{
			years--;
		}

		return years;
	}
}
