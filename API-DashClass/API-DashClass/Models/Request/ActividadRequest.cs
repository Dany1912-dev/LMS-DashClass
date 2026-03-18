using System.ComponentModel.DataAnnotations;

namespace API_DashClass.Models.Request
{
    public class CrearActividadRequest
    {
        [Required]
        public int IdCurso { get; set; }

        [Required]
        public int IdUsuario { get; set; }

        /// <summary>
        /// Categoría a la que pertenece esta actividad (nullable).
        /// Si es null, la actividad no afecta la calificación final ponderada.
        /// </summary>
        public int? IdCategoria { get; set; }

        [Required]
        [MaxLength(255)]
        public string Titulo { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        [Required]
        [Range(1, 100, ErrorMessage = "Los puntos máximos deben estar entre 1 y 100")]
        public int PuntosMaximos { get; set; }

        [Range(0, int.MaxValue)]
        public int PuntosGamificacionMaximos { get; set; } = 0;

        public DateTime? FechaLimite { get; set; }

        public bool PermiteEntregasTardias { get; set; } = false;

        /// <summary>Borrador, Publicado, Programado, Archivado</summary>
        public string Estatus { get; set; } = "Borrador";

        public DateTime? FechaProgramada { get; set; }

        /// <summary>
        /// IDs de los grupos a los que se asigna esta actividad.
        /// Si está vacío o null, se asigna a TODOS los grupos del curso.
        /// </summary>
        public List<int>? IdGrupos { get; set; }
    }

    public class ActualizarActividadRequest
    {
        public int? IdCategoria { get; set; }

        [Required]
        [MaxLength(255)]
        public string Titulo { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        [Required]
        [Range(1, 100, ErrorMessage = "Los puntos máximos deben estar entre 1 y 100")]
        public int PuntosMaximos { get; set; }

        [Range(0, int.MaxValue)]
        public int PuntosGamificacionMaximos { get; set; } = 0;

        public DateTime? FechaLimite { get; set; }

        public bool PermiteEntregasTardias { get; set; } = false;

        public string Estatus { get; set; } = "Borrador";

        public DateTime? FechaProgramada { get; set; }

        public List<int>? IdGrupos { get; set; }
    }
}