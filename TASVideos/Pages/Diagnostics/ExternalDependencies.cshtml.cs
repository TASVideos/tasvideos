using TASVideos.Core.Services.Youtube;
using TASVideos.Core.Settings;

namespace TASVideos.Pages.Diagnostics;

[RequirePermission(PermissionTo.SeeDiagnostics)]
public class ExternalDependenciesModel(IGoogleAuthService googleAuthService, AppSettings settings) : BasePageModel
{
	public bool YoutubeEnabled { get; set; }
	public bool? YoutubeAccessSuccessful { get; set; }
	public bool EmailEnabled { get; set; }
	public bool IrcEnabled { get; set; }
	public bool SecureIrcEnabled { get; set; }
	public bool DiscordEnabled { get; set; }
	public bool DiscordPrivateChannelEnabled { get; set; }

	public async Task OnGet()
	{
		YoutubeEnabled = googleAuthService.IsYoutubeEnabled();
		if (YoutubeEnabled)
		{
			try
			{
				YoutubeAccessSuccessful = !string.IsNullOrWhiteSpace(await googleAuthService.GetYoutubeAccessToken());
			}
			catch
			{
				// Do nothing;
			}
		}

		EmailEnabled = settings.Email.IsEnabled();
		IrcEnabled = settings.Irc.IsEnabled();
		SecureIrcEnabled = settings.Irc.IsSecureChannelEnabled();
		DiscordEnabled = settings.Discord.IsEnabled();
		DiscordPrivateChannelEnabled = settings.Discord.IsPrivateChannelEnabled();
	}
}
