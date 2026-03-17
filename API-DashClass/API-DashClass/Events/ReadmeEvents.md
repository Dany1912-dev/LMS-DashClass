Qué es: Sistema para que Equipo 1 y Equipo 2 se comuniquen sin acoplarse.
Para qué:

Equipo 1 califica → Dispara evento "GradeCreated"
Equipo 2 escucha evento → Da puntos al estudiante
Sin que Equipo 1 sepa de gamificación

Ejemplo: CalificacionService crea calificación → EventBus dispara evento → GamificationService escucha y da puntos.

# INTEGRACIÓN - SISTEMA DE EVENTOS DE GAMIFICACIÓN

## Para el equipo de Core Académico

Este sistema permite que la gamificación funcione **automáticamente** cuando se califican entregas o se registra asistencia.

---

## 🎯 ¿Cómo funciona?

Cuando calificas una entrega o registras asistencia, solo necesitas **disparar un evento**. El sistema de gamificación escuchará automáticamente y otorgará los puntos.

---

## 📝 Ejemplo 1: Calificación de Entregas

### En tu CalificacionService o CalificacionController:
```csharp
using API_DashClass.Events;

public class CalificacionService
{
    private readonly AppDbContext _context;
    private readonly BusEventos _busEventos;
    
    public CalificacionService(AppDbContext context, BusEventos busEventos)
    {
        _context = context;
        _busEventos = busEventos;
    }
    
    public async Task CalificarEntrega(int idEntrega, decimal puntuacion)
    {
        // 1. Obtener la entrega
        var entrega = await _context.Entregas
            .Include(e => e.Actividad)
            .FirstOrDefaultAsync(e => e.IdEntrega == idEntrega);
        
        // 2. Crear y guardar la calificación
        var calificacion = new Calificaciones
        {
            IdEntrega = idEntrega,
            Puntuacion = puntuacion,
            IdUsuario = /* id del profesor */,
            FechaCalificacion = DateTime.UtcNow
        };
        
        _context.Calificaciones.Add(calificacion);
        await _context.SaveChangesAsync();
        
        // 3. 🔔 DISPARAR EVENTO - Gamificación se activa automáticamente
        await _busEventos.PublicarAsync(new CalificacionCreadaEvento
        {
            IdEntrega = entrega.IdEntrega,
            IdActividad = entrega.IdActividad,
            IdUsuario = entrega.IdUsuario,
            IdCurso = entrega.Actividad.IdCurso,
            Puntuacion = puntuacion,
            PuntosMaximos = 100, // O el valor que uses
            PuntosGamificacionMaximos = entrega.Actividad.PuntosGamificacionMaximos
        });
        
        // ✅ ¡Listo! El estudiante ya tiene sus puntos
    }
}
```

---

## 📝 Ejemplo 2: Registro de Asistencia

### En tu AsistenciaService o AsistenciaController:
```csharp
using API_DashClass.Events;

public class AsistenciaService
{
    private readonly AppDbContext _context;
    private readonly BusEventos _busEventos;
    
    public AsistenciaService(AppDbContext context, BusEventos busEventos)
    {
        _context = context;
        _busEventos = busEventos;
    }
    
    public async Task RegistrarAsistencia(int idSesion, int idUsuario)
    {
        // 1. Obtener la sesión de asistencia
        var sesion = await _context.SesionesAsistencia
            .FirstOrDefaultAsync(s => s.IdSesionAsistencia == idSesion);
        
        // 2. Crear y guardar el registro de asistencia
        var registro = new RegistrosAsistencia
        {
            IdSesionAsistencia = idSesion,
            IdUsuario = idUsuario,
            MetodoUsado = RegistrosAsistencia.MetodoAsistencia.QR,
            FechaRegistro = DateTime.UtcNow
        };
        
        _context.RegistrosAsistencia.Add(registro);
        await _context.SaveChangesAsync();
        
        // 3. 🔔 DISPARAR EVENTO - Gamificación se activa automáticamente
        await _busEventos.PublicarAsync(new AsistenciaRegistradaEvento
        {
            IdRegistroAsistencia = registro.IdRegistroAsistencia,
            IdSesionAsistencia = idSesion,
            IdUsuario = idUsuario,
            IdCurso = sesion.IdCurso
        });
        
        // ✅ ¡Listo! El estudiante recibió 5 puntos automáticamente
    }
}
```

---

## 📦 Imports necesarios
```csharp
using API_DashClass.Events;
```

---

## ⚙️ Configuración en tu servicio

Para usar el BusEventos, solo necesitas inyectarlo en el constructor:
```csharp
private readonly BusEventos _busEventos;

public TuServicio(BusEventos busEventos, ...)
{
    _busEventos = busEventos;
}
```

---

## 🎮 ¿Qué hace el sistema de gamificación?

### Cuando se dispara CalificacionCreadaEvento:
- ✅ Calcula puntos: `(puntuacion / puntosMaximos) * puntosGamificacionMaximos`
- ✅ Crea una TransaccionPuntos automáticamente
- ✅ Actualiza el balance del estudiante
- ✅ Registra el origen como "Calificación"

### Cuando se dispara AsistenciaRegistradaEvento:
- ✅ Otorga 5 puntos automáticamente
- ✅ Crea una TransaccionPuntos
- ✅ Actualiza el balance del estudiante
- ✅ Registra el origen como "Asistencia"

---

## 🔍 Verificar que funcionó

Después de disparar un evento, puedes verificar en la tabla `transacciones_puntos`:
```sql
SELECT * FROM transacciones_puntos 
WHERE id_usuario = {idEstudiante} 
ORDER BY fecha_creacion DESC 
LIMIT 5;
```

---

## ❓ Dudas o problemas

Contacta al equipo de Gamificación.