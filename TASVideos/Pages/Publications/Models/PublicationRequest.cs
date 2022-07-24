using TASVideos.Core;

namespace TASVideos.Pages.Publications.Models;

public class PublicationRequest : PagingModel
{
	public PublicationRequest()
	{
		PageSize = 100;
	}
}
