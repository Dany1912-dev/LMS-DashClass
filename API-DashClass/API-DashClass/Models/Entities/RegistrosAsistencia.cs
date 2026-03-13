using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_DashClass.Models.Entities
{
    [Table("registros_asistencia")]
    public class RegistrosAsistencia
    {
        public enum MetodoAsistencia
        {
            QR,
            Codigo,
            Geolocalizacion
        }

        [Key]
        [Column("id_registro_asistencia")]
        public int IdRegistroAsistencia { get; set; }

        [Column("id_sesion_asistencia")]
        [Required]
        public int IdSesionAsistencia { get; set; }

        [Column("id_usuario")]
        [Required]
        public int IdUsuario { get; set; }


        [Column("metodo_usado")]
        [Required]
        public MetodoAsistencia MetodoUsado { get; set; }

        [Column("fecha_registro")]
        public DateTime FechaRegistro { get; set; }
    }
}
