using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Api.Controllers;
using TASVideos.Api.Requests;
using TASVideos.Core.Services;
using TASVideos.Data.Entity;

namespace TASVideos.Api.Tests.Controllers;

[TestClass]
public sealed class ClassControllerTests : IDisposable
{
	private readonly IClassService _mockClassService;
	private readonly ClassesController _classesController;

	public ClassControllerTests()
	{
		_mockClassService = Substitute.For<IClassService>();
		var httpContext = new DefaultHttpContext();
		_classesController = new ClassesController(_mockClassService)
		{
			ControllerContext = new ControllerContext
			{
				HttpContext = httpContext,
			}
		};
	}

	[TestMethod]
	public async Task GetAll_NoData_Returns200()
	{
		_mockClassService.GetAll().Returns(new List<PublicationClass>());

		var result = await _classesController.GetAll();
		Assert.IsNotNull(result);
		Assert.IsInstanceOfType(result, typeof(OkObjectResult));
		var okResult = (OkObjectResult)result;
		Assert.AreEqual(200, okResult.StatusCode);
		Assert.IsInstanceOfType(okResult.Value, typeof(ICollection<PublicationClass>));
		var classes = (ICollection<PublicationClass>)okResult.Value!;
		Assert.AreEqual(0, classes.Count);
	}

	[TestMethod]
	public async Task GetById_NotFound()
	{
		_mockClassService.GetById(Arg.Any<int>()).Returns((PublicationClass?)null);

		var result = await _classesController.GetById(int.MaxValue);
		Assert.IsNotNull(result);
		Assert.IsInstanceOfType(result, typeof(NotFoundResult));
		Assert.AreEqual(404, ((NotFoundResult)result).StatusCode);
	}

	[TestMethod]
	public async Task GeById_Found()
	{
		const string name = "name";
		const string link = "link";
		const string icon = "icon";
		const double weight = 1;
		_mockClassService
			.GetById(Arg.Any<int>())
			.Returns(new PublicationClass { Name = name, Link = link, IconPath = icon, Weight = weight });

		var result = await _classesController.GetById(1);
		Assert.IsNotNull(result);
		Assert.IsInstanceOfType(result, typeof(OkObjectResult));
		var okResult = (OkObjectResult)result;
		Assert.AreEqual(200, okResult.StatusCode);
		Assert.IsInstanceOfType(okResult.Value, typeof(PublicationClass));
		var publicationClass = (PublicationClass)okResult.Value!;
		Assert.AreEqual(name, publicationClass.Name);
		Assert.AreEqual(link, publicationClass.Link);
		Assert.AreEqual(icon, publicationClass.IconPath);
		Assert.AreEqual(weight, publicationClass.Weight);
	}

	[TestMethod]
	public async Task Create_Success_Returns201()
	{
		const int createdId = 1;
		_mockClassService.Add(Arg.Any<PublicationClass>()).Returns((createdId, ClassEditResult.Success));

		var result = await _classesController.Create(new ClassAddEditRequest());

		Assert.IsNotNull(result);
		Assert.IsInstanceOfType(result, typeof(CreatedResult));
		var createdResult = (CreatedResult)result;
		Assert.AreEqual(201, createdResult.StatusCode);
		Assert.IsTrue((createdResult.Location ?? "").Contains(createdId.ToString()));
	}

	[TestMethod]
	public async Task Create_Duplicate_Returns409()
	{
		_mockClassService.Add(Arg.Any<PublicationClass>()).Returns((1, ClassEditResult.DuplicateName));

		var result = await _classesController.Create(new ClassAddEditRequest());

		Assert.IsNotNull(result);
		Assert.IsInstanceOfType(result, typeof(ConflictObjectResult));
		var conflictResult = (ConflictObjectResult)result;
		Assert.AreEqual(409, conflictResult.StatusCode);
		Assert.IsInstanceOfType(conflictResult.Value, typeof(SerializableError));
		var error = (SerializableError)conflictResult.Value!;
		Assert.IsTrue(error.Count == 1);
		Assert.IsTrue(error.ContainsKey(nameof(ClassAddEditRequest.Name)));
	}

