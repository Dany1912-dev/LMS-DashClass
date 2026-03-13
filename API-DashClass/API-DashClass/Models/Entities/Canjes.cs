using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_DashClass.Models.Entities
{
    [Table ("canjes")]
    public class Canjes
    {
        public enum EstadoCanje
        {
            Pendiente,
            Reclamado,
            Expirado
        }

        [Key]
        [Column("id_canje")]
        public int IdCanje { get; set; }

        [Column("id_recompensa")]
        [Required]
        public int IdRecompensa { get; set; }

        [Column("id_usuario")]
        [Required]
        public int IdUsuario { get; set; }

        [Column("puntos_gastados")]
        [Required]
        public int PuntosGastados { get; set; }

        [Column("codigo_canje")]
        [Required]
        [MaxLength(20)]
        public string CodigoCanje { get; set; }

        [Column("estado")]
        [Required]
        public EstadoCanje Estado { get; set; } = EstadoCanje.Pendiente;

        [Column("fecha_canje")]
        public DateTime FechaCanje { get; set; }

        [Column("fecha_reclamado")]
        public DateTime? FechaReclamado { get; set; }
    }
}
