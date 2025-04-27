using System.Globalization;
using System.Net.Http.Headers;
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

		BlueskyEmbed.BlueskyEmbedImage? embedImage = null;
		if (post.ImageData is not null && post.ImageMimeType is not null)
		{
			var imageContent = new ByteArrayContent(post.ImageData);
			imageContent.Headers.ContentType = new MediaTypeHeaderValue(post.ImageMimeType);
			var blobRequest = await _client.PostAsync("com.atproto.repo.uploadBlob", imageContent);
			if (blobRequest.IsSuccessStatusCode)
			{
				var blobResponse = await blobRequest.ReadAsync<BlueskyUploadBlobResponse>();
				if (blobResponse.Blob is not null)
				{
					embedImage = new BlueskyEmbed.BlueskyEmbedImage(blobResponse.Blob, post.ImageWidth, post.ImageHeight);
				}
			}
		}

		var postResponse = await _client.PostAsync("com.atproto.repo.createRecord", new BlueskyCreateRecordRequest(session.Did, post, embedImage).ToStringContent());
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

	public class BlueskyCreateRecordRequest(string repo, IPostable post, BlueskyEmbed.BlueskyEmbedImage? image)
	{
		[JsonPropertyName("repo")]
		public string Repo { get; set; } = repo;

		[JsonPropertyName("collection")]
		public string Collection { get; } = "app.bsky.feed.post";

		[JsonPropertyName("record")]
		public BlueskyPost Record { get; set; } = new BlueskyPost(post, image);
	}

	public class BlueskyPost
	{
		public BlueskyPost(IPostable post, BlueskyEmbed.BlueskyEmbedImage? image)
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
			Embed = image is not null ? new BlueskyEmbed(image) : null;
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

		[JsonPropertyName("embed")]
		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public BlueskyEmbed? Embed { get; set; }
	}

	public class BlueskyEmbed(BlueskyEmbed.BlueskyEmbedImage image)
	{
		[JsonPropertyName("$type")]
		public string Type { get; } = "app.bsky.embed.images";
		[JsonPropertyName("images")]
		public List<BlueskyEmbedImage> Images { get; set; } = [image];

		public class BlueskyEmbedImage(BlueskyBlob image, int? width, int? height)
		{
			[JsonPropertyName("image")]
			public BlueskyBlob Image { get; set; } = image;
			[JsonPropertyName("alt")]
			public string Alt { get; set; } = "";

			[JsonPropertyName("aspectRatio")]
			[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
			public BlueskyEmbedImageAspectRatio? AspectRatio { get; set; } = width is not null && height is not null ? new BlueskyEmbedImageAspectRatio((int)width, (int)height) : null;
			public class BlueskyEmbedImageAspectRatio(int width, int height)
			{
				[JsonPropertyName("width")]
				public int Width { get; set; } = width;
				[JsonPropertyName("height")]
				public int Height { get; set; } = height;
			}
		}
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

	public class BlueskyUploadBlobResponse
	{
		[JsonPropertyName("blob")]
		public BlueskyBlob? Blob { get; set; }
	}

	public class BlueskyBlob
	{
		[JsonPropertyName("$type")]
		public string Type { get; set; } = "blob";
		[JsonPropertyName("ref")]
		public BlueskyLink? Ref { get; set; }
		[JsonPropertyName("mimeType")]
		public string MimeType { get; set; } = "";
		[JsonPropertyName("size")]
		public int Size { get; set; }

		public class BlueskyLink
		{
			[JsonPropertyName("$link")]
			public string Link { get; set; } = "";
		}
	}
}
