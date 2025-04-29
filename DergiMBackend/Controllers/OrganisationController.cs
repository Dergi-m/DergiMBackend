using DergiMBackend.Authorization;
using DergiMBackend.Models;
using DergiMBackend.Models.Dtos;
using DergiMBackend.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DergiMBackend.Controllers
{
    [Route("api/organisations")]
    [ApiController]
    public class OrganisationController : ControllerBase
    {
        private readonly IOrganisationService _organisationService;
        private readonly IUserService _userService;

        public OrganisationController(
            IOrganisationService organisationService,
            IUserService userService)
        {
            _organisationService = organisationService;
            _userService = userService;
        }

        [HttpGet]
        [SessionAuthorize]
        public async Task<IActionResult> GetAll()
        {
            var organisations = await _organisationService.GetAllOrganisationsAsync();
            return Ok(organisations);
        }

        [HttpGet("{id:guid}")]
        [SessionAuthorize]
        public async Task<IActionResult> Get(Guid id)
        {
            var organisation = await _organisationService.GetOrganisationByIdAsync(id);
            if (organisation == null)
                return NotFound();

            return Ok(organisation);
        }

        [HttpPost]
        [SessionAuthorize]
        public async Task<IActionResult> Create([FromBody] CreateOrganisationDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = HttpContext.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var user = await _userService.GetUserEntityByIdAsync(userId);
            if (user == null)
                return Unauthorized();

            var createdOrganisation = await _organisationService.CreateOrganisationAsync(
                uniqueName: dto.UniqueName,
                name: dto.Name,
                description: dto.Description,
                owner: user
            );

            return CreatedAtAction(nameof(Get), new { id = createdOrganisation.Id }, createdOrganisation);
        }

        [HttpPut("{id:guid}")]
        [SessionAuthorize]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateOrganisationDto dto)
        {
            var updated = await _organisationService.UpdateOrganisationAsync(id, dto.Name, dto.Description);
            if (updated == null)
                return NotFound();

            return Ok(updated);
        }

        [HttpDelete("{id:guid}")]
        [SessionAuthorize]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _organisationService.DeleteOrganisationAsync(id);
            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }
}
