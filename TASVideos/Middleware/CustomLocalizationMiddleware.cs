﻿using System.Globalization;
using TASVideos.Core.Services;
using TASVideos.Data;
using TASVideos.Data.Entity;

namespace TASVideos.Middleware;

public class CustomLocalizationMiddleware
{
	private readonly RequestDelegate _next;

	public CustomLocalizationMiddleware(RequestDelegate next)
	{
		_next = next;
	}

	public async Task Invoke(HttpContext context, ApplicationDbContext db, ICacheService cache)
	{
		if (context.User.IsLoggedIn())
		{
			if (!cache.TryGetValue(CacheKeys.UsersWithCustomLocale, out HashSet<string> usersWithCustomLocale))
			{
				usersWithCustomLocale = (await db.Users
					.ThatHaveCustomLocale()
					.Select(u => u.UserName)
					.ToListAsync())
					.ToHashSet();
				cache.Set(CacheKeys.UsersWithCustomLocale, usersWithCustomLocale, Durations.OneYearInSeconds);
			}

			if (usersWithCustomLocale.Contains(context.User.Name()))
			{
				int userId = context.User.GetUserId();

				if (!cache.TryGetValue(CacheKeys.CustomUserLocalePrefix + userId, out CultureInfo customCultureInfo))
				{
					CustomCultureData? customCultureData = await db.Users
						.Where(u => u.Id == userId)
						.Select(u => new CustomCultureData
						{
							DateFormat = u.DateFormat,
							TimeFormat = u.TimeFormat,
							DecimalFormat = u.DecimalFormat,
						})
						.SingleOrDefaultAsync();
					customCultureInfo = ConstructCustomCulture(customCultureData);
					cache.Set(CacheKeys.CustomUserLocalePrefix + userId, customCultureInfo, Durations.OneYearInSeconds);
				}

				CultureInfo.CurrentCulture = customCultureInfo;
			}
		}

		await _next(context);
	}

	private class CustomCultureData
	{
		public UserDateFormat DateFormat { get; set; }
		public UserTimeFormat TimeFormat { get; set; }
		public UserDecimalFormat DecimalFormat { get; set; }
	}

	private CultureInfo ConstructCustomCulture(CustomCultureData? customCultureData)
	{
		if (customCultureData == null)
		{
			return CultureInfo.CurrentCulture;
		}

		CultureInfo cultureInfo = (CultureInfo)CultureInfo.CurrentCulture.Clone(); // clone to make it editable
		switch (customCultureData.DateFormat)
		{
			default:
			case UserDateFormat.Auto:
				break;
			case UserDateFormat.YMMDD:
				cultureInfo.DateTimeFormat.ShortDatePattern = "yyyy-MM-dd";
				break;
			case UserDateFormat.DDMMY:
				cultureInfo.DateTimeFormat.ShortDatePattern = @"dd\/MM\/yyyy";
				break;
			case UserDateFormat.DDMMYDot:
				cultureInfo.DateTimeFormat.ShortDatePattern = "dd.MM.yyyy";
				break;
			case UserDateFormat.DMY:
				cultureInfo.DateTimeFormat.ShortDatePattern = @"d\/M\/yyyy";
				break;
			case UserDateFormat.MMDDY:
				cultureInfo.DateTimeFormat.ShortDatePattern = @"MM\/dd\/yyyy";
				break;
			case UserDateFormat.MDY:
				cultureInfo.DateTimeFormat.ShortDatePattern = @"M\/d\/yyyy";
				break;
		}

		switch (customCultureData.TimeFormat)
		{
			default:
			case UserTimeFormat.Auto:
				break;
			case UserTimeFormat.Clock24Hour:
				cultureInfo.DateTimeFormat.ShortTimePattern = @"HH\:mm";
				cultureInfo.DateTimeFormat.LongTimePattern = @"HH\:mm\:ss";
				break;
			case UserTimeFormat.Clock12Hour:
				cultureInfo.DateTimeFormat.AMDesignator = "AM";
				cultureInfo.DateTimeFormat.PMDesignator = "PM";
				cultureInfo.DateTimeFormat.ShortTimePattern = @"h\:mm tt";
				cultureInfo.DateTimeFormat.LongTimePattern = @"h\:mm\:ss tt";
				break;
		}

		switch (customCultureData.DecimalFormat)
		{
			default:
			case UserDecimalFormat.Auto:
				break;
			case UserDecimalFormat.Dot:
				cultureInfo.NumberFormat.NumberDecimalSeparator = ".";
				cultureInfo.NumberFormat.NumberGroupSeparator = ",";
				break;
			case UserDecimalFormat.Comma:
				cultureInfo.NumberFormat.NumberDecimalSeparator = ",";
				cultureInfo.NumberFormat.NumberGroupSeparator = ".";
				break;
		}

		return cultureInfo;
	}
}
