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
            Recomensa,
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
        [Required]
        public DateTime FechaCreacion { get; set; }
    }
}