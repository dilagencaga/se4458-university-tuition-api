using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Threading.RateLimiting;
using UniversityTuitionApi.Data;
using UniversityTuitionApi.config;
using UniversityTuitionApi.Services;


var builder = WebApplication.CreateBuilder(args);

// DbContext (PostgreSQL)
builder.Services.AddDbContext<UniversityContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// JwtSettings'i config'ten oku
var jwtSection = builder.Configuration.GetSection("JwtSettings");
builder.Services.Configure<JwtSettings>(jwtSection);
var jwtSettings = jwtSection.Get<JwtSettings>();

// Authentication (JWT Bearer)
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
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
    };
});

// Authorization
builder.Services.AddAuthorization();

// Business services
builder.Services.AddScoped<ITuitionService, TuitionService>();

// HTTP logging
builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields =
        HttpLoggingFields.RequestMethod |
        HttpLoggingFields.RequestPath |
        HttpLoggingFields.RequestHeaders |
        HttpLoggingFields.ResponseStatusCode |
        HttpLoggingFields.Duration;
});

// Global rate limiting
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        var key = httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: key,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,                    // 1 dakikada 100 istek
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            });
    });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});



// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "UniversityTuitionApi",
        Version = "v1"
    });

    // 🔐 HTTP Bearer (JWT) şeması
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "JWT token'ı şu formatta girin: Bearer {token}",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };

    // Şemayı tanımla
    c.AddSecurityDefinition("Bearer", securityScheme);

    // Tüm [Authorize] endpoint'lerine uygula
    var securityRequirement = new OpenApiSecurityRequirement
    {
        { securityScheme, new string[] { } }
    };
    c.AddSecurityRequirement(securityRequirement);
});

var app = builder.Build();

// Uygulama açılırken veritabanını otomatik oluştur
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<UniversityContext>();
    db.Database.EnsureCreated();
}

// Swagger UI

    app.UseSwagger();
    app.UseSwaggerUI();


//app.UseHttpsRedirection();

// HTTP logging ve rate limiting
app.UseHttpLogging();
app.UseRateLimiter();

app.UseAuthentication();   // 🔐 önce authentication
app.UseAuthorization();    // sonra authorization

app.MapControllers();

app.Run();

