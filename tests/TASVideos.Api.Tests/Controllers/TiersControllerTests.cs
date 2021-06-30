using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TASVideos.Api.Controllers;
using TASVideos.Api.Requests;
using TASVideos.Core.Services;
using TASVideos.Data.Entity;

namespace TASVideos.Api.Tests.Controllers
{
	[TestClass]
	public sealed class TiersControllerTests : IDisposable
	{
		private readonly Mock<ITierService> _mockTierService;
		private readonly TiersController _tiersController;

		public TiersControllerTests()
		{
			_mockTierService = new Mock<ITierService>();
			var httpContext = new DefaultHttpContext();
			_tiersController = new TiersController(_mockTierService.Object)
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
			_mockTierService
				.Setup(m => m.GetAll())
				.ReturnsAsync(new List<Tier>());

			var result = await _tiersController.GetAll();
			Assert.IsNotNull(result);
			Assert.IsInstanceOfType(result, typeof(OkObjectResult));
			var okResult = (OkObjectResult)result;
			Assert.AreEqual(200, okResult.StatusCode);
			Assert.IsInstanceOfType(okResult.Value, typeof(ICollection<Tier>));
			var tiers = (ICollection<Tier>)okResult.Value;
			Assert.AreEqual(0, tiers.Count);
		}

		[TestMethod]
		public async Task GetById_NotFound()
		{
			_mockTierService
				.Setup(m => m.GetById(It.IsAny<int>()))
				.ReturnsAsync((Tier?)null);
			var result = await _tiersController.GetById(int.MaxValue);
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
			_mockTierService
				.Setup(m => m.GetById(It.IsAny<int>()))
				.ReturnsAsync(new Tier { Name = name, Link = link, IconPath = icon, Weight = weight });

			var result = await _tiersController.GetById(1);
			Assert.IsNotNull(result);
			Assert.IsInstanceOfType(result, typeof(OkObjectResult));
			var okResult = (OkObjectResult)result;
			Assert.AreEqual(200, okResult.StatusCode);
			Assert.IsInstanceOfType(okResult.Value, typeof(Tier));
			var tier = (Tier)okResult.Value;
			Assert.AreEqual(name, tier.Name);
			Assert.AreEqual(link, tier.Link);
			Assert.AreEqual(icon, tier.IconPath);
			Assert.AreEqual(weight, tier.Weight);
		}

		[TestMethod]
		public async Task Create_Success_Returns201()
		{
			int createdId = 1;
			_mockTierService
				.Setup(m => m.Add(It.IsAny<Tier>()))
				.ReturnsAsync((createdId, TierEditResult.Success));

			var result = await _tiersController.Create(new TierAddEditRequest());

			Assert.IsNotNull(result);
			Assert.IsInstanceOfType(result, typeof(CreatedResult));
			var createdResult = (CreatedResult)result;
			Assert.AreEqual(201, createdResult.StatusCode);
			Assert.IsTrue(createdResult.Location.Contains(createdId.ToString()));
		}

		[TestMethod]
		public async Task Create_Duplicate_Returns409()
		{
			_mockTierService
				.Setup(m => m.Add(It.IsAny<Tier>()))
				.ReturnsAsync((1, TierEditResult.DuplicateName));

			var result = await _tiersController.Create(new TierAddEditRequest());

			Assert.IsNotNull(result);
			Assert.IsInstanceOfType(result, typeof(ConflictObjectResult));
			var conflictResult = (ConflictObjectResult)result;
			Assert.AreEqual(409, conflictResult.StatusCode);
			Assert.IsInstanceOfType(conflictResult.Value, typeof(SerializableError));
			var error = (SerializableError)conflictResult.Value;
			Assert.IsTrue(error.Count == 1);
			Assert.IsTrue(error.ContainsKey(nameof(TierAddEditRequest.Name)));
		}

