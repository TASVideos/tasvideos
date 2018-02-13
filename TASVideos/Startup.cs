using System;

using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Extensions;
using TASVideos.Filter;
using TASVideos.Legacy.Extensions;
using TASVideos.Services;

namespace TASVideos
{
	public class Startup
	{
		public Startup(IConfiguration configuration, IHostingEnvironment env)
		{
			Configuration = configuration;
			Environment = env;
		}

		public IConfiguration Configuration { get; }
		public IHostingEnvironment Environment { get; }

		public void ConfigureDevelopmentServices(IServiceCollection services)
		{
			services.ConfigureApplicationCookie(options =>
			{
				options.ExpireTimeSpan = TimeSpan.FromDays(90);
			});

			ConfigureServices(services);
		}

		public void ConfigureStagingServices(IServiceCollection services)
		{
			services.AddLegacyContext();
			ConfigureServices(services);
		}

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddDbContext<ApplicationDbContext>(options =>
				options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

			services.AddIdentity<User, Role>(config =>
				{
					config.SignIn.RequireConfirmedEmail = true;
					config.Password.RequiredLength = 12;
					config.Password.RequireDigit = false;
					config.Password.RequireLowercase = false;
					config.Password.RequireNonAlphanumeric = false;
					config.Password.RequiredUniqueChars = 4;
				})
				.AddEntityFrameworkStores<ApplicationDbContext>()
				.AddDefaultTokenProviders();

			// Add application services.
			services.AddTransient<IEmailSender, EmailSender>();

			services
				.AddTasks()
				.AddWikiProvider()
				.AddMovieParser();

			services.AddMvc(options =>
			{
				options.Filters.Add(new SetViewBagAttribute());
			});

			services.AddAutoMapper();

			// Sets up Dependency Injection for IPrinciple to be able to attain the user whereever we wish.
			services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
			services.AddTransient(
				provider => provider.GetService<IHttpContextAccessor>().HttpContext.User);
        }

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseBrowserLink();
				app.UseDatabaseErrorPage();
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
			}

			app.UseStaticFiles();

			app.UseAuthentication();

			app.UseMvc(routes =>
			{
				routes.MapRoute("default", "{controller=Home}/{action=Index}/{id?}");
				routes.MapRoute("sub-list", "Subs-List", defaults: new { controller = "Submission", action = "List" });
				routes.MapRoute("submission", "{id:int}S", defaults: new { controller = "Submission", action = "View" });
				routes.MapRoute("movie", "{id:int}M", defaults: new { controller = "Publication", action = "View" });
				routes.MapRoute("wiki", "{*url}", defaults: new { controller = "Wiki", action = "RenderWikiPage" });
			});
		}
	}
}
