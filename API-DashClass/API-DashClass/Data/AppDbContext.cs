using Microsoft.EntityFrameworkCore;
using API_DashClass.Models.Entities;

namespace API_DashClass.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Tablas principales
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Cursos> Cursos { get; set; }
        public DbSet<MetodosAuthUsers> MetodosAuthUsers { get; set; }
        public DbSet<Grupos> Grupos { get; set; }

        // Gestión de curso
        public DbSet<MiembrosCursos> MiembrosCurso { get; set; }
        public DbSet<InvitacionesCurso> InvitacionesCurso { get; set; }

        // Actividades y contenido
        public DbSet<Actividades> Actividades { get; set; }
        public DbSet<ActividadesGrupos> ActividadesGrupos { get; set; }
        public DbSet<CategoriasActividad> CategoriasActividad { get; set; }  // ← NUEVO
        public DbSet<MaterialesActividad> MaterialesActividad { get; set; }
        public DbSet<Anuncios> Anuncios { get; set; }
        public DbSet<MaterialesAnuncio> MaterialesAnuncio { get; set; }

        // Entregas y calificaciones
        public DbSet<Entregas> Entregas { get; set; }
        public DbSet<RecursosEntrega> RecursosEntrega { get; set; }
        public DbSet<Calificaciones> Calificaciones { get; set; }

        // Asistencia
        public DbSet<SesionesAsistencia> SesionesAsistencia { get; set; }
        public DbSet<RegistrosAsistencia> RegistrosAsistencia { get; set; }

        // Gamificación - Puntos
        public DbSet<TransaccionesPuntos> TransaccionesPuntos { get; set; }
        public DbSet<TransferenciasPuntos> TransferenciasPuntos { get; set; }

        // Gamificación - Recompensas
        public DbSet<Recompensas> Recompensas { get; set; }
        public DbSet<Canjes> Canjes { get; set; }

        // Gamificación - Logros
        public DbSet<Logros> Logros { get; set; }
        public DbSet<LogrosUsuario> LogrosUsuario { get; set; }

        // Evaluaciones
        public DbSet<Evaluaciones> Evaluaciones { get; set; }
        public DbSet<PreguntasEvaluacion> PreguntasEvaluacion { get; set; }
        public DbSet<SesionesEvaluacion> SesionesEvaluacion { get; set; }
        public DbSet<RespuestasEstudiantes> RespuestasEstudiantes { get; set; }

        // Estilos de aprendizaje
        public DbSet<EstilosAprendizaje> EstilosAprendizaje { get; set; }

        // Autenticación
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        // ========================================
        // CONFIGURACIÓN DEL MODELO
        // ========================================

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ========================================
            // CONFIGURACIÓN DE RELACIONES
            // ========================================

            // Usuario - TransferenciasPuntos (relación bidireccional)
            modelBuilder.Entity<TransferenciasPuntos>()
                .HasOne(t => t.UsuarioEmisor)
                .WithMany(u => u.TransferenciasEnviadas)
                .HasForeignKey(t => t.DesdeIdUsuario)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TransferenciasPuntos>()
                .HasOne(t => t.UsuarioReceptor)
                .WithMany(u => u.TransferenciasRecibidas)
                .HasForeignKey(t => t.HaciaIdUsuario)
                .OnDelete(DeleteBehavior.Restrict);

            // Entregas - Calificaciones (relación 1:1)
            modelBuilder.Entity<Calificaciones>()
                .HasOne(c => c.Entrega)
                .WithOne(e => e.Calificacion)
                .HasForeignKey<Calificaciones>(c => c.IdEntrega);

            // EstilosAprendizaje - Usuario (relación 1:1)
            modelBuilder.Entity<EstilosAprendizaje>()
                .HasOne(e => e.Usuario)
                .WithOne(u => u.EstiloAprendizaje)
                .HasForeignKey<EstilosAprendizaje>(e => e.IdUsuario);

            // Actividades - CategoriasActividad (nullable FK)
            modelBuilder.Entity<Actividades>()
                .HasOne(a => a.Categoria)
                .WithMany(c => c.Actividades)
                .HasForeignKey(a => a.IdCategoria)
                .OnDelete(DeleteBehavior.SetNull);

            // ========================================
            // ÍNDICES ÚNICOS
            // ========================================

            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Cursos>()
                .HasIndex(c => c.Codigo)
                .IsUnique();

            modelBuilder.Entity<MiembrosCursos>()
                .HasIndex(m => new { m.IdCurso, m.IdUsuario, m.IdGrupo })
                .IsUnique();

            modelBuilder.Entity<ActividadesGrupos>()
                .HasIndex(ag => new { ag.IdActividad, ag.IdGrupo })
                .IsUnique();

            modelBuilder.Entity<Entregas>()
                .HasIndex(e => new { e.IdActividad, e.IdUsuario, e.Version })
                .IsUnique();

            modelBuilder.Entity<RegistrosAsistencia>()
                .HasIndex(r => new { r.IdSesionAsistencia, r.IdUsuario })
                .IsUnique();

            modelBuilder.Entity<Canjes>()
                .HasIndex(c => c.CodigoCanje)
                .IsUnique();

            modelBuilder.Entity<SesionesEvaluacion>()
                .HasIndex(s => s.CodigoSesion)
                .IsUnique();

            modelBuilder.Entity<LogrosUsuario>()
                .HasIndex(l => new { l.IdLogro, l.IdUsuario })
                .IsUnique();

            modelBuilder.Entity<TransferenciasPuntos>()
                .HasIndex(t => t.CodigoTransferencia)
                .IsUnique();

            modelBuilder.Entity<EstilosAprendizaje>()
                .HasIndex(e => e.IdUsuario)
                .IsUnique();

            modelBuilder.Entity<Calificaciones>()
                .HasIndex(c => c.IdEntrega)
                .IsUnique();

            // ========================================
            // CONVERSIÓN DE ENUMS A STRING
            // ========================================

            modelBuilder.Entity<Usuario>()
                .Property(u => u.ProveedorAuthPrincipal)
                .HasConversion<string>();

            modelBuilder.Entity<MetodosAuthUsers>()
                .Property(m => m.ProveedorAuth)
                .HasConversion<string>();

            modelBuilder.Entity<MiembrosCursos>()
                .Property(m => m.Rol)
                .HasConversion<string>();

            modelBuilder.Entity<InvitacionesCurso>()
                .Property(i => i.Tipo)
                .HasConversion<string>();

            modelBuilder.Entity<Actividades>()
                .Property(a => a.Estatus)
                .HasConversion<string>();

            modelBuilder.Entity<MaterialesActividad>()
                .Property(m => m.Tipo)
                .HasConversion<string>();

            modelBuilder.Entity<MaterialesAnuncio>()
                .Property(m => m.Tipo)
                .HasConversion<string>();

            modelBuilder.Entity<Entregas>()
                .Property(e => e.Estado)
                .HasConversion<string>();

            modelBuilder.Entity<RecursosEntrega>()
                .Property(r => r.Tipo)
                .HasConversion<string>();

            modelBuilder.Entity<RegistrosAsistencia>()
                .Property(r => r.MetodoUsado)
                .HasConversion<string>();

            modelBuilder.Entity<TransaccionesPuntos>()
                .Property(t => t.Tipo)
                .HasConversion<string>();

            modelBuilder.Entity<TransaccionesPuntos>()
                .Property(t => t.Origen)
                .HasConversion<string>();

            modelBuilder.Entity<Canjes>()
                .Property(c => c.Estado)
                .HasConversion<string>();

            modelBuilder.Entity<Evaluaciones>()
                .Property(e => e.Modo)
                .HasConversion<string>();

            modelBuilder.Entity<Evaluaciones>()
                .Property(e => e.Estado)
                .HasConversion<string>();

            modelBuilder.Entity<PreguntasEvaluacion>()
                .Property(p => p.Tipo)
                .HasConversion<string>();

            // ========================================
            // VALORES POR DEFECTO
            // ========================================

            modelBuilder.Entity<Usuario>()
                .Property(u => u.Estatus)
                .HasDefaultValue(true);

            modelBuilder.Entity<Cursos>()
                .Property(c => c.Estatus)
                .HasDefaultValue(true);

            modelBuilder.Entity<Grupos>()
                .Property(g => g.Estatus)
                .HasDefaultValue(true);

            modelBuilder.Entity<MiembrosCursos>()
                .Property(m => m.Estatus)
                .HasDefaultValue(true);

            modelBuilder.Entity<InvitacionesCurso>()
                .Property(i => i.Estatus)
                .HasDefaultValue(true);

            modelBuilder.Entity<Actividades>()
                .Property(a => a.PuntosGamificacionMaximos)
                .HasDefaultValue(0);

            modelBuilder.Entity<Actividades>()
                .Property(a => a.PermiteEntregasTardias)
                .HasDefaultValue(false);

            modelBuilder.Entity<Anuncios>()
                .Property(a => a.EsImportante)
                .HasDefaultValue(false);

            modelBuilder.Entity<Anuncios>()
                .Property(a => a.Estatus)
                .HasDefaultValue(true);

            modelBuilder.Entity<SesionesAsistencia>()
                .Property(s => s.IntervaloQr)
                .HasDefaultValue(20);

            modelBuilder.Entity<SesionesAsistencia>()
                .Property(s => s.Estatus)
                .HasDefaultValue(true);

            modelBuilder.Entity<Entregas>()
                .Property(e => e.EsTardia)
                .HasDefaultValue(false);

            modelBuilder.Entity<Entregas>()
                .Property(e => e.Version)
                .HasDefaultValue(1);

            modelBuilder.Entity<MetodosAuthUsers>()
                .Property(m => m.TelefonoVerificado)
                .HasDefaultValue(false);

            modelBuilder.Entity<MetodosAuthUsers>()
                .Property(m => m.Verificado)
                .HasDefaultValue(false);

            modelBuilder.Entity<Recompensas>()
                .Property(r => r.Destacado)
                .HasDefaultValue(false);

            modelBuilder.Entity<Recompensas>()
                .Property(r => r.Estatus)
                .HasDefaultValue(true);

            modelBuilder.Entity<Evaluaciones>()
                .Property(e => e.AfectaCalificacion)
                .HasDefaultValue(false);

            modelBuilder.Entity<Evaluaciones>()
                .Property(e => e.PuntosGamificacion)
                .HasDefaultValue(0);

            modelBuilder.Entity<Evaluaciones>()
                .Property(e => e.MostrarRankingVivo)
                .HasDefaultValue(false);

            modelBuilder.Entity<Evaluaciones>()
                .Property(e => e.PermitirEntradaTardia)
                .HasDefaultValue(false);

            modelBuilder.Entity<PreguntasEvaluacion>()
                .Property(p => p.EnBancoPreguntas)
                .HasDefaultValue(false);

            modelBuilder.Entity<SesionesEvaluacion>()
                .Property(s => s.Estatus)
                .HasDefaultValue(true);

            modelBuilder.Entity<Logros>()
                .Property(l => l.Estatus)
                .HasDefaultValue(true);

            modelBuilder.Entity<TransferenciasPuntos>()
                .Property(t => t.Anonima)
                .HasDefaultValue(false);

            // ========================================
            // REFRESH TOKENS
            // ========================================

            modelBuilder.Entity<RefreshToken>()
                .HasIndex(r => r.Token)
                .IsUnique();

            modelBuilder.Entity<RefreshToken>()
                .Property(r => r.Revocado)
                .HasDefaultValue(false);

            modelBuilder.Entity<RefreshToken>()
                .HasOne(r => r.Usuario)
                .WithMany()
                .HasForeignKey(r => r.IdUsuario)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}