using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ParamonovAPI;
using ParamonovAPI.Models;
using ParamonovAPI.Services;
using System.Text;
using TaskModel = ParamonovAPI.Models.Task;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddDbContext<PostgresContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

//Generic Service

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


var app = builder.Build();

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("CorsPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/", () => "Hello World!");


app.Run();

public class AuthOptions
{
    public const string ISSUER = "MyAuthServer";
    public const string AUDIENCE = "MyAuthClient";
    const string KEY = "mysupersecret_secretsecretsecretkey!123";
    public static SymmetricSecurityKey GetSymmetricSecurityKey() =>
        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(KEY));
}