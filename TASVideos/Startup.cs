using AspNetCore.ReCaptcha;
using TASVideos.Core;
using TASVideos.Core.Settings;
using TASVideos.Data;
using TASVideos.Middleware;
using TASVideos.Services;

namespace TASVideos;

public class Startup
{
	public Startup(IConfiguration configuration, IWebHostEnvironment env)
	{
		Configuration = configuration;
		Environment = env;
	}

	public IConfiguration Configuration { get; }
	public IWebHostEnvironment Environment { get; }

	public void ConfigureServices(IServiceCollection services)
	{
	}

	public void Configure(IApplicationBuilder app, IHostEnvironment env)
	{
	}
}
