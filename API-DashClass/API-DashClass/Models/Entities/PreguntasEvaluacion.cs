using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace API_DashClass.Models.Entities
{
    [Table("preguntas_evaluacion")]
    public class PreguntasEvaluacion
    {
        public enum TipoPregunta
        {
            OpcionMultiple,
            VerdaderoFalso,
            RespuestaCorta
        }
        [Key]
        [Column("id_pregunta")]
        public int IdPregunta { get; set; }

        [Column("id_evaluacion")]
        [Required]
        public int IdEvaluacion { get; set; }

        [Column("texto_pregunta")]
        [Required]
        public string TextoPregunta { get; set; }

        [Column("url_imagen")]
        [MaxLength(500)]
        public string? UrlImagen { get; set; }

        [Column("tipo")]
        [Required]
        public TipoPregunta Tipo { get; set; }

        [Column("opciones")]
        public string? Opciones { get; set; }

        [Column("respuestas_correctas")]
        public string? RespuestasCorrectas { get; set; }

        [Column("tiempo_limite")]
        public int? TiempoLimite { get; set; }

        [Column("orden")]
        [Required]
        public int Orden { get; set; }

        [Column("en_banco_preguntas")]
        public bool EnBancoPreguntas { get; set; } = false;
    }
}
