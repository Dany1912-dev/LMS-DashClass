using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_DashClass.Models.Entities
{
    public enum EstatusActividad
    {
        Borrador,
        Publicado,
        Programado,
        Archivado
    }

    [Table("actividades")]
    public class Actividades
    {
        [Key]
        [Column("id_actividad")]
        public int IdActividad { get; set; }

        [Column("id_curso")]
        [Required]
        public int IdCurso { get; set; }

        [Column("titulo")]
        [Required]
        [MaxLength(255)]
        public string Titulo { get; set; }

        [Column("descripcion")]
        public string? Descripcion { get; set; }

        [Column("puntos_maximos")]
        [Required]
        public int PuntosMaximos { get; set; }

        [Column("puntos_gamificacion_maximos")]
        public int PuntosGamificacionMaximos { get; set; }

        [Column("fecha_limite")]
        public DateTime? FechaLimite { get; set; }

        [Column("permite_entregas_tardias")]
        public bool PermiteEntregasTardias { get; set; }

        [Column("estatus")]
        public EstatusActividad Estatus { get; set; }

        [Column("fecha_publicacion")]
        public DateTime? FechaPublicacion { get; set; }

        [Column("fecha_programada")]
        public DateTime? FechaProgramada { get; set; }

        [Column("id_usuario")]
        [Required]
        public int IdUsuario { get; set; }

        [Column("fecha_creacion")]
        public DateTime FechaCreacion { get; set; }

        // ========================================
        // NAVIGATION PROPERTIES
        // ========================================

        // Curso al que pertenece esta actividad (FK)
        [ForeignKey("IdCurso")]
        public Cursos? Curso { get; set; }

        // Usuario (profesor) que creó esta actividad (FK)
        [ForeignKey("IdUsuario")]
        public Usuario? CreadoPor { get; set; }

        // Materiales adjuntos a esta actividad
        public ICollection<MaterialesActividad>? Materiales { get; set; }

        // Entregas de estudiantes para esta actividad
        public ICollection<Entregas>? Entregas { get; set; }

        // Grupos a los que está asignada esta actividad
        public ICollection<ActividadesGrupos>? ActividadesGrupos { get; set; }
    }
}