using AutoMapper;
using DergiMBackend.DbContext;
using DergiMBackend.Models;
using DergiMBackend.Models.Dtos;
using DergiMBackend.Services.IServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace DergiMBackend.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ISessionService _sessionService;
        private readonly IMapper _mapper;

        public UserService(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ISessionService sessionService,
            IMapper mapper)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
            _sessionService = sessionService;
            _mapper = mapper;
        }

        public async Task<SessionDto> LoginAsync(LoginRequestDto request)
        {
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.UserName!.ToLower() == request.UserName.ToLower());

            if (user == null)
                throw new UnauthorizedAccessException("Invalid username or password.");

            var validPassword = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!validPassword)
                throw new UnauthorizedAccessException("Invalid username or password.");

            var sessionToken = await _sessionService.GenerateSessionTokenAsync(user);

            return new SessionDto
            {
                SessionToken = sessionToken,
                User = _mapper.Map<UserDto>(user)
            };
        }

        public async Task<SessionDto> RegisterAsync(RegistrationRequestDto request)
        {
            var user = new ApplicationUser
            {
                UserName = request.UserName,
                Email = request.UserName,
                NormalizedEmail = request.UserName.ToUpper(),
                Name = request.Name,
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
                throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));

            await EnsureRolesExist();
            await _userManager.AddToRoleAsync(user, SD.RoleUSER);

            var sessionToken = await _sessionService.GenerateSessionTokenAsync(user);

            return new SessionDto
            {
                SessionToken = sessionToken,
                User = _mapper.Map<UserDto>(user)
            };
        }

        public async Task<IEnumerable<UserDto>> GetUsersAsync(string? organisationUniqueName = null)
        {
            var usersQuery = _db.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(organisationUniqueName))
            {
                usersQuery = usersQuery
                    .Where(u => _db.OrganisationMemberships
                        .Any(m => m.UserId == u.Id && m.Organisation.UniqueName == organisationUniqueName));
            }

            var users = await usersQuery.ToListAsync();
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }

        public async Task<UserDto> GetUserAsync(string username)
        {
            var user = await _userManager.FindByNameAsync(username)
                ?? throw new InvalidOperationException("User not found.");

            return _mapper.Map<UserDto>(user);
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
