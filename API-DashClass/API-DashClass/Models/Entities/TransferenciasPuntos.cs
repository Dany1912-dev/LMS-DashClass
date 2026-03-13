using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        public int HaciaIdUsuario { get; set; }

        [Column("id_curso")]
        [Required]
        public int IdCurso { get; set; }

        [Column("cantidad")]
        [Required]
        public int Cantidad { get; set; }

        [Column("mensaje")]
        public string? Mensaje { get; set; }

        [Column("anonima")]
        public bool Anonima { get; set; }

        [Column("codigo_transferencia")]
        [Required]
        [MaxLength(20)]
        public string CodigoTransferencia { get; set; }

        [Column("fecha_transferencia")]
        public DateTime FechaTransferencia { get; set; }

        // ========================================
        // NAVIGATION PROPERTIES
        // ========================================

        // Usuario que envía los puntos (FK)
        [ForeignKey("DesdeIdUsuario")]
        public Usuario? UsuarioEmisor { get; set; }

        // Usuario que recibe los puntos (FK)
        [ForeignKey("HaciaIdUsuario")]
        public Usuario? UsuarioReceptor { get; set; }

        // Curso en el que ocurre la transferencia (FK)
        [ForeignKey("IdCurso")]
        public Cursos? Curso { get; set; }
    }
}