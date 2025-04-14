using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using DergiMBackend.Controllers;
using DergiMBackend.Services.IServices;
using DergiMBackend.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net;
using DergiMBackend.Models;

namespace DergiMBackend.Tests.Controllers
{
	[TestClass]
	public class UserControllerTests
	{
		private Mock<IUserService> _userServiceMock;
		private UserController _userController;

		[TestInitialize]
		public void Setup()
		{
			_userServiceMock = new Mock<IUserService>();
			_userController = new UserController(_userServiceMock.Object);
		}

		[TestMethod]
		public async Task Get_WithOrganisationId_ReturnsUsers()
		{
			// Arrange
			var organisationId = 1;
			var users = new List<UserDto> { new UserDto { Id = "valid", UserName = "TestUser" } };
			_userServiceMock.Setup(s => s.GetUsersAsync(organisationId)).ReturnsAsync(users);

			// Act
			var result = await _userController.Get(organisationId);

			// Assert
			Assert.IsTrue(result.Success);
			Assert.AreEqual(users, result.Result);
		}

		[TestMethod]
		public async Task Get_WithUsername_ReturnsUser()
		{
			// Arrange
			var username = "TestUser";
			var user = new UserDto { Id = "valid", UserName = username };
			_userServiceMock.Setup(s => s.GetUserAsync(username)).ReturnsAsync(user);

			// Act
			var result = await _userController.Get(username);

			// Assert
			Assert.IsTrue(result.Success);
			Assert.AreEqual(user, result.Result);
		}

		[TestMethod]
		public async Task Login_WithValidCredentials_ReturnsToken()
		{
			// Arrange
			var loginRequest = new LoginRequestDto { UserName = "TestUser", Password = "Password123" };
			var tokenDto = new TokenDto { AccessToken = "ValidToken" };
			_userServiceMock.Setup(s => s.Login(loginRequest)).ReturnsAsync(tokenDto);

			// Act
			var result = await _userController.Login(loginRequest) as OkObjectResult;

			// Assert
			Assert.IsNotNull(result);
			Assert.AreEqual((int)HttpStatusCode.OK, result.StatusCode);
		}

		[TestMethod]
		public async Task Register_WithUniqueUsername_ReturnsSuccess()
		{
			// Arrange
			var registrationRequest = new RegistrationRequestDto { UserName = "UniqueUser" };
			_userServiceMock.Setup(s => s.IsUniqueUser(registrationRequest.UserName)).Returns(true);
			_userServiceMock.Setup(s => s.Register(registrationRequest)).ReturnsAsync(new UserDto());

			// Act
			var result = await _userController.Register(registrationRequest) as OkObjectResult;

			// Assert
			Assert.IsNotNull(result);
			Assert.AreEqual((int)HttpStatusCode.OK, result.StatusCode);
		}

		[TestMethod]
		public async Task AssignUserToRole_WithValidData_ReturnsSuccess()
		{
			// Arrange
			var registrationRequest = new RegistrationRequestDto { UserName = "TestUser" };
			_userServiceMock.Setup(s => s.AssignUserToRole(registrationRequest)).ReturnsAsync(new UserDto());

			// Act
			var result = await _userController.AssignUserToRole(registrationRequest) as OkObjectResult;

			// Assert
			Assert.IsNotNull(result);
			Assert.AreEqual((int)HttpStatusCode.OK, result.StatusCode);
		}

		[TestMethod]
		public async Task AssignUserToOrganisation_WithValidData_ReturnsSuccess()
		{
			// Arrange
			var user = new ApplicationUser { Id = "valid", UserName = "TestUser" };
			_userServiceMock.Setup(s => s.AssignUserToOrganisation(user)).ReturnsAsync(true);

			// Act
			var result = await _userController.AssignUserToOrganisation(user) as OkObjectResult;

			// Assert
			Assert.IsNotNull(result);
			Assert.AreEqual((int)HttpStatusCode.OK, result.StatusCode);
		}
	}
}
