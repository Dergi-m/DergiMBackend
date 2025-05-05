using AutoMapper;
using Azure.Identity;
using DergiMBackend;
using DergiMBackend.DbContext;
using DergiMBackend.Models;
using DergiMBackend.Services;
using DergiMBackend.Services.IServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var modelBuilder = WebApplication.CreateBuilder(args);

// --- Validate Secrets ---
SecretsValidator.Validate(modelBuilder.Configuration, modelBuilder.Environment);

// --- Database Configuration ---
modelBuilder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var env = modelBuilder.Environment;
    if (env.IsDevelopment())
    {
        // Local SQL Server (username/password)
        options.UseSqlServer(modelBuilder.Configuration["ConnectionStrings:DefaultSQLConnection"]);
    }
    else
    {
        // Azure Production (Managed Identity)
        var connection = new SqlConnection(modelBuilder.Configuration["AzureSql:ConnectionString"]);
        var credential = new DefaultAzureCredential();
        var token = credential.GetToken(new Azure.Core.TokenRequestContext(new[] { "https://database.windows.net/.default" }));
        connection.AccessToken = token.Token;
        options.UseSqlServer(connection);
    }
});

// --- Identity Configuration ---
modelBuilder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// --- Services Registration ---
modelBuilder.Services.AddScoped<IUserService, UserService>();
modelBuilder.Services.AddScoped<IAuthService, AuthService>();
modelBuilder.Services.AddScoped<ISessionService, SessionService>();
modelBuilder.Services.AddScoped<IOrganisationMembershipService, OrganisationMembershipService>();
modelBuilder.Services.AddScoped<IOrganisationRoleService, OrganisationRoleService>();
modelBuilder.Services.AddScoped<IOrganisationService, OrganisationService>();
modelBuilder.Services.AddScoped<IProjectService, ProjectService>();

modelBuilder.Services.AddHttpContextAccessor();

// --- Configuration Bindings ---
modelBuilder.Services.Configure<Dictionary<string, ClientConfig>>(modelBuilder.Configuration.GetSection("Clients"));
modelBuilder.Services.Configure<ApiSettings>(modelBuilder.Configuration.GetSection("ApiSettings"));

// --- Controllers ---
modelBuilder.Services.AddControllers(options =>
{
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
}).AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
    options.JsonSerializerOptions.WriteIndented = true;
});

// --- Authentication ---
modelBuilder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = modelBuilder.Configuration["ApiSettings:Issuer"],
        ValidAudience = modelBuilder.Configuration["ApiSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(modelBuilder.Configuration["ApiSettings:SecretKey"]!)
        )
    };
});

modelBuilder.Services.AddAuthorization();

// --- Swagger ---
modelBuilder.Services.AddEndpointsApiExplorer();
modelBuilder.Services.AddSwaggerGen(options =>
{
    // JWT Bearer token
    options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter 'Bearer {your JWT token}'",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = JwtBearerDefaults.AuthenticationScheme
    });

    // SessionToken Header
    options.AddSecurityDefinition("SessionToken", new OpenApiSecurityScheme
    {
        Name = "SessionToken",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Description = "Custom session token returned after login"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = JwtBearerDefaults.AuthenticationScheme
                }
            },
            new string[] {}
        },
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "SessionToken"
                }
            },
            new string[] {}
        }
    });
});


// --- AutoMapper ---
IMapper mapper = MapperConfig.RegisterMaps().CreateMapper();
modelBuilder.Services.AddSingleton(mapper);
modelBuilder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// --- Build Application ---
var app = modelBuilder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

ApplyMigrations();
await SeedRolesAsync(app.Services);
app.Run();

// --- Helper: Apply Pending Migrations ---
void ApplyMigrations()
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (db.Database.GetPendingMigrations().Any())
    {
        db.Database.Migrate();
    }
}

static async Task SeedRolesAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    string[] roles = { "ADMIN", "USER" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}


