using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace TASVideos.Services.ExternalMediaPublisher.Distributors
{
	public class IrcDistributor : IPostDistributor
	{
		private readonly AppSettings _appSettings;

		public IrcDistributor(IOptions<AppSettings> options)
		{
			_appSettings = options.Value;

			// TODO: put in app settings
			_appSettings.GeneralIrc.Server = "irc.freenode.net";
			_appSettings.GeneralIrc.Channel = "#tasvideosdevirc";
			_appSettings.GeneralIrc.BotName = "TASVideosAgentDev";
		}
		
		public IEnumerable<PostType> Types => new[] { PostType.General, PostType.Announcement };

		public void Post(IPostable post)
		{
			// TODO
			int zzz = 0;
		}
	}
}
