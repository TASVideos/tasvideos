namespace TASVideos.Common;

public static class UriString
{
	/// <summary>determines whether a URI is "internal" (same domain, and therefore trusted) or "external"</summary>
	/// <remarks>
	/// will flag e.g. <c>https://tasvideos.org/page</c> as external<br/>
	/// could maybe use a constant which holds the domain name, but either way that should be stripped out by the UI
	/// </remarks>
	public static bool IsToExternalDomain(ReadOnlySpan<char> uri)
		=> uri.Length >= 1 && !(uri[0] is '#' || (uri[0] is '/' && (uri.Length < 2 || uri[1] is not '/')));
}
