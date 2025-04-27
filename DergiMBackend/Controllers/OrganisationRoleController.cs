using DergiMBackend.Authorization;
using DergiMBackend.Models;
using DergiMBackend.Models.Dtos;
using DergiMBackend.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace DergiMBackend.Controllers
{
    [Route("api/organisations/{organisationId:guid}/roles")]
    [ApiController]
    public class OrganisationRoleController : ControllerBase
    {
        private readonly IOrganisationRoleService _roleService;

        public OrganisationRoleController(IOrganisationRoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpGet]
        [SessionAuthorize]
        public async Task<IActionResult> GetRoles(Guid organisationId)
        {
            var roles = await _roleService.GetRolesForOrganisationAsync(organisationId);
            return Ok(roles);
        }

        [HttpGet("{roleId:guid}")]
        [SessionAuthorize]
        public async Task<IActionResult> GetRole(Guid roleId)
        {
            var role = await _roleService.GetRoleByIdAsync(roleId);
            if (role == null)
                return NotFound();

            return Ok(role);
        }

        [HttpPost]
        [SessionAuthorize]
        public async Task<IActionResult> CreateRole(Guid organisationId, [FromBody] CreateOrganisationRoleDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var newRole = new OrganisationRole
            {
                Name = dto.Name,
                Description = dto.Description,
                CanAssignTasks = dto.CanAssignTasks,
                CanCreateTasks = dto.CanCreateTasks,
                VisibleTags = dto.VisibleTags,
                OrganisationId = organisationId
            };

            var createdRole = await _roleService.CreateRoleAsync(organisationId, newRole);
            return CreatedAtAction(nameof(GetRole), new { roleId = createdRole.Id }, createdRole);
        }

        [HttpPut("{roleId:guid}")]
        [SessionAuthorize]
        public async Task<IActionResult> UpdateRole(Guid roleId, [FromBody] UpdateOrganisationRoleDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var roleToUpdate = new OrganisationRole
            {
                Id = roleId,
                Name = dto.Name,
                Description = dto.Description,
                CanAssignTasks = dto.CanAssignTasks,
                CanCreateTasks = dto.CanCreateTasks,
                VisibleTags = dto.VisibleTags
            };

            var updatedRole = await _roleService.UpdateRoleAsync(roleToUpdate);
            return Ok(updatedRole);
        }

        [HttpDelete("{roleId:guid}")]
        [SessionAuthorize]
        public async Task<IActionResult> DeleteRole(Guid roleId)
        {
            var deleted = await _roleService.DeleteRoleAsync(roleId);
            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }
}
