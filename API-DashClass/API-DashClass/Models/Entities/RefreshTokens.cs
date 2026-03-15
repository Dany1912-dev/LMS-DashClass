using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_DashClass.Models.Entities
{
    [Table("refresh_tokens")]
    public class RefreshToken
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("id_usuario")]
        [Required]
        public int IdUsuario { get; set; }

        [Column("token")]
        [Required]
        [MaxLength(255)]
        public string Token { get; set; } = string.Empty;

        [Column("expiracion")]
        public DateTime Expiracion { get; set; }

        [Column("creado_en")]
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

        [Column("revocado")]
        public bool Revocado { get; set; } = false;

        [ForeignKey("IdUsuario")]
        public Usuario? Usuario { get; set; }
    }
}