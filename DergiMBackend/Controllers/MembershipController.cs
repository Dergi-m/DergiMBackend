using DergiMBackend.Authorization;
using DergiMBackend.Models;
using DergiMBackend.Models.Dtos;
using DergiMBackend.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace DergiMBackend.Controllers
{
    [Route("api/memberships")]
    [ApiController]
    public class MembershipController : ControllerBase
    {
        private readonly IOrganisationMembershipService _membershipService;

        public MembershipController(IOrganisationMembershipService membershipService)
        {
            _membershipService = membershipService;
        }

        [HttpGet("organisation/{organisationId:guid}")]
        [SessionAuthorize]
        public async Task<IActionResult> GetMembershipsForOrganisation(Guid organisationId)
        {
            var memberships = await _membershipService.GetMembershipsForOrganisationAsync(organisationId);
            return Ok(memberships);
        }

        [HttpGet("user/{userId}")]
        [SessionAuthorize]
        public async Task<IActionResult> GetMembershipsForUser(string userId)
        {
            var memberships = await _membershipService.GetMembershipsForUserAsync(userId);
            return Ok(memberships);
        }

        [HttpGet("{id:guid}")]
        [SessionAuthorize]
        public async Task<IActionResult> GetMembershipById(Guid id)
        {
            var membership = await _membershipService.GetMembershipByIdAsync(id);
            if (membership == null) return NotFound();
            return Ok(membership);
        }

        [HttpPost]
        [SessionAuthorize]
        public async Task<IActionResult> CreateMembership([FromBody] CreateMembershipDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var membership = new OrganisationMembership
            {
                UserId = dto.UserId,
                OrganisationId = dto.OrganisationId,
                RoleId = dto.RoleId
            };

            var createdMembership = await _membershipService.CreateMembershipAsync(membership);
            return CreatedAtAction(nameof(GetMembershipById), new { id = createdMembership.Id }, createdMembership);
        }

        [HttpDelete("{id:guid}")]
        [SessionAuthorize]
        public async Task<IActionResult> DeleteMembership(Guid id)
        {
            var deleted = await _membershipService.DeleteMembershipAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}
