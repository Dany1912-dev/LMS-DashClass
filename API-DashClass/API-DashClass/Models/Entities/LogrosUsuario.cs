using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_DashClass.Models.Entities
{
    [Table("logros_usuario")]
    public class LogrosUsuario
    {
        [Key]
        [Column("id_logro_usuario")]
        public int IdLogroUsuario { get; set; }

        [Column("id_logro")]
        [Required]
        public int IdLogro { get; set; }

        [Column("id_usuario")]
        [Required]
        public int IdUsuario { get; set; }

        [Column("fecha_desbloqueo")]
        public DateTime FechaDesbloqueo { get; set; }

    }
}
