using System.Globalization;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using TASVideos.Core.HttpClientExtensions;
using TASVideos.Core.Settings;

namespace TASVideos.Core.Services.ExternalMediaPublisher.Distributors;
public sealed class BlueskyDistributor(
	AppSettings appSettings,
	IHttpClientFactory httpClientFactory,
	ILogger<BlueskyDistributor> logger) : IPostDistributor
{
	private readonly HttpClient _client = httpClientFactory.CreateClient(HttpClients.Bluesky);
	private readonly AppSettings.BlueskyConnection _settings = appSettings.Bluesky;

	public IEnumerable<PostType> Types => [PostType.Announcement];

	public async Task Post(IPostable post)
	{
		if (!_settings.IsEnabled())
		{
			return;
		}

		_client.ResetAuthorization();
		var sessionResponse = await _client.PostAsync("com.atproto.server.createSession", new BlueskyCreateSessionRequest(_settings.Identifier, _settings.Password).ToStringContent());
		if (!sessionResponse.IsSuccessStatusCode)
		{
			logger.LogError("Failed to create Bluesky session");
			return;
		}

		var session = await sessionResponse.ReadAsync<BlueskyCreateSessionResponse>();
		_client.SetBearerToken(session.AccessJwt);

		var postResponse = await _client.PostAsync("com.atproto.repo.createRecord", new BlueskyCreateRecordRequest(session.Did, post).ToStringContent());
		if (!postResponse.IsSuccessStatusCode)
		{
			logger.LogError("Failed to create Bluesky post");
		}
	}

	public class BlueskyCreateSessionRequest(string identifier, string password)
	{
		[JsonPropertyName("identifier")]
		public string Identifier { get; set; } = identifier;

		[JsonPropertyName("password")]
		public string Password { get; set; } = password;
	}

	public class BlueskyCreateSessionResponse
	{
		[JsonPropertyName("accessJwt")]
		public string AccessJwt { get; set; } = "";
		[JsonPropertyName("did")]
		public string Did { get; set; } = "";
	}

	public class BlueskyCreateRecordRequest(string repo, IPostable post)
	{
		[JsonPropertyName("repo")]
		public string Repo { get; set; } = repo;

		[JsonPropertyName("collection")]
		public string Collection { get; } = "app.bsky.feed.post";

		[JsonPropertyName("record")]
		public BlueskyPost Record { get; set; } = new BlueskyPost(post);
	}

	public class BlueskyPost
	{
		public BlueskyPost(IPostable post)
		{
			var body = post.Group switch
			{
				PostGroups.Submission => post.Title,
				PostGroups.Publication => post.Title,
				_ => post.Body
			};

			if (!string.IsNullOrWhiteSpace(post.Link))
			{
				body = body.CapAndEllipse(300 - (post.Link.Length + 2));

				body += '\n';
				var bodyLengthUtf8 = Encoding.UTF8.GetByteCount(body);
				var linkLengthUtf8 = Encoding.UTF8.GetByteCount(post.Link);

				body += post.Link;

				int byteStart = bodyLengthUtf8;
				int byteEnd = byteStart + linkLengthUtf8;

				Facets.Add(new BlueskyFacet(byteStart, byteEnd, post.Link));
			}
			else
			{
				body = body.CapAndEllipse(300);
			}

			Text = body;
			CreatedAt = DateTime.UtcNow.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture);
		}

		[JsonPropertyName("$type")]
		public string Type { get; } = "app.bsky.feed.post";

		[JsonPropertyName("text")]
		public string Text { get; set; }

		[JsonPropertyName("facets")]
		public List<BlueskyFacet> Facets { get; set; } = [];

		[JsonPropertyName("langs")]
		public List<string> Langs { get; } = ["en-US"];

		[JsonPropertyName("createdAt")]
		public string CreatedAt { get; set; }
	}

	public class BlueskyFacet(int byteStart, int byteEnd, string uri)
	{
		[JsonPropertyName("index")]
		public BlueskyFacetIndex Index { get; set; } = new BlueskyFacetIndex(byteStart, byteEnd);

		[JsonPropertyName("features")]
		public List<BlueskyFacetFeature> Features { get; set; } = [new BlueskyFacetFeature(uri)];

		public class BlueskyFacetIndex(int byteStart, int byteEnd)
		{
			[JsonPropertyName("byteStart")]
			public int ByteStart { get; set; } = byteStart;

			[JsonPropertyName("byteEnd")]
			public int ByteEnd { get; set; } = byteEnd;
		}

		public class BlueskyFacetFeature(string uri)
		{
			[JsonPropertyName("$type")]
			public string Type { get; } = "app.bsky.richtext.facet#link";

			[JsonPropertyName("uri")]
			public string Uri { get; set; } = uri;
		}
	}
}
