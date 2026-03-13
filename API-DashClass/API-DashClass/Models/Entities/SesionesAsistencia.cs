using System;
using System.Collections.Generic;
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
        public int IntervaloQr { get; set; }

        [Column("fecha_creacion")]
        public DateTime FechaCreacion { get; set; }

        [Column("fecha_expiracion")]
        [Required]
        public DateTime FechaExpiracion { get; set; }

        [Column("id_usuario")]
        [Required]
        public int IdUsuario { get; set; }

        [Column("estatus")]
        public bool Estatus { get; set; }

        // ========================================
        // NAVIGATION PROPERTIES
        // ========================================

        // Curso al que pertenece esta sesión de asistencia (FK)
        [ForeignKey("IdCurso")]
        public Cursos? Curso { get; set; }

        // Grupo al que está dirigida esta sesión (opcional) (FK)
        [ForeignKey("IdGrupo")]
        public Grupos? Grupo { get; set; }

        // Usuario (profesor) que creó esta sesión (FK)
        [ForeignKey("IdUsuario")]
        public Usuario? CreadoPor { get; set; }

        // Registros de asistencia de estudiantes en esta sesión
        public ICollection<RegistrosAsistencia>? RegistrosAsistencia { get; set; }
    }
}