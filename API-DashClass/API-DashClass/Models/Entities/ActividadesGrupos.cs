using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_DashClass.Models.Entities
{
    [Table("actividades_grupos")]
    public class ActividadesGrupos
    {
        [Key]
        [Column("id_actividad_grupo")]
        public int IdActividadGrupo { get; set; }

        [Column("id_actividad")]
        [Required]
        public int IdActividad { get; set; }

        [Column("id_grupo")]
        [Required]
        public int IdGrupo { get; set; }

        // ========================================
        // NAVIGATION PROPERTIES
        // ========================================

        // Actividad asociada (FK)
        [ForeignKey("IdActividad")]
        public Actividades? Actividad { get; set; }

        // Grupo asociado (FK)
        [ForeignKey("IdGrupo")]
        public Grupos? Grupo { get; set; }
    }
}