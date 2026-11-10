using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using ParamonovAPI;
using ParamonovAPI.Models;
using ParamonovAPI.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TaskModel = ParamonovAPI.Models.Task;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(
        new System.Text.Json.Serialization.JsonStringEnumConverter());
}); ;
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<PostgresContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
        o => o.MapEnum<ProjectStatus>("project_status")));

//var dataSourceBuilder = new NpgsqlDataSourceBuilder("DefaultConnection");
//dataSourceBuilder.MapEnum<ProjectStatus>("project_status");
//var dataSource = dataSourceBuilder.Build();

// ===== Регистрация сервисов =====
builder.Services.AddScoped<IService<Company>, CompanyService>();
builder.Services.AddScoped<IService<Team>, TeamService>();
builder.Services.AddScoped<IService<User>, UserService>();
builder.Services.AddScoped<IService<Project>, ProjectService>();
builder.Services.AddScoped<IService<TechnicalSpecification>, TechnicalSpecificationService>();
builder.Services.AddScoped<IService<SpecificationItem>, SpecificationItemService>();
builder.Services.AddScoped<IService<Invoice>, InvoiceService>();
builder.Services.AddScoped<IService<TaskList>, TaskListService>();
builder.Services.AddScoped<IService<TaskModel>, TaskService>();
builder.Services.AddScoped<IService<ChangeRequest>, ChangeRequestService>();

// ===== JWT =====
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = AuthOptions.ISSUER,
            ValidateAudience = true,
            ValidAudience = AuthOptions.AUDIENCE,
            ValidateLifetime = true,
            IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
            ValidateIssuerSigningKey = true,
        };
    });

// ===== CORS =====
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("CorsPolicy");
app.UseAuthentication();
app.UseAuthorization();

// ===== /login =====
app.MapPost("/login", async (LoginDto dto, PostgresContext db) =>
{
    var user = await db.Users.FirstOrDefaultAsync(u => u.Login == dto.Login);
    if (user == null) return Results.Unauthorized();

    var hash = AuthOptions.GenerateSha256Hash(dto.Password);
    if (user.PasswordHash != hash) return Results.Unauthorized();

    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.Login),
        new Claim(ClaimTypes.NameIdentifier, user.IdUser.ToString()),
        new Claim(ClaimTypes.Role, user.Role.ToString())
    };

    var jwt = new JwtSecurityToken(
        issuer: AuthOptions.ISSUER,
        audience: AuthOptions.AUDIENCE,
        claims: claims,
        expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(60)),
        signingCredentials: new SigningCredentials(
            AuthOptions.GetSymmetricSecurityKey(),
            SecurityAlgorithms.HmacSha256));

    var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

    return Results.Json(new
    {
        access_token = encodedJwt,
        login = user.Login,
        role = user.Role.ToString(),
        id_user = user.IdUser,
        id_team = user.IdTeam
    });
});

// ===== /register =====
app.MapPost("/register", async (RegisterDto dto, PostgresContext db) =>
{
    if (await db.Users.AnyAsync(u => u.Login == dto.Login))
        return Results.BadRequest(new { error = "Логин уже занят" });

    var user = new User
    {
        FirstName = dto.FirstName,
        LastName = dto.LastName,
        Login = dto.Login,
        PasswordHash = AuthOptions.GenerateSha256Hash(dto.Password),
        Role = dto.Role,
        IdTeam = dto.IdTeam,
        IdCompany = dto.IdCompany,
        ShortDescription = dto.ShortDescription
    };

    db.Users.Add(user);
    await db.SaveChangesAsync();

    return Results.Ok(new
    {
        id_user = user.IdUser,
        login = user.Login,
        role = user.Role.ToString()
    });
});

app.MapControllers();

app.Run();

// =====================================================
// DTO для логина и регистрации
// =====================================================
public class LoginDto
{
    public string Login { get; set; } = null!;
    public string Password { get; set; } = null!;
}

public class RegisterDto
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Login { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string Role { get; set; }
    public int? IdTeam { get; set; }
    public int? IdCompany { get; set; }
    public string? ShortDescription { get; set; }
}

// =====================================================
// AuthOptions
// =====================================================
public class AuthOptions
{
    public const string ISSUER = "MyAuthServer";
    public const string AUDIENCE = "MyAuthClient";
    const string KEY = "mysupersecret_secretsecretsecretkey!123";

    public static SymmetricSecurityKey GetSymmetricSecurityKey() =>
        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(KEY));

    public static string GenerateSha256Hash(string password)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
}