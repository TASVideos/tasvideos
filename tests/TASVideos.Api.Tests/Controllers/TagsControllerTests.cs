﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Api.Controllers;
using TASVideos.Api.Requests;
using TASVideos.Core.Services;
using TASVideos.Data.Entity;

namespace TASVideos.Api.Tests.Controllers;

//[TestClass]
//public sealed class TagsControllerTests : IDisposable
//{
//	private readonly ITagService _mockTagService;
//	private readonly TagsController _tagsController;

//	public TagsControllerTests()
//	{
//		_mockTagService = Substitute.For<ITagService>();
//		var httpContext = new DefaultHttpContext();
//		_tagsController = new TagsController(_mockTagService)
//		{
//			ControllerContext = new ControllerContext
//			{
//				HttpContext = httpContext,
//			}
//		};
//	}

//	[TestMethod]
//	public async Task GetById_NotFound()
//	{
//		_mockTagService.GetById(Arg.Any<int>()).Returns((Tag?)null);

//		var result = await _tagsController.GetById(int.MaxValue);
//		Assert.IsNotNull(result);
//		Assert.IsInstanceOfType(result, typeof(NotFoundResult));
//		Assert.AreEqual(404, ((NotFoundResult)result).StatusCode);
//	}

//	[TestMethod]
//	public async Task GetById_Found()
//	{
//		const string code = "Test";
//		const string displayName = "Test Tag";
//		_mockTagService.GetById(Arg.Any<int>()).Returns(new Tag { Code = code, DisplayName = displayName });

//		var result = await _tagsController.GetById(1);
//		Assert.IsNotNull(result);
//		Assert.IsInstanceOfType(result, typeof(OkObjectResult));
//		var okResult = (OkObjectResult)result;
//		Assert.AreEqual(200, okResult.StatusCode);
//		Assert.IsInstanceOfType(okResult.Value, typeof(Tag));
//		var tag = (Tag)okResult.Value!;
//		Assert.AreEqual(tag.Code, code);
//		Assert.AreEqual(tag.DisplayName, displayName);
//	}

//	[TestMethod]
//	public async Task Create_Success_Returns201()
//	{
//		const int createdId = 1;
//		_mockTagService
//			.Add(Arg.Any<string>(), Arg.Any<string>())
//			.Returns((createdId, TagEditResult.Success));

//		var result = await _tagsController.Create(new TagAddEditRequest());

//		Assert.IsNotNull(result);
//		Assert.IsInstanceOfType(result, typeof(CreatedResult));
//		var createdResult = (CreatedResult)result;
//		Assert.AreEqual(201, createdResult.StatusCode);
//		Assert.IsTrue((createdResult.Location ?? "").Contains(createdId.ToString()));
//	}

//	[TestMethod]
//	public async Task Create_Duplicate_Returns409()
//	{
//		_mockTagService
//			.Add(Arg.Any<string>(), Arg.Any<string>())
//			.Returns((1, TagEditResult.DuplicateCode));

//		var result = await _tagsController.Create(new TagAddEditRequest());

//		Assert.IsNotNull(result);
//		Assert.IsInstanceOfType(result, typeof(ConflictObjectResult));
//		var conflictResult = (ConflictObjectResult)result;
//		Assert.AreEqual(409, conflictResult.StatusCode);
//		Assert.IsInstanceOfType(conflictResult.Value, typeof(SerializableError));
//		var error = (SerializableError)conflictResult.Value!;
//		Assert.IsTrue(error.Count == 1);
//		Assert.IsTrue(error.ContainsKey(nameof(TagAddEditRequest.Code)));
//	}

//	[TestMethod]
//	public async Task Create_Fail_Returns400()
//	{
//		_mockTagService
//			.Add(Arg.Any<string>(), Arg.Any<string>())
//			.Returns((1, TagEditResult.Fail));

//		var result = await _tagsController.Create(new TagAddEditRequest());

//		Assert.IsNotNull(result);
//		Assert.IsInstanceOfType(result, typeof(BadRequestResult));
//		Assert.AreEqual(400, ((BadRequestResult)result).StatusCode);
//	}

//	[TestMethod]
//	public async Task Update_Success_Returns200()
//	{
//		_mockTagService
//			.Add(Arg.Any<string>(), Arg.Any<string>())
//			.Returns((1, TagEditResult.Success));

//		var result = await _tagsController.Update(1, new TagAddEditRequest());

//		Assert.IsNotNull(result);
//		Assert.IsInstanceOfType(result, typeof(OkResult));
//		Assert.AreEqual(200, ((OkResult)result).StatusCode);
//	}

//	[TestMethod]
//	public async Task Update_NotFound_Returns404()
//	{
//		_mockTagService
//			.Edit(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>())
//			.Returns(TagEditResult.NotFound);

//		var result = await _tagsController.Update(1, new TagAddEditRequest());

//		Assert.IsNotNull(result);
//		Assert.IsInstanceOfType(result, typeof(NotFoundResult));
//		Assert.AreEqual(404, ((NotFoundResult)result).StatusCode);
//	}

//	[TestMethod]
//	public async Task Update_Duplicate_Returns409()
//	{
//		_mockTagService
//			.Edit(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>())
//			.Returns(TagEditResult.DuplicateCode);

//		var result = await _tagsController.Update(1, new TagAddEditRequest());

//		Assert.IsNotNull(result);
//		Assert.IsInstanceOfType(result, typeof(ConflictObjectResult));
//		Assert.AreEqual(409, ((ConflictObjectResult)result).StatusCode);
//		var conflictResult = (ConflictObjectResult)result;
//		Assert.IsInstanceOfType(conflictResult.Value, typeof(SerializableError));
//		var error = (SerializableError)conflictResult.Value!;
//		Assert.IsTrue(error.Count == 1);
//		Assert.IsTrue(error.ContainsKey(nameof(TagAddEditRequest.Code)));
//	}

//	[TestMethod]
//	public async Task Delete_Success_Returns200()
//	{
//		_mockTagService.Delete(Arg.Any<int>()).Returns(TagDeleteResult.Success);

//		var result = await _tagsController.Delete(1);

//		Assert.IsNotNull(result);
//		Assert.IsInstanceOfType(result, typeof(OkResult));
//		Assert.AreEqual(200, ((OkResult)result).StatusCode);
//	}

//	[TestMethod]
//	public async Task Delete_NotFound_Returns404()
//	{
//		_mockTagService.Delete(Arg.Any<int>()).Returns(TagDeleteResult.NotFound);

//		var result = await _tagsController.Delete(1);

//		Assert.IsNotNull(result);
//		Assert.IsInstanceOfType(result, typeof(NotFoundResult));
//		Assert.AreEqual(404, ((NotFoundResult)result).StatusCode);
//	}

//	[TestMethod]
//	public async Task Delete_InUse_Returns409()
//	{
//		_mockTagService.Delete(Arg.Any<int>()).Returns(TagDeleteResult.InUse);

//		var result = await _tagsController.Delete(1);

//		Assert.IsNotNull(result);
//		Assert.IsInstanceOfType(result, typeof(ConflictObjectResult));
//		Assert.AreEqual(409, ((ConflictObjectResult)result).StatusCode);
//		var conflictResult = (ConflictObjectResult)result;
//		Assert.IsInstanceOfType(conflictResult.Value, typeof(SerializableError));
//		var error = (SerializableError)conflictResult.Value!;
//		Assert.IsTrue(error.Count == 1);
//	}

//	public void Dispose()
//	{
//		_tagsController.Dispose();
//	}
//}
