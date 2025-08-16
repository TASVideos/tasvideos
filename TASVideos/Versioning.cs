using System.Diagnostics;
using System.Reflection;

namespace TASVideos;

public static class Versioning
{
	private static readonly FileVersionInfo VersionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
	private static readonly string Version = GetVersionString();
	private static readonly string Sha = GetShaString();

	private static string GetVersionString()
	{
		try
		{
			var productVersion = VersionInfo.ProductVersion ?? "";

			// GitVersion v6 format: InformationalVersion contains the full version with build metadata
			// e.g., "2.6.1-git-version-update.1+391.Branch.git-version-update.Sha.e2f009c7e95aa93b732b9c7d59c00fcd30fbd0b4"
			if (productVersion.Contains('+'))
			{
				var parts = productVersion.Split('+');
				var versionPart = parts[0]; // e.g., "2.6.1-git-version-update.1"
				var buildMetadata = parts[1]; // e.g., "391.Branch.git-version-update.Sha.e2f009c7e95aa93b732b9c7d59c00fcd30fbd0b4"

				// Extract the base version (major.minor.patch) from the version part
				var baseVersion = versionPart.Contains('-')
					? versionPart.Split('-')[0]
					: versionPart;

				// Extract commit count from build metadata (first number in build metadata)
				var buildParts = buildMetadata.Split('.');
				if (buildParts.Length > 0 && int.TryParse(buildParts[0], out var commitCount))
				{
					// Extract major.minor from base version
					var versionNumbers = baseVersion.Split('.');
					if (versionNumbers.Length >= 2)
					{
						return $"{versionNumbers[0]}.{versionNumbers[1]}.{commitCount}";
					}
				}

				// Fallback to just the base version if we can't parse commit count
				var fallbackVersionNumbers = baseVersion.Split('.');
				if (fallbackVersionNumbers.Length >= 2)
				{
					return $"{fallbackVersionNumbers[0]}.{fallbackVersionNumbers[1]}.0";
				}
			}

			// Final fallback to assembly version
			return $"{VersionInfo.FileMajorPart}.{VersionInfo.FileMinorPart}.{VersionInfo.FileBuildPart}";
		}
		catch
		{
			return $"{VersionInfo.FileMajorPart}.{VersionInfo.FileMinorPart}.{VersionInfo.FileBuildPart}";
		}
	}

	private static string GetShaString()
	{
		try
		{
			var productVersion = VersionInfo.ProductVersion ?? "";

			if (productVersion.Contains('+') && productVersion.Contains("Sha."))
			{
				var parts = productVersion.Split('+');
				if (parts.Length > 1)
				{
					var buildMetadata = parts[1];
					var shaIndex = buildMetadata.IndexOf("Sha.");
					if (shaIndex >= 0)
					{
						var shaString = buildMetadata[(shaIndex + 4)..]; // "Sha."
						return shaString.Length >= 7 ? shaString[..7] : shaString;
					}
				}
			}

			return "unknown";
		}
		catch
		{
			return "unknown";
		}
	}

	public static (string Version, string Sha) GetVersion() => (Version, Sha);
}
