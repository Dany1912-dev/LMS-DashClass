using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_DashClass.Models.Entities
{
    [Table("entregas")]
    public class Entregas
    {
        public enum EstadoEntrega
        {
            Entregada,
            Calificada,
            Reemplazada
        }

        [Key]
        [Column("id_entrega")]
        public int IdEntrega { get; set; }

        [Column("id_actividad")]
        [Required]
        public int IdActividad { get; set; }

        [Column("id_usuario")]
        [Required]
        public int IdUsuario { get; set; }

        [Column("comentarios")]
        public string? Comentarios { get; set; }

        [Column("fecha_entrega")]
        public DateTime FechaEntrega { get; set; }

        [Column("es_tardia")]
        public bool EsTardia { get; set; }

        [Column("version")]
        public int Version { get; set; }

        [Column("estado")]
        public EstadoEntrega Estado { get; set; }

        // ========================================
        // NAVIGATION PROPERTIES
        // ========================================

        // Actividad a la que pertenece esta entrega (FK)
        [ForeignKey("IdActividad")]
        public Actividades? Actividad { get; set; }

        // Usuario (estudiante) que hizo esta entrega (FK)
        [ForeignKey("IdUsuario")]
        public Usuario? Estudiante { get; set; }

        // Recursos (archivos/enlaces) adjuntos a esta entrega
        public ICollection<RecursosEntrega>? Recursos { get; set; }

        // Calificación de esta entrega (1:1, opcional)
        public Calificaciones? Calificacion { get; set; }
    }
}