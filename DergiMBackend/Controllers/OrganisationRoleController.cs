using DergiMBackend.Models;
using DergiMBackend.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DergiMBackend.Controllers
{
    [Route("api/organisations/{organisationId:guid}/roles")]
    [ApiController]
    [Authorize]
    public class OrganisationRoleController : ControllerBase
    {
        private readonly IOrganisationRoleService _organisationRoleService;

        public OrganisationRoleController(IOrganisationRoleService organisationRoleService)
        {
            _organisationRoleService = organisationRoleService;
        }

        [HttpGet]
        public async Task<IActionResult> GetRoles(Guid organisationId)
        {
            var roles = await _organisationRoleService.GetRolesByOrganisationAsync(organisationId);
            return Ok(roles);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole(Guid organisationId, [FromBody] OrganisationRole role)
        {
            var createdRole = await _organisationRoleService.CreateRoleAsync(organisationId, role);
            return CreatedAtAction(nameof(GetRoles), new { organisationId = organisationId }, createdRole);
        }

        [HttpPut("{roleId:guid}")]
        public async Task<IActionResult> UpdateRole(Guid organisationId, Guid roleId, [FromBody] OrganisationRole role)
        {
            var updatedRole = await _organisationRoleService.UpdateRoleAsync(roleId, role);
            return Ok(updatedRole);
        }

        [HttpDelete("{roleId:guid}")]
        public async Task<IActionResult> DeleteRole(Guid organisationId, Guid roleId)
        {
            var success = await _organisationRoleService.DeleteRoleAsync(roleId);
            if (!success)
                return NotFound();

            return NoContent();
        }
    }
}
