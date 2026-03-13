using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.NetworkInformation;

namespace API_DashClass.Models.Entities
{
    [Table("sesiones_evaluacion")]
    public class SesionesEvaluacion
    {
        [Key]
        [Column("id_sesion_evaluacion")]
        public int IdSesionEvaluacion { get; set; }

        [Column("id_evaluacion")]
        [Required]
        public int IdEvaluacion { get; set; }

        [Column("codigo_sesion")]
        [MaxLength(6)]
        [Required]
        public string CodigoSesion { get; set; }

        [Column("fecha_inicio")]
        public DateTime FechaInicio { get; set; } = DateTime.Now;

        [Column("fecha_fin")]
        public DateTime? FechaFin { get; set; }

        [Column("estatus")]
        public bool Estatus { get; set; } = true;
    }
}
