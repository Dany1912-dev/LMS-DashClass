using API_DashClass.Data;
using API_DashClass.Events;
using API_DashClass.Events.Handlers;
using API_DashClass.Services.Implementaciones;
using API_DashClass.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Resend;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ========================================
// REGISTRAR SISTEMA DE EVENTOS
// ========================================
builder.Services.AddSingleton<BusEventos>();

builder.Services.AddScoped<CalificacionCreadaManejador>();
builder.Services.AddScoped<AsistenciaRegistradaManejador>();

// ========================================
// CONFIGURAR SERVICIOS
// ========================================

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)
    )
);

// Actividades y categorías
builder.Services.AddScoped<IActividadService, ActividadService>();
builder.Services.AddScoped<ICategoriaService, CategoriaService>();

// Entregas y calificaciones
builder.Services.AddScoped<IEntregaService, EntregaService>();

// Gamificación
builder.Services.AddScoped<IGamificationService, GamificationService>();
builder.Services.AddScoped<IRecompensaService, RecompensaService>();
builder.Services.AddScoped<ICanjeService, CanjeService>();
builder.Services.AddScoped<ILogroService, LogroService>();

// Auth
builder.Services.AddScoped<IAuthService, AuthService>();

// Email
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddHttpClient<IResend, ResendClient>();
builder.Services.Configure<ResendClientOptions>(options =>
{
    options.ApiToken = builder.Configuration["ResendSettings:ApiKey"]
        ?? throw new InvalidOperationException("Resend API Key no configurada en appsettings");
});

builder.Services.AddScoped<ICursoService, CursoService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();

builder.Services.AddMemoryCache();

// ========================================
// CONFIGURAR LÍMITE DE TAMAÑO DE ARCHIVOS
// ========================================
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 50 * 1024 * 1024; // 50 MB
});

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

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:3000",
                "http://localhost:5173",
                "https://localhost:5173"
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ========================================
// CONSTRUIR LA APLICACIÓN
// ========================================

var app = builder.Build();

// ========================================
// CONFIGURAR MIDDLEWARE PIPELINE
// ========================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");
app.UseHttpsRedirection();

// ========================================
// SERVIR ARCHIVOS ESTÁTICOS (uploads)
// ========================================
var uploadsPath = Path.Combine(app.Environment.ContentRootPath, "uploads");
Directory.CreateDirectory(uploadsPath); // crear si no existe

//Suscribir eventos
var BusEventos = app.Services.GetRequiredService<BusEventos>();
BusEventos.Suscribir<CalificacionCreadaEvento, CalificacionCreadaManejador>();
BusEventos.Suscribir<AsistenciaRegistradaEvento, AsistenciaRegistradaManejador>();

// Proveedor de tipos de contenido — permite cualquier extensión
var contentTypeProvider = new FileExtensionContentTypeProvider();
contentTypeProvider.Mappings[".docx"] = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
contentTypeProvider.Mappings[".xlsx"] = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads",
    ContentTypeProvider = contentTypeProvider
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();