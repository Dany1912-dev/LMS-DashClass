using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace API_DashClass.Models.Entities
{
    [Table("transferencias_puntos")]
    public class TransferenciasPuntos
    {
        [Key]
        [Column("id_transferencia")]
        public int IdTransferencia { get; set; }

        [Column("desde_id_usuario")]
        [Required]
        public int DesdeIdUsuario { get; set; }

        [Column("hacia_id_usuario")]
        [Required]
        public int DesdeDesde { get; set; }

        [Column("id_curso")]
        [Required]
        public int IdCurso { get; set; }

        [Column("cantidad")]
        [Required]
        public int Cantidad { get; set; }

        [Column("mensaje")]
        public string? Mensaje { get; set; }

        [Column("anonima")]
        public bool Anonima { get; set; } = false;

        [Column("codigo_transferencia")]
        [Required]
        [MaxLength(20)]
        public string CodigoTransferencia { get; set; }

        [Column("fecha_transferencia")]
        public DateTime FechaTransferencia { get; set; } = DateTime.Now;
    }
}
