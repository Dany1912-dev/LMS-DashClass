Qué es: Código que se ejecuta ANTES de llegar a tus controllers.
Para qué:

Rate limiting: Bloquear usuarios que hagan 100 requests por minuto
Manejo de errores: Si hay excepción, devolver JSON bonito en vez de error feo
Logging: Guardar log de cada petición

Ejemplo: Usuario hace 1000 requests en 1 segundo → Middleware lo bloquea antes de llegar al controller.