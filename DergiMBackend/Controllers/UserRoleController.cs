using DergiMBackend.Models;
using DergiMBackend.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DergiMBackend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/organisations/{organisationUniqueName}/roles")]
    public class UserRoleController : ControllerBase
    {
        private readonly IUserRoleService _roleService;

        public UserRoleController(IUserRoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(string organisationUniqueName)
        {
            var roles = await _roleService.GetAllRolesAsync(organisationUniqueName);
            return Ok(roles);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string organisationUniqueName, Guid id)
        {
            var role = await _roleService.GetRoleByIdAsync(id);
            if (role == null || role.OrganisationUniqueName != organisationUniqueName)
                return NotFound();

            return Ok(role);
        }

        [HttpPost]
        public async Task<IActionResult> Create(string organisationUniqueName, [FromBody] UserRole role)
        {
            if (role.OrganisationUniqueName != organisationUniqueName)
                return BadRequest("Organisation mismatch.");

            var created = await _roleService.CreateRoleAsync(role);
            return CreatedAtAction(nameof(Get), new { organisationUniqueName = created.OrganisationUniqueName, id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string organisationUniqueName, Guid id, [FromBody] UserRole role)
        {
            if (id != role.Id || role.OrganisationUniqueName != organisationUniqueName)
                return BadRequest("Invalid role update request.");

            var updated = await _roleService.UpdateRoleAsync(role);
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string organisationUniqueName, Guid id)
        {
            var role = await _roleService.GetRoleByIdAsync(id);
            if (role == null || role.OrganisationUniqueName != organisationUniqueName)
                return NotFound();

            var success = await _roleService.DeleteRoleAsync(id);
            return success ? NoContent() : NotFound();
        }
    }
}