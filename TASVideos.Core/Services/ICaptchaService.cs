namespace TASVideos.Core.Services;

public interface ICaptchaService
{
	string ProviderName { get; }

	Task<object> GenerateChallengeAsync();

	Task<(bool IsValid, string FailureReason)> VerifyAsync(string responseBase64);
}
