using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ProblemReportingSystem.Application.ServiceAbstractions;
using ProblemReportingSystem.Application.Services;
using ProblemReportingSystem.API.Middleware;
using ProblemReportingSystem.DAL.Infrastructure;
using ProblemReportingSystem.DAL.Repositories;
using ProblemReportingSystem.DAL.RepositoryAbstractions;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
var builder = WebApplication.CreateBuilder(args);

var cultureInfo = new CultureInfo("en-US");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiI2OGI2MjlkNC04MzQwLTRjMzktOWE1ZS04Zjk2MGM0OWYzNzkiLCJlbWFpbCI6Im5hemFyQGdtYWlsLmNvbSIsInVuaXF1ZV9uYW1lIjoibmF6YXJjaHVrIiwibmJmIjoxNzc2MjYyOTc4LCJleHAiOjE3NzYyNjY1NzgsImlhdCI6MTc3NjI2Mjk3OCwiaXNzIjoiUHJvYmxlbVJlcG9ydGluZ1N5c3RlbSIsImF1ZCI6IlByb2JsZW1SZXBvcnRpbmdTeXN0ZW1Vc2VycyJ9.L6U8tKddptH3TtY2okcTxr7uFNqlenQIPzVxGhFrknY",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

// Add DbContext
builder.Services.AddDbContext<ProblemReportingSystemDbContext>();

// Add AutoMapper
builder.Services.AddAutoMapper(_ => { }, AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();

builder.Services.AddDbContext<ProblemReportingSystemDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secret = jwtSettings["Secret"];
var issuer = jwtSettings["Issuer"];
var audience = jwtSettings["Audience"];

if (string.IsNullOrEmpty(secret) || string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience))
{
    throw new InvalidOperationException("JWT settings are not properly configured in appsettings.json");
}

var key = Encoding.ASCII.GetBytes(secret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = issuer,
        ValidateAudience = true,
        ValidAudience = audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Register Services
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IGeolocateService, GeolocateService>();
builder.Services.AddScoped<IProblemService, ProblemService>();
builder.Services.AddScoped<IAppealService, AppealService>();
builder.Services.AddScoped<IPollService, PollService>();
builder.Services.AddScoped<ICouncilEmployeeService, CouncilEmployeeService>();
builder.Services.AddScoped<IStatisticsService, StatisticsService>();
builder.Services.AddScoped<ICityCouncilService, CityCouncilService>();
builder.Services.AddScoped<IAddressService, AddressService>();

// Register Repositories
builder.Services.AddScoped(typeof(IProblemReportingSystemRepository<>),
    typeof(ProblemReportingSystemRepository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProblemRepository, ProblemRepository>();
builder.Services.AddScoped<IAppealRepository, AppealRepository>();
builder.Services.AddScoped<IPollRepository, PollRepository>();
builder.Services.AddScoped<ICouncilEmployeeRepository, CouncilEmployeeRepository>();
builder.Services.AddScoped<ICityCouncilRepository, CityCouncilRepository>();
builder.Services.AddScoped<IAddressRepository, AddressRepository>();

// Register HttpClient for GeolocateService
builder.Services.AddHttpClient<IGeolocateService, GeolocateService>();

// Add CORS if needed
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", corsBuilder =>
    {
        corsBuilder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
    });

    options.AddPolicy("AllowLocalhost3000", corsBuilder =>
    {
        corsBuilder.WithOrigins("http://localhost:3000", "https://localhost:3000")
                   .AllowAnyMethod()
                   .AllowAnyHeader()
                   .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Use global exception handling middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

// Use CORS
app.UseCors("AllowLocalhost3000");

// Use Authentication and Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();