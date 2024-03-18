using TASVideos.Core;

namespace TASVideos.Pages.UserFiles.Models;

public class UserFileListRequest : PagingModel
{
	public UserFileListRequest()
	{
		PageSize = 50;
		Sort = $"-{nameof(UserFileListModel.UploadTimestamp)}";
	}
}
