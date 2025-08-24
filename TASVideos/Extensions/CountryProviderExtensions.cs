using Nager.Country;

namespace TASVideos.Extensions;

public static class CountryProviderExtensions
{
	public static string ToEmoji(this ICountryInfo c)
		=> string.Concat(c.Alpha2Code.ToString().ToUpper().Select(ch => char.ConvertFromUtf32(0x1F1E6 - 'A' + ch))); // please don't ban me

	public static string ToSelectListText(this ICountryInfo c)
		=> $"{c.ToEmoji()} {(c.CommonName == c.NativeName ? c.CommonName : $"{c.CommonName} ({c.NativeName})")}";

	public static string ToSelectListValue(this ICountryInfo c) => $"{c.ToEmoji()} {c.CommonName}";

	public static bool IsCountryListValue(this string s) => CountryList.ItemSet.Contains(s);
}

public static class CountryList
{
	public static readonly List<SelectListItem> Items = [.. new CountryProvider()
		.GetCountries()
		.OrderBy(c => c.CommonName)
		.Select(c => new SelectListItem()
		{
			Text = c.ToSelectListText(),
			Value = c.ToSelectListValue()
		})];

	public static HashSet<string> ItemSet = [.. Items.Select(c => c.Value)];
}
