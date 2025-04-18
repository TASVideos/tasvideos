﻿using TASVideos.Data.Entity;
using TASVideos.MovieParsers;

namespace TASVideos.Core.Tests.Services;

[TestClass]
public class MovieFormatDeprecatorTests : TestDbBase
{
	private readonly MovieFormatDeprecator _deprecator;
	private readonly IMovieParser _mockParser;

	public MovieFormatDeprecatorTests()
	{
		_mockParser = Substitute.For<IMovieParser>();
		_deprecator = new MovieFormatDeprecator(_db, _mockParser);
	}

	#region GetAll

	[TestMethod]
	public async Task GetAll_NoEntries_ReturnsEmptyList()
	{
		_mockParser.SupportedMovieExtensions.Returns([]);

		var actual = await _deprecator.GetAll();
		Assert.IsNotNull(actual);
		Assert.AreEqual(0, actual.Count);
	}

	[TestMethod]
	public async Task GetAll_Parsers_ButNoDbEntries_ReturnsAllParsers()
	{
		var formats = new[] { ".test1", ".test2", ".test3" };
		_mockParser.SupportedMovieExtensions.Returns(formats);

		var actual = await _deprecator.GetAll();
		Assert.IsNotNull(actual);
		Assert.AreEqual(formats.Length, actual.Count);
		foreach (string format in formats)
		{
			Assert.IsTrue(actual.ContainsKey(format));
			var entry = actual[format];
			Assert.IsNull(entry);
		}
	}

	[TestMethod]
	public async Task GetAll_Parsers_SomeDbEntries_ReturnsAllParsers()
	{
		const string existsAndDeprecated = ".test1";
		const string existsAndAllowed = ".test2";
		const string notExists = ".test3";
		var formats = new[] { existsAndDeprecated, existsAndAllowed, notExists };
		_mockParser.SupportedMovieExtensions.Returns(formats);

		_db.DeprecatedMovieFormats.Add(new DeprecatedMovieFormat
		{
			FileExtension = existsAndDeprecated,
			Deprecated = true
		});
		_db.DeprecatedMovieFormats.Add(new DeprecatedMovieFormat
		{
			FileExtension = existsAndAllowed,
			Deprecated = false
		});
		await _db.SaveChangesAsync();

		var actual = await _deprecator.GetAll();

		Assert.IsNotNull(actual);
		Assert.AreEqual(formats.Length, actual.Count);

		Assert.IsTrue(actual.ContainsKey(existsAndDeprecated));
		Assert.IsNotNull(actual[existsAndDeprecated]);
		Assert.IsTrue(actual[existsAndDeprecated]!.Deprecated);

		Assert.IsTrue(actual.ContainsKey(existsAndAllowed));
		Assert.IsNotNull(actual[existsAndAllowed]);
		Assert.IsFalse(actual[existsAndAllowed]!.Deprecated);

		Assert.IsTrue(actual.ContainsKey(notExists));
		Assert.IsNull(actual[notExists]);
	}

	#endregion

	#region IsMovieExtension

	[TestMethod]
	public void IsMovieExtension_Exists_ReturnsTrue()
	{
		const string existingExtension = ".test";
		_mockParser.SupportedMovieExtensions.Returns([existingExtension]);

		var actual = _deprecator.IsMovieExtension(existingExtension);
		Assert.IsTrue(actual);
	}

	[TestMethod]
	public void IsMovieExtension_NotExists_ReturnsFalse()
	{
		const string existingExtension = ".test";
		_mockParser.SupportedMovieExtensions.Returns([existingExtension]);

		var actual = _deprecator.IsMovieExtension("not exists");
		Assert.IsFalse(actual);
	}

	[TestMethod]
	public void IsMovieExtension_CaseSensitive()
	{
		const string existingExtension = ".TEST";
		_mockParser.SupportedMovieExtensions.Returns([existingExtension]);

		var actual = _deprecator.IsMovieExtension(existingExtension.ToLower());
		Assert.IsFalse(actual);
	}

	#endregion

	#region IsDeprecated

