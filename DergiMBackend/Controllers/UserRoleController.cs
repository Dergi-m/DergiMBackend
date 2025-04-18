using DergiMBackend.Models;
using DergiMBackend.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UserRoleController : ControllerBase
{
    private readonly IUserRoleService _roleService;

    public UserRoleController(IUserRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _roleService.GetAllRolesAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var role = await _roleService.GetRoleByIdAsync(id);
        return role is null ? NotFound() : Ok(role);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UserRole role)
    {
        var created = await _roleService.CreateRoleAsync(role);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UserRole role)
    {
        if (id != role.Id) return BadRequest();
        var updated = await _roleService.UpdateRoleAsync(role);
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await _roleService.DeleteRoleAsync(id);
        return success ? NoContent() : NotFound();
    }
}
