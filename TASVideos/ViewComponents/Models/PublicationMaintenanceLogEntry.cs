namespace TASVideos.ViewComponents;

public record PublicationMaintenanceLogEntry(string Log, string UserName, DateTime Timestamp);
public record ParentPublicationMaintenanceEntry(int Id, string Title);
