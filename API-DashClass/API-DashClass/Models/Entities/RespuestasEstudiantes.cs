using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_DashClass.Models.Entities
{
    [Table("respuestas_estudiantes")]
    public class RespuestasEstudiantes
    {
        [Key]
        [Column ("id_respuesta")]
        public int IdRespuesta { get; set; }

        [Column("id_sesion_evaluacion")]
        [Required]
        public int IdSesionEvaluacion { get; set; }

        [Column("id_usuario")]
        [Required]
        public int IdUsuario { get; set; }

        [Column("id_pregunta")]
        [Required]
        public int IdPuntuacion { get; set; }

        [Column ("respuesta_dada")]
        public string? RespuestaDada { get; set; }
         
        [Column ("es_correcta")]
        public bool EsCorrecta { get; set; } = true;

        [Column ("tiempo_tomado")]
        public int? TiempoTomado { get; set; }

        [Column ("puntuacion_obtenida")]
        public int? PuntuacionObtenida { get; set; }

        [Column ("fecha_respuesta")]
        public DateTime FechaRespuesta { get; set; }
    }
}
