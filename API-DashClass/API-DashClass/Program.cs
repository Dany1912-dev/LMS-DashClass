using API_DashClass.Data;
using API_DashClass.Services.Implementaciones;
using API_DashClass.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

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

//Registrar servicios de gamificación
builder.Services.AddScoped<IGamificationService, GamificationService>();

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

// Autenticación y Autorización (configurar después)
// app.UseAuthentication();
// app.UseAuthorization();

// Mapear controllers
app.MapControllers();

// ========================================
// EJECUTAR LA APLICACIÓN
// ========================================

app.Run();