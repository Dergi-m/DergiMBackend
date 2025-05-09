using AutoMapper;
using DergiMBackend.Controllers;
using DergiMBackend.DbContext;
using DergiMBackend.Models;
using DergiMBackend.Models.Dtos;
using DergiMBackend.Services.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DergiMBackend.Tests.Controllers
{
    public class OrganisationControllerTests
    {
        private readonly Mock<ApplicationDbContext> _mockDb;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ISessionService> _mockSessionService;
        private readonly OrganisationController _controller;

        public OrganisationControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "OrganisationTestDb")
                .Options;

            _mockDb = new Mock<ApplicationDbContext>(options);
            _mockMapper = new Mock<IMapper>();
            _mockSessionService = new Mock<ISessionService>();

            _controller = new OrganisationController(_mockDb.Object, _mockMapper.Object, _mockSessionService.Object);

            var context = new DefaultHttpContext();
            context.Request.Headers["SessionToken"] = "dummy-token";
            _controller.ControllerContext.HttpContext = context;
        }

        [Fact]
        public async Task GetAllOrganisations_ReturnsOk()
        {
            // Arrange
            _mockSessionService.Setup(x => x.ValidateSessionToken(It.IsAny<string>())).Returns("User");

            _mockDb.Setup(db => db.Organisations.ToListAsync(default))
                .ReturnsAsync(new List<Organisation> { new Organisation { UniqueName = "org1", Name = "Org 1" } });

            _mockMapper.Setup(x => x.Map<List<OrganisationDto>>(It.IsAny<List<Organisation>>()))
                .Returns(new List<OrganisationDto> { new OrganisationDto { UniqueName = "org1", Name = "Org 1" } });

            // Act
            var result = await _controller.GetAllOrganisations();

            // Assert
            Assert.True(result.Success);
        }

        [Fact]
        public async Task GetOrganisation_ReturnsOk()
        {
            // Arrange
            string uniqueName = "org1";
            _mockSessionService.Setup(x => x.ValidateSessionToken(It.IsAny<string>())).Returns("User");

            _mockDb.Setup(db => db.Organisations.FirstOrDefaultAsync(x => x.UniqueName == uniqueName, default))
                .ReturnsAsync(new Organisation { UniqueName = uniqueName, Name = "Org 1" });

            _mockMapper.Setup(x => x.Map<OrganisationDto>(It.IsAny<Organisation>()))
                .Returns(new OrganisationDto { UniqueName = uniqueName, Name = "Org 1" });

            // Act
            var result = await _controller.GetOrganisation(uniqueName);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Result);
        }

        [Fact]
        public async Task CreateOrganisation_AdminOnly_ReturnsCreated()
        {
            // Arrange
            _mockSessionService.Setup(x => x.ValidateSessionToken(It.IsAny<string>())).Returns("Admin");

            var organisationDto = new OrganisationDto { UniqueName = "org2", Name = "Org 2" };

            _mockMapper.Setup(x => x.Map<Organisation>(It.IsAny<OrganisationDto>()))
                .Returns(new Organisation { UniqueName = "org2", Name = "Org 2" });

            // Act
            var result = await _controller.CreateOrganisation(organisationDto);

            // Assert
            Assert.True(result.Success);
        }

        [Fact]
        public async Task UpdateOrganisation_AdminOnly_ReturnsOk()
        {
            // Arrange
            string uniqueName = "org1";
            _mockSessionService.Setup(x => x.ValidateSessionToken(It.IsAny<string>())).Returns("Admin");

            _mockDb.Setup(db => db.Organisations.FirstOrDefaultAsync(x => x.UniqueName == uniqueName, default))
                .ReturnsAsync(new Organisation { UniqueName = uniqueName, Name = "Org 1" });

            // Act
            var organisationDto = new OrganisationDto { UniqueName = uniqueName, Name = "Org 1 Updated" };
            var result = await _controller.UpdateOrganisation(organisationDto);

            // Assert
            Assert.True(result.Success);
        }

        [Fact]
        public async Task DeleteOrganisation_AdminOnly_ReturnsNoContent()
        {
            // Arrange
            string uniqueName = "org1";
            _mockSessionService.Setup(x => x.ValidateSessionToken(It.IsAny<string>())).Returns("Admin");

            _mockDb.Setup(db => db.Organisations.FirstOrDefaultAsync(x => x.UniqueName == uniqueName, default))
                .ReturnsAsync(new Organisation { UniqueName = uniqueName, Name = "Org 1" });

            // Act
            var result = await _controller.DeleteOrganisation(uniqueName);

            // Assert
            Assert.True(result.Success);
        }
    }
}