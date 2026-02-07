using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace TASVideos.Core.HttpClientExtensions;

public static class HttpClientExtensions
{
	public static StringContent ToStringContent(this object obj)
	{
		return new StringContent(JsonSerializer.Serialize(obj), Encoding.UTF8, "application/json");
	}

	public static async Task<T> ReadAsync<T>(this HttpResponseMessage response)
	{
		var content = await response.Content.ReadAsStringAsync();
		var result = JsonSerializer.Deserialize<T>(content);
		return result ?? throw new InvalidCastException($"Unable to deserialize {content} to type {typeof(T)}");
	}

	extension(HttpClient client)
	{
		public void SetBearerToken(string token)
			=> client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

		public void SetOAuthToken(string token)
			=> client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("OAuth", token);

		public void SetBotToken(string token)
			=> client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bot", token);

		public void SetBasicAuth(string basicAuthHeader)
			=> client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basicAuthHeader);

		public void ResetAuthorization() => client.DefaultRequestHeaders.Authorization = null;
	}
}