	[TestMethod]
	public async Task IsDeprecated_ReturnsFalse_IfNoEntry()
	{
		var actual = await _deprecator.IsDeprecated("does not exist");
		Assert.IsFalse(actual);
	}

	[TestMethod]
	public async Task IsDeprecated_ReturnsFalse_IfFalseEntry()
	{
		const string ext = "test";
		_db.DeprecatedMovieFormats.Add(new DeprecatedMovieFormat
		{
			FileExtension = ext,
			Deprecated = false
		});
		await _db.SaveChangesAsync();

		var actual = await _deprecator.IsDeprecated(ext);
		Assert.IsFalse(actual);
	}

	[TestMethod]
	public async Task IsDeprecated_ReturnsTrue_IfTrueEntry()
	{
		const string ext = "test";
		_db.DeprecatedMovieFormats.Add(new DeprecatedMovieFormat
		{
			FileExtension = ext,
			Deprecated = true
		});
		await _db.SaveChangesAsync();

		var actual = await _deprecator.IsDeprecated(ext);
		Assert.IsTrue(actual);
	}

	#endregion

	#region Allow

	[TestMethod]
	public async Task Allow_InvalidFormat_ReturnsFalse()
	{
		_mockParser.SupportedMovieExtensions.Returns([".test1"]);

		var actual = await _deprecator.Allow("invalid");
		Assert.IsFalse(actual);
		Assert.AreEqual(0, _db.DeprecatedMovieFormats.Count());
	}

	[TestMethod]
	public async Task Allow_ConcurrentUpdateConflict_ReturnsFalse()
	{
		const string existingFormat = ".test1";
		_mockParser.SupportedMovieExtensions.Returns([existingFormat]);
		_db.DeprecatedMovieFormats.Add(new DeprecatedMovieFormat
		{
			FileExtension = existingFormat,
			Deprecated = true
		});
		await _db.SaveChangesAsync();
		_db.CreateConcurrentUpdateConflict();

		var actual = await _deprecator.Allow(existingFormat);
		Assert.IsFalse(actual);
	}

	[TestMethod]
	public async Task Allow_UpdateConflict_ReturnsFalse()
	{
		const string existingFormat = ".test1";
		_mockParser.SupportedMovieExtensions.Returns([existingFormat]);
		_db.DeprecatedMovieFormats.Add(new DeprecatedMovieFormat
		{
			FileExtension = existingFormat,
			Deprecated = true
		});
		await _db.SaveChangesAsync();
		_db.CreateUpdateConflict();

		var actual = await _deprecator.Allow(existingFormat);
		Assert.IsFalse(actual);
	}

	[TestMethod]
	public async Task Allow_NoDbRecord_ReturnsTrue()
	{
		const string existingFormat = ".test1";
		_mockParser.SupportedMovieExtensions.Returns([existingFormat]);

		var actual = await _deprecator.Allow(existingFormat);
		Assert.IsTrue(actual);
		Assert.AreEqual(0, _db.DeprecatedMovieFormats.Count());
	}

	[TestMethod]
	public async Task Allow_ExistsAndDeprecated_Allows()
	{
		const string existingFormat = ".test1";
		_mockParser.SupportedMovieExtensions.Returns([existingFormat]);

		_db.DeprecatedMovieFormats.Add(new DeprecatedMovieFormat
		{
			FileExtension = existingFormat,
			Deprecated = true
		});
		await _db.SaveChangesAsync();

		var actual = await _deprecator.Allow(existingFormat);
		Assert.IsTrue(actual);
		Assert.AreEqual(1, _db.DeprecatedMovieFormats.Count());
		var record = _db.DeprecatedMovieFormats.Single();
		Assert.AreEqual(existingFormat, record.FileExtension);
		Assert.IsFalse(record.Deprecated);
	}

	[TestMethod]
	public async Task Allow_ExistsAndAllowed_Allows()
	{
		const string existingFormat = ".test1";
		_mockParser.SupportedMovieExtensions.Returns([existingFormat]);

		_db.DeprecatedMovieFormats.Add(new DeprecatedMovieFormat
		{
			FileExtension = existingFormat,
			Deprecated = false
		});
		await _db.SaveChangesAsync();

		var actual = await _deprecator.Allow(existingFormat);
		Assert.IsTrue(actual);
		Assert.AreEqual(1, _db.DeprecatedMovieFormats.Count());
		var record = _db.DeprecatedMovieFormats.Single();
		Assert.AreEqual(existingFormat, record.FileExtension);
		Assert.IsFalse(record.Deprecated);
	}

