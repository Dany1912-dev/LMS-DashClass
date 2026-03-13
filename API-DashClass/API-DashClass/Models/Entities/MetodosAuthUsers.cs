using System;
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

        [Column("id_usuario_proveedor")]
        [MaxLength(255)]
        public string? IdUsuarioProveedor { get; set; }

        [Column("contrasena_hash")]
        [MaxLength(255)]
        public string? ContrasenaHash { get; set; }

        [Column("telefono")]
        [MaxLength(20)]
        public string? Telefono { get; set; }

        [Column("telefono_verificado")]
        public bool TelefonoVerificado { get; set; }

        [Column("email")]
        [MaxLength(255)]
        public string? Email { get; set; }

        [Column("verificado")]
        public bool Verificado { get; set; }

        [Column("vinculado_en")]
        public DateTime VinculadoEn { get; set; }

        [Column("ultimo_uso")]
        public DateTime? UltimoUso { get; set; }

        // ========================================
        // NAVIGATION PROPERTIES
        // ========================================

        // Usuario al que pertenece este método de autenticación (FK)
        [ForeignKey("IdUsuario")]
        public Usuario? Usuario { get; set; }
    }
}