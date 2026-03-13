using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_DashClass.Models.Entities
{
    [Table("transacciones_puntos")]
    public class TransaccionesPuntos
    {
        public enum TipoTransaccion
        {
            Ganado,
            Gastado,
            Transferido,
            Manual
        }

        public enum OrigenTransaccion
        {
            Calificacion,
            Asistencia,
            Evaluacion,
            Social,
            Recompensa,
            Manual
        }

        [Key]
        [Column("id_transaccion")]
        public int IdTransaccion { get; set; }

        [Column("id_usuario")]
        [Required]
        public int IdUsuario { get; set; }

        [Column("id_curso")]
        [Required]
        public int IdCurso { get; set; }

        [Column("tipo")]
        [Required]
        public TipoTransaccion Tipo { get; set; }

        [Column("origen")]
        [Required]
        public OrigenTransaccion Origen { get; set; }

        [Column("cantidad")]
        [Required]
        public int Cantidad { get; set; }

        [Column("balance_despues")]
        [Required]
        public int BalanceDespues { get; set; }

        [Column("id_referencia")]
        public int? IdReferencia { get; set; }

        [Column("descripcion")]
        [MaxLength(500)]
        public string? Descripcion { get; set; }

        [Column("fecha_creacion")]
        public DateTime FechaCreacion { get; set; }

        // ========================================
        // NAVIGATION PROPERTIES
        // ========================================

        // Usuario al que pertenece esta transacción (FK)
        [ForeignKey("IdUsuario")]
        public Usuario? Usuario { get; set; }

        // Curso en el que ocurrió esta transacción (FK)
        [ForeignKey("IdCurso")]
        public Cursos? Curso { get; set; }
    }
}