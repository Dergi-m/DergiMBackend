using DergiMBackend.DbContext;
using DergiMBackend.Models;
using DergiMBackend.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DergiMBackend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/organisations")]
    public class OrganisationController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public OrganisationController(ApplicationDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// List all organisations.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var organisations = await _db.Organisations.ToListAsync();
            return Ok(organisations);
        }

        /// <summary>
        /// Get a single organisation by UniqueName.
        /// </summary>
        [HttpGet("{uniqueName}")]
        public async Task<IActionResult> Get(string uniqueName)
        {
            var organisation = await _db.Organisations.FirstOrDefaultAsync(o => o.UniqueName == uniqueName);
            if (organisation == null)
                return NotFound("Organisation not found.");

            return Ok(organisation);
        }

        /// <summary>
        /// Create a new organisation.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Organisation organisation)
        {
            if (await _db.Organisations.AnyAsync(o => o.UniqueName == organisation.UniqueName))
                return BadRequest("Organisation with this unique name already exists.");

            await _db.Organisations.AddAsync(organisation);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { uniqueName = organisation.UniqueName }, organisation);
        }

        /// <summary>
        /// Update an organisation's basic details.
        /// </summary>
        [HttpPut("{uniqueName}")]
        public async Task<IActionResult> Update(string uniqueName, [FromBody] Organisation updatedOrganisation)
        {
            var organisation = await _db.Organisations.FirstOrDefaultAsync(o => o.UniqueName == uniqueName);
            if (organisation == null)
                return NotFound("Organisation not found.");

            organisation.Name = updatedOrganisation.Name;
            organisation.Description = updatedOrganisation.Description;

            _db.Organisations.Update(organisation);
            await _db.SaveChangesAsync();
            return Ok(organisation);
        }

        /// <summary>
        /// Delete an organisation.
        /// </summary>
        [HttpDelete("{uniqueName}")]
        public async Task<IActionResult> Delete(string uniqueName)
        {
            var organisation = await _db.Organisations.FirstOrDefaultAsync(o => o.UniqueName == uniqueName);
            if (organisation == null)
                return NotFound("Organisation not found.");

            _db.Organisations.Remove(organisation);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
