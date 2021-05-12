using System;

namespace TASVideos.RazorPages.ViewComponents
{
	[AttributeUsage(AttributeTargets.Class)]
	public class WikiModuleAttribute : Attribute
	{
		public WikiModuleAttribute(string name)
		{
			Name = name;
		}

		public string Name { get; }
	}
}
