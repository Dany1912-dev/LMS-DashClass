using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_DashClass.Models.Entities
{


    [Table("metodos_auth_users")]
    public class MetodosAuthUsers
    {
        public enum EnumsProveedorAuth
        {
            Google,
            Facebook,
            Microsoft,
            Telefono,
            Apple,
            Local
        }

        [Key]
        [Column("id_metodo")]
        public int IdMetodo { get; set; }

        [Column("id_usuario")]
        [Required]
        public int IdUsuario { get; set; }

        [Column("proveedor")]
        public EnumsProveedorAuth ProveedorAuth { get; set; }

        [Column("id_usuarios_proveedor")]
        [MaxLength(255)]
        public string? IdUsuariosProveedor { get; set; }

        [Column("contrasena_hash")]
        [MaxLength(255)]
        public string? ContrasenaHash { get; set; }

        [Column("telefono")]
        [MaxLength(20)]
        public string? Telefono { get; set; }

        [Column("telefono_verificado")]
        public bool TelefonoVerificado { get; set; } = false;

        [Column("email")]
        [MaxLength(255)]
        public string? Email { get; set; }

        [Column("verificado")]
        public bool Verificado { get; set; } = false;

        [Column("verificado_en")]
        public DateTime VerificadoEn { get; set; }

        [Column("ultimo_uso")]
        public DateTime? UltimoUso { get; set; }
    }
}
