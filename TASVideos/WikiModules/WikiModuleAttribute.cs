namespace TASVideos.WikiModules;

[AttributeUsage(AttributeTargets.Class)]
public class WikiModuleAttribute(string name) : Attribute
{
	public string Name { get; } = name;
}

[AttributeUsage(AttributeTargets.Class)]
public class TextModuleAttribute : Attribute;

[AttributeUsage(AttributeTargets.Class)]
public class MetaDescriptionModuleAttribute : Attribute;
