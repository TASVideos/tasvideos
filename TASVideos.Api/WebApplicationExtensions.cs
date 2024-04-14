using Microsoft.AspNetCore.Builder;
using TASVideos.Api.Controllers;

namespace TASVideos.Api;
public static class WebApplicationExtensions
{
	public static WebApplication UseTasvideosApiEndpoints(this WebApplication app)
	{
		PublicationsApiMapper.Map(app);
		return app;
	}
}
