using AutoMapper;
using DergiMBackend.DbContext;
using DergiMBackend.Models;
using DergiMBackend.Models.Dtos;
using DergiMBackend.Services.IServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DergiMBackend.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMapper _mapper;
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;

        public UserService(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            IMapper mapper)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;

            _secretKey = configuration["ApiSettings:SecretKey"] ?? throw new InvalidOperationException("Secret is missing in config.");
            _issuer = configuration["ApiSettings:Issuer"] ?? throw new InvalidOperationException("Issuer is missing in config.");
            _audience = configuration["ApiSettings:Audience"] ?? throw new InvalidOperationException("Audience is missing in config.");
        }

        public bool IsUniqueUser(string username)
        {
            return !_db.Users.Any(u => u.UserName == username);
        }

        public async Task<SessionDto> Login(LoginRequestDto loginRequestDto)
        {
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.UserName!.ToLower() == loginRequestDto.UserName.ToLower());

            if (user == null)
                throw new InvalidOperationException("Invalid username or password.");

            var organisation = await _db.Organisations
                .FirstOrDefaultAsync(o => o.UniqueName == loginRequestDto.OrganisationUniqueName);

            if (organisation == null)
                throw new InvalidOperationException("Organisation does not exist.");

            if (user.OrganisationUniqueName != organisation.UniqueName)
                throw new UnauthorizedAccessException("User does not belong to this organisation.");

            if (!await _userManager.CheckPasswordAsync(user, loginRequestDto.Password))
                throw new UnauthorizedAccessException("Invalid username or password.");

            var sessionToken = await GenerateSessionTokenAsync(user);

            return new SessionDto
            {
                SessionToken = sessionToken,
                User = _mapper.Map<UserDto>(user)
            };
        }

        public async Task<UserDto> Register(RegistrationRequestDto registrationRequestDto)
        {
            var organisation = await _db.Organisations.FirstOrDefaultAsync(o => o.UniqueName == registrationRequestDto.OrganisationUniqueName)
                ?? throw new InvalidOperationException("Organisation not found.");

            var user = new ApplicationUser
            {
                UserName = registrationRequestDto.UserName,
                Email = registrationRequestDto.UserName,
                NormalizedEmail = registrationRequestDto.UserName.ToUpper(),
                Name = registrationRequestDto.Name,
                OrganisationUniqueName = organisation.UniqueName,
            };

            var result = await _userManager.CreateAsync(user, registrationRequestDto.Password);
            if (!result.Succeeded)
                throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));

            // Assign default role
            await EnsureRolesExist();
            await _userManager.AddToRoleAsync(user, SD.RoleUSER);

            return _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto> AssignUserToRole(string userName, string roleName)
        {
            var user = await _userManager.FindByNameAsync(userName) ?? throw new InvalidDataException("User not found");

            if (!await _roleManager.RoleExistsAsync(roleName))
                throw new InvalidDataException("Role does not exist.");

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, roleName);

            return _mapper.Map<UserDto>(user);
        }

        public async Task<IEnumerable<UserDto>> GetUsersAsync(string? organisationUniqueName = null)
        {
            var query = _db.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(organisationUniqueName))
            {
                query = query.Where(u => u.OrganisationUniqueName == organisationUniqueName);
            }

            var users = await query.ToListAsync();
            return _mapper.Map<List<UserDto>>(users);
        }

        public async Task<UserDto> GetUserAsync(string username)
        {
            var user = await _userManager.FindByNameAsync(username)
                ?? throw new InvalidDataException("User not found.");

            return _mapper.Map<UserDto>(user);
        }

        public async Task<bool> AssignUserToOrganisation(ApplicationUser user)
        {
            var existingUser = await _db.Users.FirstOrDefaultAsync(u => u.UserName == user.UserName);

            if (existingUser == null)
                throw new InvalidDataException("User not found.");

            if (!string.IsNullOrEmpty(existingUser.OrganisationUniqueName))
                throw new InvalidOperationException("User is already assigned to an organisation.");

            existingUser.OrganisationUniqueName = user.OrganisationUniqueName;
            _db.Users.Update(existingUser);
            await _db.SaveChangesAsync();

            return true;
        }

        private async Task<string> GenerateSessionTokenAsync(ApplicationUser user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim("Organisation", user.OrganisationUniqueName ?? string.Empty),
            };

            var userRoles = await _userManager.GetRolesAsync(user);
            claims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task EnsureRolesExist()
        {
            if (!await _roleManager.RoleExistsAsync(SD.RoleADMIN))
                await _roleManager.CreateAsync(new IdentityRole(SD.RoleADMIN));

            if (!await _roleManager.RoleExistsAsync(SD.RoleUSER))
                await _roleManager.CreateAsync(new IdentityRole(SD.RoleUSER));
        }
    }
}