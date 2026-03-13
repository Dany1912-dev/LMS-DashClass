using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_DashClass.Models.Entities
{
    [Table("usuarios")]
    public class Usuario
    {
        [Key]
        [Column("id_usuario")]
        public int IdUsuario { get; set; }

        [Column("email")]
        [Required]
        [MaxLength(100)]
        public string Email { get; set; }

        [Column("proveedor_auth_principal")]
        [Required]
        [MaxLength(50)]
        public string ProveedorAuthPrincipal { get; set; }

        [Column("nombre")]
        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; }

        [Column("apellidos")]
        [Required]
        [MaxLength(100)]
        public string Apellidos { get; set; }

        [Column("foto_perfil_url")]
        [MaxLength(100)]
        public string? FotoPerfilUrl { get; set; }

        [Column("biografia")]
        public string? Biografia { get; set; }

        [Column("fecha_creacion")]
        public DateTime FechaCreacion { get; set; }

        [Column("ultimo_acceso")]
        public DateTime? UltimoAcceso { get; set; }

        [Column("estatus")]
        public bool Estatus { get; set; }


        // ========================================
        // NAVIGATION PROPERTIES
        // ========================================

        // Cursos creados por este usuario (como profesor)
        public ICollection<Cursos>? CursosCreados { get; set; }

        // Métodos de autenticación del usuario
        public ICollection<MetodosAuthUsers>? MetodosAuth { get; set; }

        // Inscripciones a cursos (como estudiante o profesor)
        public ICollection<MiembrosCursos>? MiembrosCurso { get; set; }

        // Actividades creadas por este usuario (como profesor)
        public ICollection<Actividades>? ActividadesCreadas { get; set; }

        // Anuncios publicados por este usuario (como profesor)
        public ICollection<Anuncios>? AnunciosPublicados { get; set; }

        // Sesiones de asistencia creadas por este usuario (como profesor)
        public ICollection<SesionesAsistencia>? SesionesAsistenciaCreadas { get; set; }

        // Entregas realizadas por este usuario (como estudiante)
        public ICollection<Entregas>? Entregas { get; set; }

        // Calificaciones dadas por este usuario (como profesor)
        public ICollection<Calificaciones>? CalificacionesDadas { get; set; }

        // Registros de asistencia de este usuario (como estudiante)
        public ICollection<RegistrosAsistencia>? RegistrosAsistencia { get; set; }

        // Transacciones de puntos de este usuario
        public ICollection<TransaccionesPuntos>? TransaccionesPuntos { get; set; }

        // Recompensas creadas por este usuario (como profesor)
        public ICollection<Recompensas>? RecompensasCreadas { get; set; }

        // Canjes realizados por este usuario (como estudiante)
        public ICollection<Canjes>? Canjes { get; set; }

        // Evaluaciones creadas por este usuario (como profesor)
        public ICollection<Evaluaciones>? EvaluacionesCreadas { get; set; }

        // Respuestas dadas en evaluaciones (como estudiante)
        public ICollection<RespuestasEstudiantes>? RespuestasEvaluaciones { get; set; }

        // Logros creados por este usuario (como profesor)
        public ICollection<Logros>? LogrosCreados { get; set; }

        // Logros desbloqueados por este usuario (como estudiante)
        public ICollection<LogrosUsuario>? LogrosDesbloqueados { get; set; }

        // Transferencias de puntos enviadas por este usuario
        [InverseProperty("UsuarioEmisor")]
        public ICollection<TransferenciasPuntos>? TransferenciasEnviadas { get; set; }

        // Transferencias de puntos recibidas por este usuario
        [InverseProperty("UsuarioReceptor")]
        public ICollection<TransferenciasPuntos>? TransferenciasRecibidas { get; set; }

        // Estilo de aprendizaje del usuario (1:1)
        public EstilosAprendizaje? EstiloAprendizaje { get; set; }
    }
}