using TASVideos.E2E.Tests.Configuration;

namespace TASVideos.E2E.Tests.Infrastructure;

public static class ThrottleManager
{
	private static readonly object Lock = new();
	private static DateTime _lastRequestTime = DateTime.MinValue;

	public static async Task WaitIfNeededAsync(E2ESettings settings)
	{
		if (!settings.IsProductionEnvironment())
		{
			return;
		}

		TimeSpan remainingWait;
		lock (Lock)
		{
			var timeSinceLastRequest = DateTime.UtcNow - _lastRequestTime;
			var minimumDelay = TimeSpan.FromMilliseconds(settings.ThrottleDelayMs);

			if (timeSinceLastRequest < minimumDelay)
			{
				remainingWait = minimumDelay - timeSinceLastRequest;
			}
			else
			{
				remainingWait = TimeSpan.Zero;
			}

			_lastRequestTime = DateTime.UtcNow;
		}

		if (remainingWait > TimeSpan.Zero)
		{
			await Task.Delay(remainingWait);
		}
	}

	public static async Task<T> ExecuteWithThrottleAsync<T>(E2ESettings settings, Func<Task<T>> action)
	{
		await WaitIfNeededAsync(settings);
		return await action();
	}
}
