using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Text;
using Volts.Application.Interfaces;
using Volts.Application.Services;
using Volts.Domain.Interfaces;
using Volts.Infrastructure.Data;
using Volts.Infrastructure.UnitOfWork;
using Volts.Api.Middleware;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// CORS - allow dev frontend
var devCorsPolicy = "DevCors";
builder.Services.AddCors(options =>
{
 options.AddPolicy(devCorsPolicy, policy =>
 {
 policy.WithOrigins("http://localhost:5173")
 .AllowAnyHeader()
 .AllowAnyMethod()
 .AllowCredentials();
 });
});

// Configurar DbContext com SQLite
builder.Services.AddDbContext<VoltsDbContext>(options =>
 options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>(); // todos os repositories estão aqui
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IOrganizationService, OrganizationService>();
builder.Services.AddScoped<IGroupService, GroupService>();
builder.Services.AddScoped<IPositionService, PositionService>();
builder.Services.AddScoped<IShiftService, ShiftService>();
builder.Services.AddScoped<IShiftPositionAssignmentService, ShiftPositionAssignmentService>();

var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["SecretKey"] ??
 throw new InvalidOperationException("JWT SecretKey não configurada");

builder.Services.AddAuthentication(options =>
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
 ValidIssuer = jwtSettings["Issuer"],
 ValidAudience = jwtSettings["Audience"],
 IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
 ClockSkew = TimeSpan.Zero
 };
});



// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
 options.SwaggerDoc("v1", new OpenApiInfo
 {
 Title = "JWT Auth API",
 Version = "v1",
 Description = "API de autenticação com JWT, Repository Pattern e Unit of Work"
 });

 // Configurar autenticação JWT no Swagger
 options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
 {
 Name = "Authorization",
 Type = SecuritySchemeType.Http,
 Scheme = "bearer",
 BearerFormat = "JWT",
 In = ParameterLocation.Header,
 Description = "Insira o token JWT no formato: Bearer {seu token}"
 });

 options.AddSecurityRequirement(new OpenApiSecurityRequirement
 {
 {
 new OpenApiSecurityScheme
 {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
 },
            []
 }
 });
});

builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy("API is running"));

var app = builder.Build();

// Criar banco de dados automaticamente
using (var scope = app.Services.CreateScope())
{
 var db = scope.ServiceProvider.GetRequiredService<VoltsDbContext>();
 db.Database.EnsureCreated();
}



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
 app.UseSwagger();
 app.UseSwaggerUI();
}

// Register global exception middleware as first middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Enable CORS for development
app.UseCors(devCorsPolicy);

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
