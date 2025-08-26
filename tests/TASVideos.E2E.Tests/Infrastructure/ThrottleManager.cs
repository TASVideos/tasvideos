using TASVideos.E2E.Tests.Configuration;

namespace TASVideos.E2E.Tests.Infrastructure;

public class ThrottleManager
{
	private static readonly object Lock = new();
	private static DateTime _lastRequestTime = DateTime.MinValue;

	public static async Task WaitIfNeededAsync(E2ESettings settings)
	{
		if (!settings.IsProductionEnvironment())
		{
			return;
		}

		lock (Lock)
		{
			var timeSinceLastRequest = DateTime.UtcNow - _lastRequestTime;
			var minimumDelay = TimeSpan.FromMilliseconds(settings.ThrottleDelayMs);

			if (timeSinceLastRequest < minimumDelay)
			{
				var remainingWait = minimumDelay - timeSinceLastRequest;
				Thread.Sleep(remainingWait);
			}

			_lastRequestTime = DateTime.UtcNow;
		}
	}

	public static async Task ExecuteWithThrottleAsync(E2ESettings settings, Func<Task> action)
	{
		await WaitIfNeededAsync(settings);
		await action();
	}

	public static async Task<T> ExecuteWithThrottleAsync<T>(E2ESettings settings, Func<Task<T>> action)
	{
		await WaitIfNeededAsync(settings);
		return await action();
	}
}
