using Microsoft.EntityFrameworkCore;

namespace TASVideos.Data;

internal class CustomAutoHistory : AutoHistory
{
	public int UserId { get; set; }
}
