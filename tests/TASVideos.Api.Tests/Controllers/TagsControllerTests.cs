using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TASVideos.Api.Controllers;
using TASVideos.Core.Services;
using TASVideos.Data.Entity;

namespace TASVideos.Api.Tests.Controllers
{
	[TestClass]
	public sealed class TagsControllerTests : IDisposable
	{
		private readonly Mock<ITagService> _mockTagService;
		private readonly TagsController _tagsController;

		public TagsControllerTests()
		{
			_mockTagService = new Mock<ITagService>();
			_tagsController = new TagsController(_mockTagService.Object);
		}

		[TestMethod]
		public async Task GetById_NotFound()
		{
			_mockTagService
				.Setup(m => m.GetById(It.IsAny<int>()))
				.ReturnsAsync((Tag?)null);
			var result = await _tagsController.GetById(int.MaxValue);
			Assert.IsNotNull(result);
			Assert.IsInstanceOfType(result, typeof(NotFoundResult));
			Assert.AreEqual(404, ((NotFoundResult)result).StatusCode);
		}

		[TestMethod]
		public async Task GetById_Found()
		{
			const string code = "Test";
			const string displayName = "Test Tag";
			_mockTagService
				.Setup(m => m.GetById(It.IsAny<int>()))
				.ReturnsAsync(new Tag { Code = code, DisplayName = displayName });
			var result = await _tagsController.GetById(1);
			Assert.IsNotNull(result);
			Assert.IsInstanceOfType(result, typeof(OkObjectResult));
			var okResult = (OkObjectResult)result;
			Assert.AreEqual(200, okResult.StatusCode);
			Assert.IsInstanceOfType(okResult.Value, typeof(Tag));
			var tag = (Tag)okResult.Value;
			Assert.AreEqual(tag.Code, code);
			Assert.AreEqual(tag.DisplayName, displayName);
		}

		public void Dispose()
		{
			_tagsController.Dispose();
		}
	}
}
