using System.Security.Claims;
using System.Text;
using API;
using Infra;
using LinqToDB;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Service;
using Service.Security;

var builder = WebApplication.CreateBuilder(args);

// Db configuration
var options = new DataOptions<MyDatabaseConnection>(
    new DataOptions().UseSQLite("Data Source=../Infra/db.db"));

// JWT config
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is missing in appsettings.json");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer is missing in appsettings.json");
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? throw new InvalidOperationException("Jwt:Audience is missing in appsettings.json");
var jwtExpiresMinutes = builder.Configuration.GetValue<int>("Jwt:ExpiresMinutes", 60);


builder.Services.AddScoped<MyDatabaseConnection>(_ => new MyDatabaseConnection(options));
builder.Services.AddScoped<IPasswordHasher, Argon2PasswordHasher>();
builder.Services.AddSingleton<ITokenService>(new JwtTokenService(jwtKey, jwtIssuer, jwtAudience, jwtExpiresMinutes));
builder.Services.AddScoped<UserService>();

// JWT authentication
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(jwtOptions =>
    {
        jwtOptions.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,

            ValidateAudience = true,
            ValidAudience = jwtAudience,

            ValidateLifetime = true,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),

            NameClaimType = ClaimTypes.Name,
            RoleClaimType = ClaimTypes.Role
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();

builder.Services.AddOpenApiDocument(document =>
{
    document.DocumentProcessors.Add(
        new NSwag.Generation.Processors.Security.SecurityDefinitionAppender(
            "Bearer",
            new NSwag.OpenApiSecurityScheme
            {
                Type = NSwag.OpenApiSecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Enter your JWT bearer token"
            }));
    
    document.OperationProcessors.Add(
        new NSwag.Generation.Processors.Security
            .AspNetCoreOperationSecurityScopeProcessor("Bearer"));
});

builder.Services.AddCors();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<MyExceptionHandler>();

var app = builder.Build();

// db initialization, no seeder cause it's only one Admin initially
    using (var scope = app.Services.CreateScope())
    {
        try
        {
            // Ensure directory exists so SQLite doesn't fail
            Directory.CreateDirectory("../Infra");

            var db = scope.ServiceProvider.GetRequiredService<MyDatabaseConnection>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

            db.CreateTable<User>(tableOptions: TableOptions.CreateIfNotExists);

            if (!db.Users.Any())
            {
                var admin = new User
                {
                    UserId = Guid.NewGuid().ToString(),
                    FirstName = "Admin",
                    LastName = "System",
                    Login = "admin",
                    PasswordHash = hasher.HashAndSaltPassword("Admin123!"),
                    Role = "Admin"
                };
                db.Insert(admin);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("================ STARTUP ERROR ================");
            Console.WriteLine(ex.InnerException?.ToString() ?? ex.ToString());
            Console.WriteLine("===============================================");
            throw;
        }
    
}

app.UseExceptionHandler();
app.UseCors(config => config.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());

app.UseAuthentication();
app.UseAuthorization();

app.UseOpenApi();
app.UseSwaggerUi();

app.MapControllers();

app.Run();