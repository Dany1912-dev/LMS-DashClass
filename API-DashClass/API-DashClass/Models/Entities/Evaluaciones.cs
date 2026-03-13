using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        public bool AfectaCalificacion { get; set; }

        [Column("puntos_academicos_maximos")]
        public int? PuntosAcademicosMaximos { get; set; }

        [Column("puntos_gamificacion")]
        public int PuntosGamificacion { get; set; }

        [Column("puntuacion_base")]
        public int? PuntuacionBase { get; set; }

        [Column("penalidad_tiempo")]
        public int? PenalidadTiempo { get; set; }

        [Column("mostrar_ranking_vivo")]
        public bool MostrarRankingVivo { get; set; }

        [Column("permitir_entrada_tardia")]
        public bool PermitirEntradaTardia { get; set; }

        [Column("estado")]
        public EstadoEvaluacion Estado { get; set; }

        [Column("id_usuario")]
        [Required]
        public int IdUsuario { get; set; }

        [Column("fecha_creacion")]
        public DateTime FechaCreacion { get; set; }

        // ========================================
        // NAVIGATION PROPERTIES
        // ========================================

        // Curso al que pertenece esta evaluación (FK)
        [ForeignKey("IdCurso")]
        public Cursos? Curso { get; set; }

        // Grupo al que está dirigida esta evaluación (opcional) (FK)
        [ForeignKey("IdGrupo")]
        public Grupos? Grupo { get; set; }

        // Usuario (profesor) que creó esta evaluación (FK)
        [ForeignKey("IdUsuario")]
        public Usuario? CreadoPor { get; set; }

        // Preguntas de esta evaluación
        public ICollection<PreguntasEvaluacion>? Preguntas { get; set; }

        // Sesiones activas de esta evaluación (para modo Kahoot)
        public ICollection<SesionesEvaluacion>? Sesiones { get; set; }
    }
}