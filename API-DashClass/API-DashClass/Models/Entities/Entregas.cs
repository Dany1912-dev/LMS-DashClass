using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_DashClass.Models.Entities
{
    [Table("entregas")]
    id_entrega INT PRIMARY KEY AUTO_INCREMENT,
    id_actividad INT NOT NULL,
    id_usuario INT NOT NULL,
    comentarios TEXT,
    fecha_entrega DATETIME DEFAULT CURRENT_TIMESTAMP,
    es_tardia BOOLEAN DEFAULT FALSE,
    version INT DEFAULT 1,
    estado ENUM('Entregada', 'Calificada', 'Reemplazada') DEFAULT 'Entregada',
    public class Entregas
    {
        public enum EstadoEntrega
        {
            Entregada,
            Calificada,
            Reemplazada
        }

        [Key]
        [Column("id_entrega")]
        public int IdEntrega { get; set; }

        [Column("id_actividad")]
        [Required]
        public int IdActividad { get; set; }

        [Column("id_usuario")]
        [Required]
        public int IdUsuario { get; set; }

        [Column("comentarios")]
        public string? Comentarios { get; set; }

        [Column("fecha_entrega")]
        public DateTime FechaEntrega { get; set; } = DateTime.Now;

        [Column("es_tardia")]
        public bool EsTardia { get; set; } = false;

        [Column("version")]
        public int Version { get; set; } = 1;

        [Column("estado")]
        public EstadoEntrega Estado { get; set; } = EstadoEntrega.Entregada;

    }
}
