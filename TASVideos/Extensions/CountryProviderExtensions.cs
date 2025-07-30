using Nager.Country;
using TASVideos.Pages.PublicationClasses;

namespace TASVideos.Extensions;

public static class CountryProviderExtensions
{
	public static string ToEmoji(this ICountryInfo c)
	{
		return string.Concat(c.Alpha2Code.ToString().ToUpper().Select(ch => char.ConvertFromUtf32(0x1F1E6 - 'A' + ch))); // please don't ban me
	}

	public static string ToSelectListText(this ICountryInfo c)
	{
		return $"{c.ToEmoji()} {(c.CommonName == c.NativeName ? c.CommonName : $"{c.CommonName} ({c.NativeName})")}";
	}

	public static string ToSelectListValue(this ICountryInfo c)
	{
		return $"{c.ToEmoji()} {c.CommonName}";
	}

	public static bool IsCountryListValue(this string s)
	{
		return CountryList.ItemSet.Contains(s);
	}
}

public static class CountryList
{
	public static List<SelectListItem> Items = new CountryProvider()
		.GetCountries()
		.OrderBy(c => c.CommonName)
		.Select(c => new SelectListItem()
		{
			Text = c.ToSelectListText(),
			Value = c.ToSelectListValue()
		})
		.ToList();

	public static HashSet<string> ItemSet = Items.Select(c => c.Value).ToHashSet();
}
