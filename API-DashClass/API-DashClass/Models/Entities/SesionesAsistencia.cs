using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_DashClass.Models.Entities
{
    [Table("sesiones_asistencia")]
    public class SesionesAsistencia
    {
        [Key]
        [Column("id_sesion")]
        public int IdSesion { get; set; }

        [Column("id_curso")]
        [Required]
        public int IdCurso { get; set; }

        [Column("id_grupo")]
        public int? IdGrupo { get; set; }

        [Column("nombre")]
        [Required]
        [MaxLength(255)]
        public string Nombre { get; set; }

        [Column("clave_secreta")]
        [Required]
        [MaxLength(255)]
        public string ClaveSecreta { get; set; }

        [Column("codigo_verificacion")]
        [Required]
        [MaxLength(6)]
        public string CodigoVerificacion { get; set; }

        [Column("intervalo_qr")]
        [Required]
        public int IntervaloQr { get; set; } = 20;

        [Column("fecha_creacion")]
        public DateTime FechaCreacion { get; set; }

        [Column("fecha_expiracion")]
        [Required]
        public DateTime FechaExpiracion { get; set; }

        [Column("id_usuario")]
        [Required]
        public int IdUsuario { get; set; }

        [Column("estatus")]
        public bool Estatus { get; set; } = true;
    }
}