using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_DashClass.Models.Entities
{
    [Table("cursos")]
    public class Cursos
    {
        [Key]
        [Column("id_curso")]
        public int IdCurso { get; set; }

        [Column("codigo")]
        [Required]
        [MaxLength(50)]
        public string Codigo { get; set; }

        [Column("nombre")]
        [Required]
        [MaxLength(255)]
        public string Nombre { get; set; }

        [Column("descripcion")]
        public string? Descripcion { get; set; }

        [Column("imagen_banner")]
        [MaxLength(500)]
        public string? ImagenBanner { get; set; }

        [Column("id_usuario")]
        [Required]
        public int IdUsuario { get; set; }

        [Column("fecha_creacion")]
        public DateTime FechaCreacion { get; set; }

        [Column("estatus")]
        public bool Estatus { get; set; }

        // ========================================
        // NAVIGATION PROPERTIES
        // ========================================

        // Profesor que creó el curso (FK)
        [ForeignKey("IdUsuario")]
        public Usuario? CreadoPor { get; set; }

        // Grupos dentro de este curso
        public ICollection<Grupos>? Grupos { get; set; }

        // Miembros inscritos en este curso
        public ICollection<MiembrosCursos>? Miembros { get; set; }

        // Invitaciones para este curso
        public ICollection<InvitacionesCurso>? Invitaciones { get; set; }

        // Actividades de este curso
        public ICollection<Actividades>? Actividades { get; set; }

        // Anuncios de este curso
        public ICollection<Anuncios>? Anuncios { get; set; }

        // Sesiones de asistencia de este curso
        public ICollection<SesionesAsistencia>? SesionesAsistencia { get; set; }

        // Transacciones de puntos en este curso
        public ICollection<TransaccionesPuntos>? TransaccionesPuntos { get; set; }

        // Recompensas disponibles en este curso
        public ICollection<Recompensas>? Recompensas { get; set; }

        // Evaluaciones de este curso
        public ICollection<Evaluaciones>? Evaluaciones { get; set; }

        // Logros disponibles en este curso
        public ICollection<Logros>? Logros { get; set; }

        // Transferencias de puntos en este curso
        public ICollection<TransferenciasPuntos>? TransferenciasPuntos { get; set; }
    }
}