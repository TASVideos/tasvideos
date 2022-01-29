using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;

namespace TASVideos.Tests.Base;

public static class HttpClientFactoryMock
{
	public static Mock<IHttpClientFactory> Create()
	{
		var clientHandlerMock = new Mock<DelegatingHandler>();
		clientHandlerMock.Protected()
			.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK))
			.Verifiable();
		clientHandlerMock.As<IDisposable>().Setup(s => s.Dispose());
		var httpClient = new HttpClient(clientHandlerMock.Object);
		var clientFactoryMock = new Mock<IHttpClientFactory>();
		clientFactoryMock.Setup(cf => cf.CreateClient(It.IsAny<string>())).Returns(httpClient).Verifiable();

		return clientFactoryMock;
	}
}
