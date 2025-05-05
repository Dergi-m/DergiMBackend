using AutoMapper;
using DergiMBackend.DbContext;
using DergiMBackend.Models;
using DergiMBackend.Models.Dtos;
using DergiMBackend.Services.IServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
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
        private readonly IMapper _mapper;
        private readonly ApiSettings _apiSettings;
        private readonly ISessionService _sessionService;

        public UserService(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager,
            IMapper mapper,
            ISessionService sessionService,
            IOptions<ApiSettings> apiSettings)
        {
            _db = db;
            _userManager = userManager;
            _mapper = mapper;
            _sessionService = sessionService;
            _apiSettings = apiSettings.Value;
        }

        public async Task<SessionDto> LoginAsync(LoginRequestDto loginRequestDto)
        {
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.UserName!.ToLower() == loginRequestDto.UserName.ToLower());

            if (user == null || !await _userManager.CheckPasswordAsync(user, loginRequestDto.Password))
                throw new UnauthorizedAccessException("Invalid credentials.");

            var sessionToken = await GenerateSessionTokenAsync(user);

            return new SessionDto
            {
                SessionToken = sessionToken,
                User = _mapper.Map<UserDto>(user),
                Organisations = new List<OrganisationMembershipDto>()
            };
        }

        public async Task<SessionDto> RegisterAsync(RegistrationRequestDto registrationRequest)
        {
            if (!await IsUserUniqueAsync(registrationRequest.UserName, registrationRequest.Email))
                throw new InvalidOperationException("Username or email already exists.");

            var organisation = await _db.Organisations.FirstOrDefaultAsync(o => o.UniqueName == "dergim");
            if (organisation == null)
            {
                organisation = new Organisation
                {
                    UniqueName = "dergim",
                    Name = "DergiM"
                };
                _db.Organisations.Add(organisation);
                await _db.SaveChangesAsync();
            }

            if (organisation == null) throw new Exception("No organisation");

            // Ensure "Member" role exists
            var memberRole = await _db.OrganisationRoles.FirstOrDefaultAsync(r =>
                r.OrganisationId == organisation.Id && r.Name == "Member");

            if (memberRole == null)
            {
                memberRole = new OrganisationRole
                {
                    OrganisationId = organisation.Id,
                    Name = "Member",
                    Description = "Default member role"
                };
                _db.OrganisationRoles.Add(memberRole);
                await _db.SaveChangesAsync();
            }

            if (memberRole == null) throw new Exception("No role");


            var newUser = new ApplicationUser
            {
                UserName = registrationRequest.UserName,
                Email = registrationRequest.Email,
                Name = registrationRequest.Name,
            };

            var createResult = await _userManager.CreateAsync(newUser, registrationRequest.Password);

            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException(string.Join("; ", createResult.Errors.Select(e => e.Description)));
            }

            
            await _userManager.AddToRoleAsync(newUser, SD.RoleUSER); // Always assign basic role

            _db.OrganisationMemberships.Add(new OrganisationMembership
            {
                OrganisationId = organisation.Id,
                RoleId = memberRole.Id,
                UserId = newUser.Id
            });

            await _db.SaveChangesAsync();

            var token = await _sessionService.GenerateSessionTokenAsync(newUser);

            return new SessionDto
            {
                SessionToken = token,
                User = _mapper.Map<UserDto>(newUser),
                Organisations = new List<OrganisationMembershipDto>()
            };
        }



        public async Task<UserDto> GetUserAsync(string uid)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == uid)
                ?? throw new Exception("User not found");

            return _mapper.Map<UserDto>(user);
        }

        public async Task<IEnumerable<UserDto>> GetUsersAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            return _mapper.Map<List<UserDto>>(users);
        }

        public async Task<bool> IsUserUniqueAsync(string username, string email)
        {
            return !await _userManager.Users.AnyAsync(u => u.UserName == username || u.Email == email);
        }

        private Task<string> GenerateSessionTokenAsync(ApplicationUser user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_apiSettings.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id ?? string.Empty),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty)
            };

            var token = new JwtSecurityToken(
                issuer: _apiSettings.Issuer,
                audience: _apiSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
        }
        public async Task<ApplicationUser> GetUserEntityByIdAsync(string id)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id)
                ?? throw new Exception("User not found");

            return user;
        }
    }
}
