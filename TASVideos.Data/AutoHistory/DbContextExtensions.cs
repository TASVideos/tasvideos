using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace TASVideos.Data.AutoHistory;
public static class DbContextExtensions
{
	public static void EnsureAutoHistory<TAutoHistory>(this DbContext context, Func<TAutoHistory> createHistoryFactory)
		where TAutoHistory : AutoHistoryEntry
	{
		// Must ToArray() here for excluding the AutoHistory model.
		// Currently, only support Modified and Deleted entity.
		var entries = context.ChangeTracker.Entries()
			.Where(e => e.State == EntityState.Modified || e.State == EntityState.Deleted)
			.ToArray();
		foreach (var entry in entries)
		{
			var autoHistory = entry.AutoHistory(createHistoryFactory);
			if (autoHistory != null)
			{
				context.Add(autoHistory);
			}
		}
	}

	internal static TAutoHistory? AutoHistory<TAutoHistory>(this EntityEntry entry, Func<TAutoHistory> createHistoryFactory)
		where TAutoHistory : AutoHistoryEntry
	{
		if (!IsEntityIncluded(entry))
		{
			return null;
		}

		var properties = GetPropertiesWithoutExcluded(entry).ToArray();
		if (!(properties.Any(p => p.IsModified) || entry.State == EntityState.Deleted))
		{
			return null;
		}

		var history = createHistoryFactory();
		history.TableName = entry.Metadata.GetTableName() ?? "";
		switch (entry.State)
		{
			case EntityState.Added:
				WriteHistoryAddedState(history, properties);
				break;
			case EntityState.Modified:
				WriteHistoryModifiedState(history, entry, properties);
				break;
			case EntityState.Deleted:
				WriteHistoryDeletedState(history, entry, properties);
				break;
			case EntityState.Detached:
			case EntityState.Unchanged:
			default:
				throw new NotSupportedException("AutoHistory only support Deleted and Modified entity.");
		}

		return history;
	}

	private static bool IsEntityIncluded(EntityEntry entry) =>
		entry.Metadata.ClrType.GetCustomAttributes(typeof(IncludeInAutoHistoryAttribute), true).Any();

	private static IEnumerable<PropertyEntry> GetPropertiesWithoutExcluded(EntityEntry entry)
	{
		// Get the mapped properties for the entity type.
		// (include shadow properties, not include navigations & references)
		var excludedProperties = entry.Metadata.ClrType.GetProperties()
				.Where(p => p.GetCustomAttributes(typeof(ExcludeFromAutoHistoryAttribute), true).Length > 0)
				.Select(p => p.Name);

		var properties = entry.Properties.Where(f => !excludedProperties.Contains(f.Metadata.Name));
		return properties;
	}

	public static void EnsureAddedHistory<TAutoHistory>(
		this DbContext context,
		Func<TAutoHistory> createHistoryFactory,
		EntityEntry[] addedEntries)
		where TAutoHistory : AutoHistoryEntry
	{
		foreach (var entry in addedEntries)
		{
			var autoHistory = entry.AddedHistory(createHistoryFactory);
			if (autoHistory != null)
			{
				context.Add(autoHistory);
			}
		}
	}

	internal static TAutoHistory? AddedHistory<TAutoHistory>(
		this EntityEntry entry,
		Func<TAutoHistory> createHistoryFactory)
		where TAutoHistory : AutoHistoryEntry
	{
		if (!IsEntityIncluded(entry))
		{
			return null;
		}

		var properties = GetPropertiesWithoutExcluded(entry);

		var json = new Dictionary<string, object?>();
		foreach (var prop in properties)
		{
			json[prop.Metadata.Name] = prop?.OriginalValue;
		}

		var history = createHistoryFactory();
		history.TableName = entry.Metadata.GetTableName() ?? "";
		history.RowId = entry.PrimaryKey();
		history.Kind = EntityState.Added;
		history.Changed = JsonSerializer.Serialize(json);
		return history;
	}

	private static string PrimaryKey(this EntityEntry entry)
	{
		var key = entry.Metadata.FindPrimaryKey();

		if (key == null)
		{
			return "";
		}

		var values = new List<object>();
		foreach (var property in key.Properties)
		{
			var value = entry.Property(property.Name).CurrentValue;
			if (value != null)
			{
				values.Add(value);
			}
		}

		return string.Join(",", values);
	}

	private static void WriteHistoryAddedState(AutoHistoryEntry history, IEnumerable<PropertyEntry> properties)
	{
		var json = new Dictionary<string, object?>();

		foreach (var prop in properties)
		{
			if (prop.Metadata.IsKey() || prop.Metadata.IsForeignKey())
			{
				continue;
			}

			json[prop.Metadata.Name] = prop?.CurrentValue;
		}

		// REVIEW: what's the best way to set the RowId?
		history.RowId = "0";
		history.Kind = EntityState.Added;
		history.Changed = JsonSerializer.Serialize(json);
	}

	private static void WriteHistoryModifiedState(AutoHistoryEntry history, EntityEntry entry, IEnumerable<PropertyEntry> properties)
	{
		var json = new Dictionary<string, object?>();
		var bef = new Dictionary<string, object?>();
		var aft = new Dictionary<string, object?>();

		PropertyValues? databaseValues = null;
		foreach (var prop in properties)
		{
			if (prop.IsModified)
			{
				if (prop.OriginalValue != null)
				{
					if (!prop.OriginalValue.Equals(prop.CurrentValue))
					{
						bef[prop.Metadata.Name] = prop.OriginalValue;
					}
					else
					{
						databaseValues ??= entry.GetDatabaseValues();
						var originalValue = databaseValues?.GetValue<object>(prop.Metadata.Name);
						bef[prop.Metadata.Name] = originalValue;
					}
				}
				else
				{
					bef[prop.Metadata.Name] = null;
				}

				aft[prop.Metadata.Name] = prop.CurrentValue;
			}
		}

		json["before"] = bef;
		json["after"] = aft;

		history.RowId = entry.PrimaryKey();
		history.Kind = EntityState.Modified;
		history.Changed = JsonSerializer.Serialize(json);
	}

	private static void WriteHistoryDeletedState(AutoHistoryEntry history, EntityEntry entry, IEnumerable<PropertyEntry> properties)
	{
		var json = new Dictionary<string, object?>();

		foreach (var prop in properties)
		{
			json[prop.Metadata.Name] = prop.OriginalValue;
		}

		history.RowId = entry.PrimaryKey();
		history.Kind = EntityState.Deleted;
		history.Changed = JsonSerializer.Serialize(json);
	}
}
