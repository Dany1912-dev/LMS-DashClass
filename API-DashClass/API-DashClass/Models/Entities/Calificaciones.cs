using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_DashClass.Models.Entities
{
    [Table("calificaciones")]
    public class Calificaciones
    {
        [Key]
        [Column("id_calificacion")]
        public int IdCalificacion { get; set; }

        [Column("id_entrega")]
        [Required]
        public int IdEntrega { get; set; }

        [Column("puntuacion")]
        [Required]
        public decimal Puntuacion { get; set; }


        [Column("retroalimentacion")]
        public string? Retroalimentacion { get; set; }

        [Column("datos_rubrica")]
        public string? DatosRubrica { get; set; }

        [Column("id_usuario")]
        [Required]
        public int IdUsuario { get; set; }

        [Column("fecha_calificacion")]
        public DateTime FechaCalificacion  { get; set; }
    }
}
