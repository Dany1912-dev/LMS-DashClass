Extensions/
Qué es: Métodos que extienden funcionalidad de clases existentes.
Para qué:

Configurar todos los servicios en un solo método
Program.cs más limpio

Ejemplo:

// Sin Extensions (Program.cs largo)
builder.Services.AddDbContext...
builder.Services.AddScoped...
builder.Services.AddScoped...
// 50 líneas más

// Con Extensions (Program.cs limpio)
builder.Services.AddApplicationServices();