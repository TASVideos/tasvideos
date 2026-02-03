using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Routing;

namespace TASVideos.Api;

internal static class RegistrationExtensions
{
	public static RouteGroupBuilder MapApiGroup(this WebApplication app, string group)
	{
		return app.MapGroup($"api/v1/{group.ToLower()}").WithTags(group);
	}

	public static RouteHandlerBuilder ProducesFromId<T>(this RouteHandlerBuilder builder, string resource)
	{
		return builder
			.WithDescription($"""
							<p>200 If a {resource} is found with the given id</p>
							<p>404 if no {resource} was found<p>
							""")
			.Produces<T>()
			.Produces<NotFoundResponse>(StatusCodes.Status404NotFound)
			.WithSummary($"Returns a {resource} with the given id.");
	}

	public static RouteHandlerBuilder Receives<T>(this RouteHandlerBuilder builder)
	{
		return builder
			.Produces<T>()
			.Produces(StatusCodes.Status400BadRequest);
	}

	public static RouteHandlerBuilder ProducesList<T>(this RouteHandlerBuilder builder, string summary)
	{
		return builder
			.WithDescription($"""
							<p>200 If request parameters are valid, returns {summary}</p>
							<p>400 if the request parameters are invalid</p>
							""")
			.WithSummary($"Returns {summary}").Produces<IEnumerable<T>>();
	}

	// ReSharper disable once ClassNeverInstantiated.Local
	[SuppressMessage("ReSharper", "NotAccessedPositionalProperty.Local", Justification = "Only used for reflection")]
	private record NotFoundResponse(string Title, int Status);
}
