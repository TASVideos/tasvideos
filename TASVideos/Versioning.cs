using System.Diagnostics;
using System.Reflection;

namespace TASVideos;

public static class Versioning
{
	private static readonly FileVersionInfo VersionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
	private static string Version => $"{VersionInfo.FileMajorPart}.{VersionInfo.FileMinorPart}.{(VersionInfo.ProductVersion ?? "").Split('+').Skip(1).First().Split('.').First()}";
	private static string Sha => (VersionInfo.ProductVersion ?? "").Split('+').Skip(1).First().Split('.').Last();

	public static (string Version, string Sha) GetVersion() => (Version, Sha);
}
