using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_DashClass.Models.Entities
{
    [Table("respuestas_estudiantes")]
    public class RespuestasEstudiantes
    {
        [Key]
        [Column("id_respuesta")]
        public int IdRespuesta { get; set; }

        [Column("id_sesion_evaluacion")]
        [Required]
        public int IdSesionEvaluacion { get; set; }

        [Column("id_usuario")]
        [Required]
        public int IdUsuario { get; set; }

        [Column("id_pregunta")]
        [Required]
        public int IdPregunta { get; set; }

        [Column("respuesta_dada")]
        public string? RespuestaDada { get; set; }

        [Column("es_correcta")]
        public bool? EsCorrecta { get; set; }

        [Column("tiempo_tomado")]
        public int? TiempoTomado { get; set; }

        [Column("puntuacion_obtenida")]
        public int? PuntuacionObtenida { get; set; }

        [Column("fecha_respuesta")]
        public DateTime FechaRespuesta { get; set; }

        // ========================================
        // NAVIGATION PROPERTIES
        // ========================================

        // Sesión de evaluación en la que se dio esta respuesta (FK)
        [ForeignKey("IdSesionEvaluacion")]
        public SesionesEvaluacion? SesionEvaluacion { get; set; }

        // Usuario (estudiante) que dio la respuesta (FK)
        [ForeignKey("IdUsuario")]
        public Usuario? Estudiante { get; set; }

        // Pregunta que se respondió (FK)
        [ForeignKey("IdPregunta")]
        public PreguntasEvaluacion? Pregunta { get; set; }
    }
}