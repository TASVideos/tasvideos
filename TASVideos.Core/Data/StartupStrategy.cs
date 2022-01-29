namespace TASVideos.Core.Data;

public enum StartupStrategy
{
	/// <summary>
	/// Does nothing. Simply checks if the database exists.
	/// No data is imported, schema is not created nor validated
	/// </summary>
	Minimal,

	/// <summary>
	/// Deletes and recreates the database,
	/// Populates required seed data,
	/// Generates minimal sample data
	/// </summary>
	Sample,

	/// <summary>
	/// Runs database migrations.  If no database exists,
	/// it will be created, and schema updated to latest migrations.
	/// No Seed data will be generated, no import will be run, nor
	/// any sample data.
	/// </summary>
	Migrate
}
