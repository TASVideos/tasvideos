using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using TASVideos.Data.Entity;

namespace TASVideos.ViewComponents
{
	public class Welcome : ViewComponent
	{
		public IViewComponentResult Invoke(WikiPage pageData, string pp)
		{
			return View();
		}
	}
}
