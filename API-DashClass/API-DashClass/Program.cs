using API_DashClass.Data;
using API_DashClass.Services.Implementaciones;
using API_DashClass.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Resend;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ========================================
// CONFIGURAR SERVICIOS
// ========================================

// Configurar DbContext con MySQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)
    )
);

// Registrar servicios de gamificación
builder.Services.AddScoped<IGamificationService, GamificationService>();
builder.Services.AddScoped<IRecompensaService, RecompensaService>();
builder.Services.AddScoped<ICanjeService, CanjeService>();
builder.Services.AddScoped<ILogroService, LogroService>();

// Registrar servicio de autenticación
builder.Services.AddScoped<IAuthService, AuthService>();

// Registrar servicio de email (Resend)
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddHttpClient<IResend, ResendClient>();
builder.Services.Configure<ResendClientOptions>(options =>
{
    options.ApiToken = builder.Configuration["ResendSettings:ApiKey"]
        ?? throw new InvalidOperationException("Resend API Key no configurada en appsettings");
});

// .- .. --.. ---
builder.Services.AddScoped<ICursoService, CursoService>();

// Registrar IMemoryCache para códigos de verificación
builder.Services.AddMemoryCache();

// Configurar JWT
var jwtKey = builder.Configuration["JwtSettings:Secret"]
    ?? throw new InvalidOperationException("JWT Secret no configurado en appsettings");
var jwtIssuer = builder.Configuration["JwtSettings:Issuer"]
    ?? throw new InvalidOperationException("JWT Issuer no configurado en appsettings");
var jwtAudience = builder.Configuration["JwtSettings:Audience"]
    ?? throw new InvalidOperationException("JWT Audience no configurado en appsettings");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

// Configurar Controllers
builder.Services.AddControllers();

// Configurar CORS
var corsOrigins = builder.Configuration.GetSection("CorsSettings:AllowedOrigins").Get<string[]>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(corsOrigins ?? new[] { "http://localhost:3000", "http://localhost:5173" })
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Configurar Swagger/OpenAPI para desarrollo
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ========================================
// CONSTRUIR LA APLICACIÓN
// ========================================

var app = builder.Build();

// ========================================
// CONFIGURAR MIDDLEWARE PIPELINE
// ========================================

// Swagger solo en desarrollo
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// CORS (debe ir antes de Authorization)
app.UseCors("AllowFrontend");

// HTTPS Redirection
app.UseHttpsRedirection();

// Servir archivos estáticos desde la carpeta uploads
app.UseStaticFiles();

// Autenticación y Autorización
app.UseAuthentication();
app.UseAuthorization();

// Mapear controllers
app.MapControllers();

// ========================================
// EJECUTAR LA APLICACIÓN
// ========================================

app.Run();