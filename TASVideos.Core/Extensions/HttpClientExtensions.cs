using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace TASVideos.Core.HttpClientExtensions;

public static class HttpClientExtensions
{
	public static StringContent ToStringContent(this object obj)
	{
		return new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json");
	}

	public static async Task<T> ReadAsync<T>(this HttpResponseMessage response)
	{
		var content = await response.Content.ReadAsStringAsync();
		var result = JsonConvert.DeserializeObject<T>(content);
		if (result == null)
		{
			throw new InvalidCastException($"Unable to deserialize {content} to type {typeof(T)}");
		}

		return result;
	}

	public static void SetBearerToken(this HttpClient client, string token)
	{
		client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
	}

	public static void SetOAuthToken(this HttpClient client, string token)
	{
		client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("OAuth", token);
	}

	public static void SetBotToken(this HttpClient client, string token)
	{
		client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bot", token);
	}
}
