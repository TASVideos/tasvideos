using System;

using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace TASVideos.Views.Profile
{
	public static class ProfileNavPages
	{
		public static string ActivePageKey => "ActivePage";

		public static string Index => "Index";

		public static string ChangePassword => "ChangePassword";

		public static string HomePage => "HomePage";

		public static string Settings => "Settings";

		public static string IndexNavClass(ViewContext viewContext) => PageNavClass(viewContext, Index);

		public static string ChangePasswordNavClass(ViewContext viewContext) => PageNavClass(viewContext, ChangePassword);

		public static string HomePageNavClass(ViewContext viewContext) => PageNavClass(viewContext, HomePage);

		public static string SettingsNavClass(ViewContext viewContext) => PageNavClass(viewContext, Settings);

		public static string PageNavClass(ViewContext viewContext, string page)
		{
			var activePage = viewContext.ViewData["ActivePage"] as string;
			return string.Equals(activePage, page, StringComparison.OrdinalIgnoreCase) ? "active" : null;
		}

		public static void AddActivePage(this ViewDataDictionary viewData, string activePage) => viewData[ActivePageKey] = activePage;
	}
}
