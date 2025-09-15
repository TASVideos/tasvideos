using System.Reflection;

namespace TASVideos;

public static class Versioning
{
	private static readonly string Version = GetVersionString();
	private static readonly string Sha = GetSha();
	private static string GetVersionString() => $"2.6-{GetShortSha()}";

	public static (string Version, string Sha) GetVersion() => (Version, Sha);

	private static string GetShortSha()
	{
		try
		{
			var assembly = Assembly.GetExecutingAssembly();
			var meta = assembly.GetCustomAttributes<AssemblyMetadataAttribute>().ToList();
			var gitCommitShort = meta.FirstOrDefault(x => x.Key == "GitCommitShort")?.Value;
			return gitCommitShort ?? "unknown";
		}
		catch
		{
			return "unknown";
		}
	}

	private static string GetSha()
	{
		try
		{
			var gitCommitId = Assembly.GetExecutingAssembly()
				.GetCustomAttributes<AssemblyMetadataAttribute>()
				.FirstOrDefault(x => x.Key == "GitCommitId")?.Value;

			return !string.IsNullOrEmpty(gitCommitId) && gitCommitId != "unknown" ? gitCommitId : "";
		}
		catch
		{
			return "";
		}
	}
}
