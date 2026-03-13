using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace API_DashClass.Models.Entities
{
    [Table("logros")]
    public class Logros
    {
        [Key]
        [Column("id_logro")]
        public int IdLogro { get; set; }

        [Column("id_curso")]
        [Required]
        public int IdCurso { get; set; }

        [Column("nombre")]
        [Required]
        [MaxLength(255)]
        public string Nombre { get; set; }

        [Column("descripcion")]
        public string Descripcion { get; set; }

        [Column("url_icono")]
        [MaxLength(500)]
        public string UrlIcono { get; set; }

        [Column("criterios")]
        public string Criterios { get; set; }

        [Column("estatus")]
        public bool Estatus { get; set; } = true;

        [Column("id_usuario")]
        [Required]
        public int IdUsuario { get; set; }

        [Column("fecha_creacion")]
        public DateTime FechaCreacion { get; set; }
    }
}
