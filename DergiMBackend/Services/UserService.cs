using AutoMapper;
using DergiMBackend.DbContext;
using DergiMBackend.Models.Dtos;
using DergiMBackend.Models;
using DergiMBackend.Services.IServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace DergiMBackend.Services
{
	public class UserService : IUserService
	{
		private readonly ApplicationDbContext _db;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly RoleManager<IdentityRole> _roleManager;
		private readonly IMapper _mapper;
		private string secretKey;
		private string issuer;
		private string audience;

		public UserService(ApplicationDbContext db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration, IMapper mapper)
		{
			_db = db;
			_userManager = userManager;
			_roleManager = roleManager;
			secretKey = configuration.GetValue<string>("ApiSettings:Secret") ?? throw new InvalidOperationException("Secret is not configured."); ;
			issuer = configuration.GetValue<string>("ApiSettings:Issuer") ?? throw new InvalidOperationException("Issuer is not configured."); ;
			audience = configuration.GetValue<string>("ApiSettings:Audience") ?? throw new InvalidOperationException("Audience is not configured."); ;
			_mapper = mapper;
		}

		public bool IsUniqueUser(string username)
		{
			var user = _db.Users.FirstOrDefault(u => u.UserName == username);
			if (user == null)
			{
				return true;
			}
			return false;
		}

		public async Task<SessionDto> Login(LoginRequestDto loginRequestDto)
		{
			var user = await _db.Users.FirstOrDefaultAsync(user => user.UserName!.ToLower() == loginRequestDto.UserName.ToLower());
			var organisation = await _db.Organisation.FirstOrDefaultAsync(org => org.UniqueName == loginRequestDto.OrganisationUniqueName);

            if (user == null)
                throw new InvalidOperationException("User not found");

            if (organisation == null)
                throw new InvalidOperationException("Organisation not found");

            if (user.OrganisationUniqueName != organisation.UniqueName) 
				throw new InvalidOperationException("User not found in this organisation");

            bool isValid = await _userManager.CheckPasswordAsync(user, loginRequestDto.Password);

			var tokenid = $"JWI{Guid.NewGuid()}";
			var accessToken = await GetSessionTokenAsync(user, tokenid);

            SessionDto tokendtoDto = new SessionDto()
			{
				SessionToken = accessToken,
				User = new()
				{
					Id = user.Id,
					Name = user.Name,
					UserName = loginRequestDto.UserName,
					Role = user.Role,
                    OrganisationUniqueName = loginRequestDto.OrganisationUniqueName,
				},
			};
			return tokendtoDto;
		}

		private async Task<string> GetSessionTokenAsync(ApplicationUser applicationUser, string tokenid)
		{
			var roles = await _userManager.GetRolesAsync(applicationUser);
			var tokenhandler = new JwtSecurityTokenHandler();
			var key = Encoding.ASCII.GetBytes(secretKey);
			var tokenDescriptor = new SecurityTokenDescriptor
			{
				Subject = new ClaimsIdentity(new Claim[]
				{
					new Claim(ClaimTypes.Name,applicationUser.Id.ToString()),
					new Claim(ClaimTypes.Role,applicationUser.Role.Name),
					new Claim(JwtRegisteredClaimNames.Jti, tokenid),
					new Claim(JwtRegisteredClaimNames.Sub,applicationUser.Id)
				}),
				Expires = DateTime.UtcNow.AddHours(2),
				SigningCredentials = new(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
				Issuer = issuer,
				Audience = audience
			};

			var token = tokenhandler.CreateToken(tokenDescriptor);
			return tokenhandler.WriteToken(token);
		}

		public async Task<UserDto> Register(RegistrationRequestDto registrationRequestDto)
		{
			var role = _db.UserRoles.FirstOrDefaultAsync(
				r => r.Id.ToString() == registrationRequestDto.Role.Id.ToString() &&
				r.OrganisationUniqueName == registrationRequestDto.OrganisationUniqueName) ?? throw new InvalidDataException("Role not found");

            ApplicationUser user = new()
			{
				Name = registrationRequestDto.Name,
				Role = role,
				Email = registrationRequestDto.UserName,
				NormalizedEmail = registrationRequestDto.UserName.ToUpper(),
				UserName = registrationRequestDto.UserName,
			};
			try
			{
				var result = await _userManager.CreateAsync(user, registrationRequestDto.Password);
				if (result.Succeeded)
				{
					if (!await _roleManager.RoleExistsAsync(SD.RoleUSER))
					{
						await _roleManager.CreateAsync(new IdentityRole(SD.RoleADMIN));
						await _roleManager.CreateAsync(new IdentityRole(SD.RoleUSER));
					}
					await _userManager.AddToRoleAsync(user, SD.RoleUSER);
					var userToReturn = _db.Users.FirstOrDefault(u => u.UserName == registrationRequestDto.UserName);
					return _mapper.Map<UserDto>(userToReturn);
				}
			}
			catch(Exception ex)
			{
				throw new Exception(ex.Message);
			}

			return null!;
		}
		public async Task<UserDto> AssignUserToRole(string userName, string roleId)
		{
			try
			{
				var user = await _db.Users.FirstOrDefaultAsync(u => u.UserName == userName) ?? throw new InvalidDataException("User not found");
				var role = await _db.UserRoles.FirstOrDefaultAsync(r => r.Id.ToString() == roleId) ?? throw new InvalidDataException("Role not found");

				var userToReturn = _db.Users.FirstOrDefault(u => u.UserName == userName);
				return _mapper.Map<UserDto>(userToReturn);
				
			}
			catch 
			{
            }

			return null!;
		}

		public async Task<IEnumerable<UserDto>> GetUsersAsync(string? organisationUniqueName = null)
		{
			var users = await _db.Users.ToListAsync();
			var result = new List<UserDto>();

			if (organisationUniqueName != null)
			{
				users = users.Where(u => u.OrganisationUniqueName == organisationUniqueName).ToList();
			}

			foreach (var user in users)
			{
				var userDto = _mapper.Map<UserDto>(user);
				result.Add(userDto);
			}

			return result;
		}


		public async Task<UserDto> GetUserAsync(string username)
		{
			var user = await _db.Users.FirstOrDefaultAsync(u => u.UserName == username);
			var result = _mapper.Map<UserDto>(user);
			return result;
		}

		public async Task<bool> AssignUserToOrganisation(ApplicationUser user)
		{
			try
			{
				var userFromDb = await _db.Users.FirstOrDefaultAsync(u => u.UserName == user.UserName);

				if (userFromDb == null) throw new InvalidDataException("User not found");

				if(userFromDb.OrganisationUniqueName != null)
				{
					throw new InvalidOperationException("Already assigned to organisation");
				}

				userFromDb.OrganisationUniqueName = user.OrganisationUniqueName;

				_db.Update(userFromDb);
				await _db.SaveChangesAsync();

				return true;
			}
			catch
			{
				return false;
			}
		}
	}
}
