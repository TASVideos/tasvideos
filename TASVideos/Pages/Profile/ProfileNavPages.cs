﻿using System;

using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace TASVideos.Pages.Profile;

public static class ProfileNavPages
{
	public static string ActivePageKey => "ActivePage";

	public static string Index => "Index";
	public static string IndexNavClass(ViewContext viewContext) => PageNavClass(viewContext, Index);

	public static string Settings => "Settings";
	public static string SettingsNavClass(ViewContext viewContext) => PageNavClass(viewContext, Settings);

	public static string ChangePassword => "ChangePassword";
	public static string ChangePasswordNavClass(ViewContext viewContext) => PageNavClass(viewContext, ChangePassword);

	public static string HomePage => "HomePage";
	public static string HomePageNavClass(ViewContext viewContext) => PageNavClass(viewContext, HomePage);

	public static string UserFiles => "UserFiles";
	public static string UserFilesNavClass(ViewContext viewContext) => PageNavClass(viewContext, UserFiles);

	public static string Ratings => "Ratings";
	public static string RatingsNavClass(ViewContext viewContext) => PageNavClass(viewContext, Ratings);

	public static string Unrated => "Unrated";
	public static string UnratedNavClass(ViewContext viewContext) => PageNavClass(viewContext, Unrated);

	public static string Topics => "Topics";
	public static string TopicsNavClass(ViewContext viewContext) => PageNavClass(viewContext, Topics);

	public static string PageNavClass(ViewContext viewContext, string page)
	{
		var activePage = viewContext.ViewData["ActivePage"] as string;
		return string.Equals(activePage, page, StringComparison.OrdinalIgnoreCase) ? "active" : "";
	}

	public static void AddActivePage(this ViewDataDictionary viewData, string activePage) => viewData[ActivePageKey] = activePage;
}
