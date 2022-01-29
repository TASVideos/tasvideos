﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity;

namespace TASVideos.Core.Services;

public interface IPointsService
{
	/// <summary>
	/// Calculates the player points for the user with the given id.
	/// If a user with the given <see cref="userId"/> does not exist, 0 is returned
	/// </summary>
	ValueTask<double> PlayerPoints(int userId);

	/// <summary>
	/// Returns the averaged overall, tech, and entertainment ratings for a publication
	/// with the given id.
	/// </summary>
	ValueTask<RatingDto> PublicationRating(int id);

	/// <summary>
	/// Returns the averaged overall, tech, and entertainment ratings for a publication
	/// with the given set of ids
	/// <seealso cref="PublicationRating"/>
	/// </summary>
	Task<IDictionary<int, RatingDto>> PublicationRatings(IEnumerable<int> publicationIds);
}

internal class PointsService : IPointsService
{
	private const string MovieRatingKey = "OverallRatingForMovie-";
	private const string PlayerPointKey = "PlayerPoints-";
	private const string AverageNumberOfRatingsKey = "AverageNumberOfRatings";

	private readonly ApplicationDbContext _db;
	private readonly ICacheService _cache;

	public PointsService(
		ApplicationDbContext db,
		ICacheService cache)
	{
		_db = db;
		_cache = cache;
	}

	public async ValueTask<double> PlayerPoints(int userId)
	{
		string cacheKey = PlayerPointKey + userId;
		if (_cache.TryGetValue(cacheKey, out double playerPoints))
		{
			return playerPoints;
		}

		var publications = await _db.Publications
			.Where(p => p.Authors.Select(pa => pa.UserId).Contains(userId))
			.Select(p => new PublicationRatingDto
			{
				Id = p.Id,
				Obsolete = p.ObsoletedById.HasValue,
				ClassWeight = p.PublicationClass!.Weight,
				AuthorCount = p.Authors.Count,
				PublicationRatings = p.PublicationRatings
					.Select(r => new Rating(r.Type, r.Value))
					.ToList()
			})
			.ToListAsync();

		var pubsForCalculation = publications
			.Select(pub => new PointsCalculator.Publication
			{
				Id = pub.Id,
				AuthorCount = pub.AuthorCount,
				Obsolete = pub.Obsolete,
				ClassWeight = pub.ClassWeight,
				RatingCount = pub.PublicationRatings.Count,
				AverageRating = Rate(pub.PublicationRatings).Overall ?? 0
			})
			.ToList();

		var averageRatings = await AverageNumberOfRatingsPerPublication();
		playerPoints = Math.Round(PointsCalculator.PlayerPoints(pubsForCalculation, averageRatings), 1);

		_cache.Set(cacheKey, playerPoints);
		return playerPoints;
	}

	public async ValueTask<RatingDto> PublicationRating(int id)
	{
		string cacheKey = MovieCacheKey(id);
		if (_cache.TryGetValue(cacheKey, out RatingDto rating))
		{
			return rating;
		}

		var ratings = await _db.PublicationRatings
			.ForPublication(id)
			.ThatAreNotFromAnAuthor()
			.ThatCanBeUsedToRate()
			.Select(r => new Rating(r.Type, r.Value))
			.ToListAsync();

		rating = Rate(ratings);

		_cache.Set(cacheKey, rating);
		return rating;
	}

	public async Task<IDictionary<int, RatingDto>> PublicationRatings(IEnumerable<int> publicationIds)
	{
		var ids = publicationIds.ToList();
		var ratings = _cache
			.GetAll<RatingDto>(ids.Select(MovieCacheKey))
			.ToDictionary(
				tkey => int.Parse(tkey.Key.Replace(MovieRatingKey, "")),
				tvalue => tvalue.Value);

		var publicationsToRate = ids.Where(i => !ratings.ContainsKey(i));

		var ratingsByPub = (await _db.PublicationRatings
			.Where(p => publicationsToRate.Contains(p.PublicationId))
			.ThatAreNotFromAnAuthor()
			.ThatCanBeUsedToRate()
			.ToListAsync())
			.GroupBy(gkey => gkey.PublicationId);

		foreach (var pub in ratingsByPub)
		{
			var cacheKey = MovieCacheKey(pub.Key);
			var pubRatings = pub.ToList();
			var rating = Rate(pubRatings
				.Select(r => new Rating(r.Type, r.Value))
				.ToList());
			_cache.Set(cacheKey, rating);
			ratings.Add(pub.Key, rating);
		}

		return ratings;
	}

	private static string MovieCacheKey(int id) => MovieRatingKey + id;

	private static RatingDto Rate(ICollection<Rating> ratings)
	{
		var entRatings = ratings
			.Where(r => r.Type == PublicationRatingType.Entertainment)
			.Select(r => r.Value)
			.ToList();

		var techRatings = ratings
			.Where(r => r.Type == PublicationRatingType.TechQuality)
			.Select(r => r.Value)
			.ToList();

		var rating = new RatingDto
		{
			Entertainment = entRatings.Any()
				? entRatings.Average()
				: null,
			TechQuality = techRatings.Any()
				? techRatings.Average()
				: null,
			TotalEntertainmentVotes = entRatings.Count,
			TotalTechQualityVotes = techRatings.Count
		};

		if (entRatings.Any() || techRatings.Any())
		{
			// Entertainment counts 2:1 over Tech
			rating.Overall = entRatings
				.Concat(entRatings)
				.Concat(techRatings)
				.Average();
		}

		return rating;
	}

	// total ratings / (2 * total publications)
	private async ValueTask<double> AverageNumberOfRatingsPerPublication()
	{
		if (_cache.TryGetValue(AverageNumberOfRatingsKey, out double playerPoints))
		{
			return playerPoints;
		}

		var totalPublications = await _db.Publications.CountAsync();

		double avg = 0;
		if (totalPublications > 0)
		{
			var totalRatings = await _db.PublicationRatings.CountAsync();
			avg = totalRatings / (double)(2 * totalPublications);
		}

		_cache.Set(AverageNumberOfRatingsKey, avg);
		return avg;
	}

	private class PublicationRatingDto
	{
		public int Id { get; init; }
		public bool Obsolete { get; init; }
		public double ClassWeight { get; init; }
		public int AuthorCount { get; init; }
		public ICollection<Rating> PublicationRatings { get; init; } = new List<Rating>();
	}

	private record Rating(PublicationRatingType Type, double Value);
}
