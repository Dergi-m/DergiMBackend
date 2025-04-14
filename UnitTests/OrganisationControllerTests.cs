using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using DergiMBackend.Controllers;
using DergiMBackend.Models;
using DergiMBackend.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using DergiMBackend.DbContext;
using Microsoft.EntityFrameworkCore.InMemory;

namespace DergiMBackend.Tests.Controllers
{
	[TestClass]
	public class OrganisationControllerTests
	{
		private ApplicationDbContext _dbContext;
		private Random _random;
		private Mock<IMapper> _mapperMock;
		private OrganisationController _organisationController;

		[TestInitialize]
		public void Setup()
		{
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}") // Unique database name for each test
				.Options;

			_dbContext = new ApplicationDbContext(options);
			_random = new Random();
			_mapperMock = new Mock<IMapper>();
			_organisationController = new OrganisationController(_dbContext, _mapperMock.Object);
		}

		[TestCleanup]
		public void Cleanup()
		{
			_dbContext.Organisation.RemoveRange(_dbContext.Organisation);
			_dbContext.SaveChanges();
		}

		[TestMethod]
		public async Task Get_ReturnsAllOrganisations()
		{
			// Arrange
			var id1 = _random.Next();
			var id2 = _random.Next();
			var organisations = new List<Organisation>
			{
				new Organisation { Id = id1, Name = "Org1", Description = "Description1" },
				new Organisation { Id = id2, Name = "Org2", Description = "Description2" }
			};
			await _dbContext.Organisation.AddRangeAsync(organisations);
			await _dbContext.SaveChangesAsync();

			var organisationDtos = new List<OrganisationDto>
			{
				new OrganisationDto { Id = id1, Name = "Org1", Description = "Description1" },
				new OrganisationDto { Id = id2, Name = "Org2", Description = "Description2" }
			};

			_mapperMock.Setup(m => m.Map<List<OrganisationDto>>(organisations)).Returns(organisationDtos);

			// Act
			var result = await _organisationController.Get();

			// Assert
			Assert.IsTrue(result.Success);
			Assert.AreEqual(organisationDtos, result.Result);
		}

		[TestMethod]
		public async Task Get_WithId_ReturnsOrganisation()
		{
			// Arrange
			var id = _random.Next();
			var organisation = new Organisation { Id = id, Name = "Org1", Description = "Description1" };
			await _dbContext.Organisation.AddAsync(organisation);
			await _dbContext.SaveChangesAsync();

			var organisationDto = new OrganisationDto { Id = id, Name = "Org1", Description = "Description1" };
			_mapperMock.Setup(m => m.Map<OrganisationDto>(organisation)).Returns(organisationDto);

			// Act
			var result = await _organisationController.Get(id);

			// Assert
			Assert.IsTrue(result.Success);
			Assert.AreEqual(organisationDto, result.Result);
		}

		[TestMethod]
		public async Task Create_AddsOrganisation()
		{
			// Arrange
			_dbContext.Organisation.RemoveRange(_dbContext.Organisation);
			await _dbContext.SaveChangesAsync();
			var organisationDto = new OrganisationDto { Name = "Org1", Description = "Description1" };
			var organisation = new Organisation { Name = "Org1", Description = "Description1" };

			_mapperMock.Setup(m => m.Map<Organisation>(organisationDto)).Returns(organisation);
			_mapperMock.Setup(m => m.Map<OrganisationDto>(organisation)).Returns(organisationDto);
			var count = _dbContext.Organisation.Count();

			// Act
			var result = await _organisationController.Create(organisationDto);

			// Assert
			Assert.IsTrue(result.Success);
			Assert.AreEqual(organisationDto, result.Result);
			Assert.AreEqual(count+1, _dbContext.Organisation.Count());
		}

		[TestMethod]
		public async Task Update_UpdatesOrganisation()
		{
			// Arrange
			var id = _random.Next();
			var organisation = new Organisation { Id = id, Name = "Org1", Description = "Old Description" };
			await _dbContext.Organisation.AddAsync(organisation);
			await _dbContext.SaveChangesAsync();

			var organisationDto = new OrganisationDto { Id = id, Name = "UpdatedOrg", Description = "Updated Description" };
			_mapperMock.Setup(m => m.Map<OrganisationDto>(organisation)).Returns(organisationDto);

			// Act
			var result = await _organisationController.Update(organisationDto);

			// Assert
			Assert.IsTrue(result.Success);
			var updatedOrganisation = await _dbContext.Organisation.FindAsync(id);
			Assert.AreEqual("UpdatedOrg", updatedOrganisation.Name);
			Assert.AreEqual("Updated Description", updatedOrganisation.Description);
		}

		[TestMethod]
		public async Task Delete_RemovesOrganisation()
		{
			// Arrange
			var id = _random.Next();
			var organisation = new Organisation { Id=id, Name = "Org1", Description = "Updated Description" };
			await _dbContext.Organisation.AddAsync(organisation);
			await _dbContext.SaveChangesAsync();
			var count = _dbContext.Organisation.Count();

			// Act

			var result = await _organisationController.Delete(id);

			// Assert
			Assert.IsTrue(result.Success);
			Assert.AreEqual(count-1, _dbContext.Organisation.Count());
		}
	}
}