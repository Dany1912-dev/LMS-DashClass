Qué es: Sistema para que Equipo 1 y Equipo 2 se comuniquen sin acoplarse.
Para qué:

Equipo 1 califica → Dispara evento "GradeCreated"
Equipo 2 escucha evento → Da puntos al estudiante
Sin que Equipo 1 sepa de gamificación

Ejemplo: CalificacionService crea calificación → EventBus dispara evento → GamificationService escucha y da puntos.