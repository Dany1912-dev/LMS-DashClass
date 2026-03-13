using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace API_DashClass.Models.Entities
{
    [Table("evaluaciones")]
    public class Evaluaciones
    {
        public enum ModoEvaluacion
        {
            Kahoot,
            Formulario
        }
         public enum EstadoEvaluacion
        {
            Borrador,
            Listo,
            Activo,
            Completado
        }

        [Key]
        [Column("id_evaluacion")]
        public int IdEvaluacion { get; set; }

        [Column("id_curso")]
        [Required]
        public int IdCurso { get; set; }

        [Column("id_grupo")]
        public int? IdGrupo { get; set; }

        [Column("titulo")]
        [Required]
        [MaxLength(255)]
        public string Titulo { get; set; }

        [Column("descripcion")]
        public string? Descripcion { get; set; }

        [Column("modo")]
        [Required]
        public ModoEvaluacion Modo { get; set; }

        [Column("afecta_calificacion")]
        public bool AfectaCalificacion { get; set; } = false;

        [Column("puntos_academicos_maximos")]
        public int? PuntosAcademicosMaximos { get; set; }

        [Column("puntos_gamificacion")]
        public int PuntosGamificacion { get; set; }

        [Column("puntuacion_base")]
        public int? PuntuacionBase { get; set; }

        [Column("penalidad_tiempo")]
        public int? PenalidadTiempo { get; set; }

        [Column("mostrar_ranking_vivo")]
        public bool MostrarRankingVivo { get; set; } = false;

        [Column("permitir_entrada_tardia")]
        public bool PermitirEntradaTardia { get; set; } = false;

        [Column("estado")]
        public EstadoEvaluacion Estado { get; set; } = EstadoEvaluacion.Borrador;

        [Column("id_usuario")]
        [Required]
        public int IdUsuario { get; set; }

        [Column("fecha_creacion")]
        public DateTime FechaCreacion { get; set; }
    }
}
