using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Routing;

namespace TASVideos.Api;

internal static class RegistrationExtensions
{
	public static RouteGroupBuilder MapApiGroup(this WebApplication app, string group)
		=> app.MapGroup($"api/v1/{group.ToLower()}").WithTags(group);

	extension(RouteHandlerBuilder builder)
	{
		public RouteHandlerBuilder ProducesFromId<T>(string resource)
			=> builder
				.WithDescription($"""
								<p>200 If a {resource} is found with the given id</p>
								<p>404 if no {resource} was found<p>
								""")
				.Produces<T>()
				.Produces<NotFoundResponse>(StatusCodes.Status404NotFound)
				.WithSummary($"Returns a {resource} with the given id.");

		public RouteHandlerBuilder Receives<T>()
			=> builder
				.Produces<T>()
				.Produces(StatusCodes.Status400BadRequest);

		public RouteHandlerBuilder ProducesList<T>(string summary)
			=> builder
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
