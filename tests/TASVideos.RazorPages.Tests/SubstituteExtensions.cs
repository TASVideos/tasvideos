namespace TASVideos.RazorPages.Tests;

public static class ConfigureSubstitute
{
	public static T For<T>(Action<T> configure)
		where T : class
	{
		var mock = Substitute.For<T>();
		configure(mock);
		return mock;
	}
}
