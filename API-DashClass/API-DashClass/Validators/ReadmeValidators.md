Qué es: Reglas de validación complejas separadas.
Para qué:

Validar que CreateCourseRequest tenga código único
Validar que estudiante tenga puntos antes de canjear
Validar que actividad no tenga fecha pasada

Ejemplo: En vez de 50 líneas de validación en el controller, pones FluentValidation en un archivo aparte.