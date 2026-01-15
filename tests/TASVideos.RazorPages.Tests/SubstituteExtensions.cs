using Microsoft.Extensions.Options;

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

	public static IOptions<T> ForIOptions<T>(T? optionsObj = null)
		where T : class, new()
	{
		optionsObj ??= new();
		return For<IOptions<T>>(mock => _ = mock.Value.Returns(optionsObj));
	}
}
