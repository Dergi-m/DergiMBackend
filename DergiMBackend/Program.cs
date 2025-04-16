using AutoMapper;
using DergiMBackend;
using DergiMBackend.DbContext;
using DergiMBackend.Models;
using DergiMBackend.Services.IServices;
using DergiMBackend.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultSQLConnection"));
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllers();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddEndpointsApiExplorer();

var secret = builder.Configuration.GetValue<string>("ApiSettings:Secret");

var key = Encoding.ASCII.GetBytes(secret);
var issuer = builder.Configuration.GetValue<string>("ApiSettings:Issuer");
var audience = builder.Configuration.GetValue<string>("ApiSettings:Audience");
builder.Services.AddSwaggerGen(options =>
{
	options.AddSecurityDefinition(name: JwtBearerDefaults.AuthenticationScheme, securityScheme: new OpenApiSecurityScheme()
	{
		Name = "Authorization",
		Description = "Enter Authorization string as following: Bearer JwtToken",
		In = ParameterLocation.Header,
		Type = SecuritySchemeType.ApiKey,
		Scheme = "Bearer"
	});
	options.AddSecurityRequirement(new OpenApiSecurityRequirement()
	   {
		   {
			   new OpenApiSecurityScheme()
			   {
				   Reference = new OpenApiReference()
				   {
					   Type = ReferenceType.SecurityScheme,
					   Id = JwtBearerDefaults.AuthenticationScheme
				   }
			   }, new string[] {}
		   }
	   });
});

builder.Services.AddAuthentication(options =>
{
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
	options.Events = new JwtBearerEvents
	{
		OnMessageReceived = context =>
		{
			try
			{
				var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
				if (!string.IsNullOrEmpty(token))
				{
					var tokenHandler = new JwtSecurityTokenHandler();
					var configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();

					var jwtToken = tokenHandler.ReadJwtToken(token);

					var clientId = jwtToken.Claims.FirstOrDefault(c => c.Type == "clientId").Value;

					if (string.IsNullOrEmpty(clientId))
					{
						context.Fail("clientId is missing in the token.");
						Console.WriteLine("clientId is missing in the token.");
					}

					ClientConfig client = new()
					{
						ClientId = clientId,
						ClientSecret = configuration["CLIENT_SECRET"]
					};

					if (string.IsNullOrEmpty(client.ClientSecret))
					{
						context.Fail("Invalid clientId.");
						Console.WriteLine("Invalid clientId.");
					}

					options.TokenValidationParameters = new TokenValidationParameters
					{
						ValidateIssuer = true,
						ValidateAudience = true,
						ValidateLifetime = true,
						ValidateIssuerSigningKey = true,
						ValidIssuer = configuration["ApiSettings:Issuer"],
						ValidAudience = configuration["ApiSettings:Audience"],
						IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(client.ClientSecret))
					};
				}
				else
					context.Fail($"Token validation failed: empty token or wrong token format");
				return Task.CompletedTask;
			}
			catch(Exception ex)
			{
				context.Fail($"Token validation failed: {ex.Message}");
				return Task.CompletedTask;
			}
		},
		OnAuthenticationFailed = context =>
		{
			Console.WriteLine($"Authentication failed: {context.Exception.Message}");
			return Task.CompletedTask;
		}
	};
});

builder.Services.AddAuthorization();

IMapper mapper = MapperConfig.RegisterMaps().CreateMapper();
builder.Services.AddSingleton(mapper);
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
ApplyMigrations();
app.Run();

void ApplyMigrations()
{
	using (var scope = app.Services.CreateScope())
	{
		var _db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

		if (_db != null && _db.Database.GetPendingMigrations().Count() > 0)
		{
			_db.Database.Migrate();
		}
	}
}
