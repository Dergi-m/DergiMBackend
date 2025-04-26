using DergiMBackend.Authorization;
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

        [HttpGet("{organisationId:guid}")]
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

        [HttpGet("{id:guid}/single")]
        [SessionAuthorize]
        public async Task<IActionResult> GetMembershipById(Guid id)
        {
            var membership = await _membershipService.GetMembershipByIdAsync(id);
            return membership is null ? NotFound() : Ok(membership);
        }

        [HttpPost]
        [SessionAuthorize]
        public async Task<IActionResult> CreateMembership([FromBody] CreateMembershipDto dto)
        {
            var created = await _membershipService.CreateMembershipAsync(dto);
            return CreatedAtAction(nameof(GetMembershipById), new { id = created.Id }, created);
        }

        [HttpDelete("{id:guid}")]
        [SessionAuthorize]
        public async Task<IActionResult> DeleteMembership(Guid id)
        {
            var success = await _membershipService.DeleteMembershipAsync(id);
            return success ? NoContent() : NotFound();
        }
    }
}
