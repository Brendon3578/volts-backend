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
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using System.Text.RegularExpressions;
using Volts.Infrastructure.Seed;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Global Rate Limiting: 100 req/min por IP
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ip,
            factory: partition => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            });
    });

    // Política padrão (não aplicada por endpoint, apenas para referência)
    options.AddPolicy("default", httpContext =>
    {
        var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ip,
            factory: partition => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            });
    });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.ContentType = "application/json";
        await context.HttpContext.Response.WriteAsync(
            "{\"message\":\"Too many requests. Please try again later.\"}", token);
    };
});

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

// Configuração do DbContext com PostgreSQL (Supabase)
// Anterior (SQLite) mantido como referência:
// builder.Services.AddDbContext<VoltsDbContext>(options =>
//     options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

string BuildPostgresConnectionString()
{
    // Tenta usar DATABASE_URL (opcional) no formato: postgres://user:pass@host:port/dbname
    var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
    if (!string.IsNullOrWhiteSpace(databaseUrl))
    {
        // Normaliza para URI
        var uri = new Uri(databaseUrl);
        var userInfo = uri.UserInfo.Split(':');
        var username = Uri.UnescapeDataString(userInfo[0]);
        var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty;
        var host = uri.Host;
        var port = uri.Port > 0 ? uri.Port : 5432;
        var database = uri.AbsolutePath.TrimStart('/');

        // SSL é geralmente exigido no Supabase
        return $"Host={host};Port={port};Database={database};Username={username};Password={password};Ssl Mode=Require;Trust Server Certificate=true";
    }

    // Caso contrário, monta a partir das variáveis SUPABASE_*
    var hostEnv = Environment.GetEnvironmentVariable("SUPABASE_DB_HOST");
    var portEnv = Environment.GetEnvironmentVariable("SUPABASE_DB_PORT") ?? "5432";
    var userEnv = Environment.GetEnvironmentVariable("SUPABASE_DB_USER");
    var passEnv = Environment.GetEnvironmentVariable("SUPABASE_DB_PASSWORD");
    var nameEnv = Environment.GetEnvironmentVariable("SUPABASE_DB_NAME");

    // Se não houver variáveis, tenta appsettings: ConnectionStrings:PostgresConnection
    var appsettingsConn = builder.Configuration.GetConnectionString("PostgresConnection");
    if (!string.IsNullOrWhiteSpace(appsettingsConn))
        return appsettingsConn;

    if (string.IsNullOrWhiteSpace(hostEnv) || string.IsNullOrWhiteSpace(userEnv) || string.IsNullOrWhiteSpace(passEnv) || string.IsNullOrWhiteSpace(nameEnv))
        throw new InvalidOperationException("PostgreSQL connection is not configured. Set SUPABASE_DB_* env vars or DATABASE_URL, or provide ConnectionStrings:PostgresConnection.");

    return $"Host={hostEnv};Port={portEnv};Database={nameEnv};Username={userEnv};Password={passEnv};Ssl Mode=Require;Trust Server Certificate=true";
}

var postgresConnectionString = BuildPostgresConnectionString();

builder.Services.AddDbContext<VoltsDbContext>(options =>
    options.UseNpgsql(postgresConnectionString));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>(); // todos os repositories est�o aqui
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IOrganizationService, OrganizationService>();
builder.Services.AddScoped<IGroupService, GroupService>();
builder.Services.AddScoped<IPositionService, PositionService>();
builder.Services.AddScoped<IShiftService, ShiftService>();
builder.Services.AddScoped<IShiftPositionAssignmentService, ShiftPositionAssignmentService>();
builder.Services.AddScoped<DatabaseSeeder>();

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

    // Configurar autentica��o JWT no Swagger
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
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
        },
        []
    }
    });
});

builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy("API is running"));

var app = builder.Build();

// Aplicar migrações automaticamente (requer migrations criadas para PostgreSQL)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<VoltsDbContext>();
    db.Database.Migrate();


    if (app.Environment.IsDevelopment())
    {
    // Executa o seed automaticamente na inicialização
        var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
        seeder.SeedAsync().GetAwaiter().GetResult();
    }
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

// Aplicar rate limiting global
app.UseRateLimiter();

app.MapControllers();

app.Run();
