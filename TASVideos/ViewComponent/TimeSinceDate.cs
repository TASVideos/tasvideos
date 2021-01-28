using System;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents
{
	[WikiModule(WikiModules.TimeSinceDate)]
	public class TimeSinceDate : ViewComponent
	{
		public IViewComponentResult Invoke(string pp)
		{
			var day = ParamHelper.GetInt(pp, "d");
			var month = ParamHelper.GetInt(pp, "m");
			var year = ParamHelper.GetInt(pp, "y");
			var outType = ParamHelper.GetValueFor(pp, "out");

			if (!day.HasValue
				|| !month.HasValue
				|| !year.HasValue
				|| string.IsNullOrWhiteSpace(outType))
			{
				return new ContentViewComponentResult("Error: missing parameters!");
			}

			var previousDate = new DateTime(year.Value, month.Value, day.Value);
			var nowDate = DateTime.UtcNow;

			string output = outType.ToLower() switch
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
}
