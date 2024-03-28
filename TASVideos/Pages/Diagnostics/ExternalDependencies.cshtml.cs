using TASVideos.Core.Services.Youtube;
using TASVideos.Core.Settings;

namespace TASVideos.Pages.Diagnostics;

[RequirePermission(PermissionTo.SeeDiagnostics)]
public class ExternalDependenciesModel(IGoogleAuthService googleAuthService, AppSettings settings) : BasePageModel
{
	public ExternalDependencies Statuses { get; set; } = new();

	public async Task OnGet()
	{
		Statuses.YoutubeEnabled = googleAuthService.IsYoutubeEnabled();
		if (Statuses.YoutubeEnabled)
		{
			try
			{
				Statuses.YoutubeAccessSuccessful = !string.IsNullOrWhiteSpace(await googleAuthService.GetYoutubeAccessToken());
			}
			catch
			{
				// Do nothing;
			}
		}

		Statuses.EmailEnabled = settings.Email.IsEnabled();
		Statuses.IrcEnabled = settings.Irc.IsEnabled();
		Statuses.SecureIrcEnabled = settings.Irc.IsSecureChannelEnabled();
		Statuses.DiscordEnabled = settings.Discord.IsEnabled();
		Statuses.DiscordPrivateChannelEnabled = settings.Discord.IsPrivateChannelEnabled();
	}

	public class ExternalDependencies
	{
		public bool YoutubeEnabled { get; set; }
		public bool? YoutubeAccessSuccessful { get; set; }
		public bool EmailEnabled { get; set; }
		public bool IrcEnabled { get; set; }
		public bool SecureIrcEnabled { get; set; }
		public bool DiscordEnabled { get; set; }
		public bool DiscordPrivateChannelEnabled { get; set; }
	}
}
