using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace API_DashClass.Models.Entities
{
    [Table("grupos")]
    public class Grupos
    {
        [Key]
        [Column("id_grupo")]
        public int IdGrupo { get; set; }

        [Column("id_curso")]
        [Required]
        public int IdCurso { get; set; }

        [Column("nombre")]
        [Required]
        [MaxLength(100)]
        public string nombre { get; set; }

        [Column("descripcion")]
        public string? descripcion { get; set; }

        [Column("fecha_creacion")]
        [Required]
        public DateTime fechaCreacion { get; set; }

        [Column("estatus")]
        public bool Estatus { get; set; } = true;
    }
}
