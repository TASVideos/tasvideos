﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
	public sealed class TiersControllerTests : IDisposable
	{
		private readonly Mock<ITagService> _mockTagService;
		private readonly TagsController _tagsController;

		public TiersControllerTests()
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

		public void Dispose()
		{
			_tagsController.Dispose();
		}
	}
}
