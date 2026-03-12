Qué es: Clases para leer appsettings.json de forma tipada.
Para qué:

En vez de leer strings de appsettings directamente
Tienes clases C# con las opciones

Ejemplo:

// Sin Configuration (malo)
var secret = Configuration["JwtSettings:Secret"];

// Con Configuration (bueno)
var secret = _jwtSettings.Secret; // tipado