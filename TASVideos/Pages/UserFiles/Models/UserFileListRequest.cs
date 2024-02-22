using TASVideos.Core;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.UserFiles.Models;

public class UserFileListRequest : PagingModel
{
	public UserFileListRequest()
	{
		PageSize = 50;
		Sort = $"-{nameof(UserFileListModel.UploadTimestamp)}";
	}
}