	#endregion

	#region Deprecate

	[TestMethod]
	public async Task Deprecate_InvalidFormat_ReturnsFalse()
	{
		_mockParser.SupportedMovieExtensions.Returns([".test1"]);

		var actual = await _deprecator.Deprecate("invalid");
		Assert.IsFalse(actual);
		Assert.AreEqual(0, _db.DeprecatedMovieFormats.Count());
	}

	[TestMethod]
	public async Task Deprecate_ConcurrentUpdateConflict_ReturnsFalse()
	{
		const string existingFormat = ".test1";
		_mockParser.SupportedMovieExtensions.Returns([existingFormat]);

		_db.DeprecatedMovieFormats.Add(new DeprecatedMovieFormat
		{
			FileExtension = existingFormat,
			Deprecated = true
		});
		await _db.SaveChangesAsync();
		_db.CreateConcurrentUpdateConflict();

		var actual = await _deprecator.Deprecate(existingFormat);
		Assert.IsFalse(actual);
	}

	[TestMethod]
	public async Task Deprecate_UpdateConflict_ReturnsFalse()
	{
		const string existingFormat = ".test1";
		_mockParser.SupportedMovieExtensions.Returns([existingFormat]);
		_db.DeprecatedMovieFormats.Add(new DeprecatedMovieFormat
		{
			FileExtension = existingFormat,
			Deprecated = true
		});
		await _db.SaveChangesAsync();
		_db.CreateUpdateConflict();

		var actual = await _deprecator.Deprecate(existingFormat);
		Assert.IsFalse(actual);
	}

	[TestMethod]
	public async Task Deprecate_DbRecordExistsAndAllowed_Deprecates()
	{
		const string existingFormat = ".test1";
		_mockParser.SupportedMovieExtensions.Returns([existingFormat]);
		_db.DeprecatedMovieFormats.Add(new DeprecatedMovieFormat
		{
			FileExtension = existingFormat,
			Deprecated = true
		});
		await _db.SaveChangesAsync();

		var actual = await _deprecator.Deprecate(existingFormat);
		Assert.IsTrue(actual);
		Assert.AreEqual(1, _db.DeprecatedMovieFormats.Count());
		var record = _db.DeprecatedMovieFormats.Single();
		Assert.AreEqual(existingFormat, record.FileExtension);
		Assert.IsTrue(record.Deprecated);
	}

	[TestMethod]
	public async Task Deprecate_DbRecordExistsAndDeprecated_Deprecates()
	{
		const string existingFormat = ".test1";
		_mockParser.SupportedMovieExtensions.Returns([existingFormat]);
		_db.DeprecatedMovieFormats.Add(new DeprecatedMovieFormat
		{
			FileExtension = existingFormat,
			Deprecated = false
		});
		await _db.SaveChangesAsync();

		var actual = await _deprecator.Deprecate(existingFormat);
		Assert.IsTrue(actual);
		Assert.AreEqual(1, _db.DeprecatedMovieFormats.Count());
		var record = _db.DeprecatedMovieFormats.Single();
		Assert.AreEqual(existingFormat, record.FileExtension);
		Assert.IsTrue(record.Deprecated);
	}

	[TestMethod]
	public async Task Deprecate_NoDbRecord_Adds()
	{
		const string existingFormat = ".test1";
		_mockParser.SupportedMovieExtensions.Returns([existingFormat]);

		var actual = await _deprecator.Deprecate(existingFormat);
		Assert.IsTrue(actual);
		Assert.AreEqual(1, _db.DeprecatedMovieFormats.Count());
		var record = _db.DeprecatedMovieFormats.Single();
		Assert.AreEqual(existingFormat, record.FileExtension);
		Assert.IsTrue(record.Deprecated);
	}

	#endregion
}
