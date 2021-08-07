using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TASVideos.Data.Entity;

namespace TASVideos.Core.Services.Youtube
{
	public interface IWikiToTextRenderer
	{
		Task<string> RenderWikiForYoutube(WikiPage page);
	}
}