		[TestMethod]
		public async Task Create_Fail_Returns400()
		{
			_mockTierService
				.Setup(m => m.Add(It.IsAny<Tier>()))
				.ReturnsAsync((1, TierEditResult.Fail));

			var result = await _tiersController.Create(new TierAddEditRequest());

			Assert.IsNotNull(result);
			Assert.IsInstanceOfType(result, typeof(BadRequestResult));
			Assert.AreEqual(400, ((BadRequestResult)result).StatusCode);
		}

		[TestMethod]
		public async Task Update_Success_Returns200()
		{
			_mockTierService
				.Setup(m => m.Edit(It.IsAny<int>(), It.IsAny<Tier>()))
				.ReturnsAsync(TierEditResult.Success);

			var result = await _tiersController.Update(1, new TierAddEditRequest());

			Assert.IsNotNull(result);
			Assert.IsInstanceOfType(result, typeof(OkResult));
			Assert.AreEqual(200, ((OkResult)result).StatusCode);
		}

		[TestMethod]
		public async Task Update_NotFound_Returns404()
		{
			_mockTierService
				.Setup(m => m.Edit(It.IsAny<int>(), It.IsAny<Tier>()))
				.ReturnsAsync(TierEditResult.NotFound);

			var result = await _tiersController.Update(1, new TierAddEditRequest());

			Assert.IsNotNull(result);
			Assert.IsInstanceOfType(result, typeof(NotFoundResult));
			Assert.AreEqual(404, ((NotFoundResult)result).StatusCode);
		}

		[TestMethod]
		public async Task Update_Duplicate_Returns409()
		{
			_mockTierService
				.Setup(m => m.Edit(It.IsAny<int>(), It.IsAny<Tier>()))
				.ReturnsAsync(TierEditResult.DuplicateName);

			var result = await _tiersController.Update(1, new TierAddEditRequest());

			Assert.IsNotNull(result);
			Assert.IsInstanceOfType(result, typeof(ConflictObjectResult));
			Assert.AreEqual(409, ((ConflictObjectResult)result).StatusCode);
			var conflictResult = (ConflictObjectResult)result;
			Assert.IsInstanceOfType(conflictResult.Value, typeof(SerializableError));
			var error = (SerializableError)conflictResult.Value;
			Assert.IsTrue(error.Count == 1);
			Assert.IsTrue(error.ContainsKey(nameof(TierAddEditRequest.Name)));
		}

		[TestMethod]
		public async Task Delete_Success_Returns200()
		{
			_mockTierService
				.Setup(m => m.Delete(It.IsAny<int>()))
				.ReturnsAsync(TierDeleteResult.Success);

			var result = await _tiersController.Delete(1);

			Assert.IsNotNull(result);
			Assert.IsInstanceOfType(result, typeof(OkResult));
			Assert.AreEqual(200, ((OkResult)result).StatusCode);
		}

		[TestMethod]
		public async Task Delete_NotFound_Returns404()
		{
			_mockTierService
				.Setup(m => m.Delete(It.IsAny<int>()))
				.ReturnsAsync(TierDeleteResult.NotFound);

			var result = await _tiersController.Delete(1);

			Assert.IsNotNull(result);
			Assert.IsInstanceOfType(result, typeof(NotFoundResult));
			Assert.AreEqual(404, ((NotFoundResult)result).StatusCode);
		}

		[TestMethod]
		public async Task Delete_InUse_Returns409()
		{
			_mockTierService
				.Setup(m => m.Delete(It.IsAny<int>()))
				.ReturnsAsync(TierDeleteResult.InUse);

			var result = await _tiersController.Delete(1);

			Assert.IsNotNull(result);
			Assert.IsInstanceOfType(result, typeof(ConflictObjectResult));
			Assert.AreEqual(409, ((ConflictObjectResult)result).StatusCode);
			var conflictResult = (ConflictObjectResult)result;
			Assert.IsInstanceOfType(conflictResult.Value, typeof(SerializableError));
			var error = (SerializableError)conflictResult.Value;
			Assert.IsTrue(error.Count == 1);
			Assert.IsTrue(error.ContainsKey(""));
		}

		public void Dispose()
		{
			_tiersController.Dispose();
		}
	}
}
