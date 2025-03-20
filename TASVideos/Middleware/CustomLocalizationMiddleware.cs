using System.Globalization;

namespace TASVideos.Middleware;

public class CustomLocalizationMiddleware(RequestDelegate next)
{
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
				cache.Set(CacheKeys.UsersWithCustomLocale, usersWithCustomLocale, Durations.OneYear);
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
							DecimalFormat = u.DecimalFormat
						})
						.SingleOrDefaultAsync();
					customCultureInfo = ConstructCustomCulture(customCultureData);
					cache.Set(CacheKeys.CustomUserLocalePrefix + userId, customCultureInfo, Durations.OneYear);
				}

				CultureInfo.CurrentCulture = customCultureInfo;
			}
		}

		await next(context);
	}

	private class CustomCultureData
	{
		public UserDateFormat DateFormat { get; init; }
		public UserTimeFormat TimeFormat { get; init; }
		public UserDecimalFormat DecimalFormat { get; init; }
	}

	private static CultureInfo ConstructCustomCulture(CustomCultureData? customCultureData)
	{
		if (customCultureData is null)
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
