namespace TASVideos.Data.AutoHistory;

[AttributeUsage(AttributeTargets.Class)]
public class IncludeInAutoHistoryAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Property)]
public class ExcludeFromAutoHistoryAttribute : Attribute { }