	[TestMethod]
	public async Task Create_Fail_Returns400()
	{
		_mockClassService.Add(Arg.Any<PublicationClass>()).Returns((1, ClassEditResult.Fail));

		var result = await _classesController.Create(new ClassAddEditRequest());

		Assert.IsNotNull(result);
		Assert.IsInstanceOfType(result, typeof(BadRequestResult));
		Assert.AreEqual(400, ((BadRequestResult)result).StatusCode);
	}

	[TestMethod]
	public async Task Update_Success_Returns200()
	{
		_mockClassService
			.Edit(Arg.Any<int>(), Arg.Any<PublicationClass>())
			.Returns(ClassEditResult.Success);

		var result = await _classesController.Update(1, new ClassAddEditRequest());

		Assert.IsNotNull(result);
		Assert.IsInstanceOfType(result, typeof(OkResult));
		Assert.AreEqual(200, ((OkResult)result).StatusCode);
	}

	[TestMethod]
	public async Task Update_NotFound_Returns404()
	{
		_mockClassService
			.Edit(Arg.Any<int>(), Arg.Any<PublicationClass>())
			.Returns(ClassEditResult.NotFound);

		var result = await _classesController.Update(1, new ClassAddEditRequest());

		Assert.IsNotNull(result);
		Assert.IsInstanceOfType(result, typeof(NotFoundResult));
		Assert.AreEqual(404, ((NotFoundResult)result).StatusCode);
	}

	[TestMethod]
	public async Task Update_Duplicate_Returns409()
	{
		_mockClassService
			.Edit(Arg.Any<int>(), Arg.Any<PublicationClass>())
			.Returns(ClassEditResult.DuplicateName);

		var result = await _classesController.Update(1, new ClassAddEditRequest());

		Assert.IsNotNull(result);
		Assert.IsInstanceOfType(result, typeof(ConflictObjectResult));
		Assert.AreEqual(409, ((ConflictObjectResult)result).StatusCode);
		var conflictResult = (ConflictObjectResult)result;
		Assert.IsInstanceOfType(conflictResult.Value, typeof(SerializableError));
		var error = (SerializableError)conflictResult.Value!;
		Assert.IsTrue(error.Count == 1);
		Assert.IsTrue(error.ContainsKey(nameof(ClassAddEditRequest.Name)));
	}

	[TestMethod]
	public async Task Delete_Success_Returns200()
	{
		_mockClassService.Delete(Arg.Any<int>()).Returns(ClassDeleteResult.Success);

		var result = await _classesController.Delete(1);

		Assert.IsNotNull(result);
		Assert.IsInstanceOfType(result, typeof(OkResult));
		Assert.AreEqual(200, ((OkResult)result).StatusCode);
	}

	[TestMethod]
	public async Task Delete_NotFound_Returns404()
	{
		_mockClassService.Delete(Arg.Any<int>()).Returns(ClassDeleteResult.NotFound);

		var result = await _classesController.Delete(1);

		Assert.IsNotNull(result);
		Assert.IsInstanceOfType(result, typeof(NotFoundResult));
		Assert.AreEqual(404, ((NotFoundResult)result).StatusCode);
	}

	[TestMethod]
	public async Task Delete_InUse_Returns409()
	{
		_mockClassService.Delete(Arg.Any<int>()).Returns(ClassDeleteResult.InUse);

		var result = await _classesController.Delete(1);

		Assert.IsNotNull(result);
		Assert.IsInstanceOfType(result, typeof(ConflictObjectResult));
		Assert.AreEqual(409, ((ConflictObjectResult)result).StatusCode);
		var conflictResult = (ConflictObjectResult)result;
		Assert.IsInstanceOfType(conflictResult.Value, typeof(SerializableError));
		var error = (SerializableError)conflictResult.Value!;
		Assert.IsTrue(error.Count == 1);
		Assert.IsTrue(error.ContainsKey(""));
	}

	public void Dispose()
	{
		_classesController.Dispose();
	}
}
