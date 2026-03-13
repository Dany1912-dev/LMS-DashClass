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
        public bool Estatus { get; set; } = true;

        //// Cursos creados por este usuario
        //public ICollection<Curso>? CursosCreados { get; set; }

        //// Métodos de autenticación del usuario
        //public ICollection<MetodoAuthUsuario>? MetodosAuth { get; set; }

        //// Inscripciones a cursos
        //public ICollection<MiembroCurso>? MiembrosCurso { get; set; }

        //// Transacciones de puntos
        //public ICollection<TransaccionPuntos>? TransaccionesPuntos { get; set; }

        //// Canjes realizados
        //public ICollection<Canje>? Canjes { get; set; }

        //// Transferencias enviadas
        //public ICollection<TransferenciaPuntos>? TransferenciasEnviadas { get; set; }

        //// Transferencias recibidas
        //public ICollection<TransferenciaPuntos>? TransferenciasRecibidas { get; set; }

        //// Logros desbloqueados
        //public ICollection<LogroUsuario>? LogrosUsuario { get; set; }

        //// Estilo de aprendizaje
        //public EstiloAprendizaje? EstiloAprendizaje { get; set; }

        //// Entregas realizadas
        //public ICollection<Entrega>? Entregas { get; set; }

        //// Registros de asistencia
        //public ICollection<RegistroAsistencia>? RegistrosAsistencia { get; set; }

        //// Calificaciones dadas (como profesor)
        //public ICollection<Calificacion>? CalificacionesDadas { get; set; }

        //// Respuestas en evaluaciones
        //public ICollection<RespuestaEstudiante>? RespuestasEvaluaciones { get; set; }
    }
}
