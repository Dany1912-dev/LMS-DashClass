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

            // ========================================
            // ÍNDICES ÚNICOS
            // ========================================

            // Usuario - Email único
            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Curso - Código único
            modelBuilder.Entity<Cursos>()
                .HasIndex(c => c.Codigo)
                .IsUnique();

            // MiembrosCurso - Combinación única
            modelBuilder.Entity<MiembrosCursos>()
                .HasIndex(m => new { m.IdCurso, m.IdUsuario, m.IdGrupo })
                .IsUnique();

            // ActividadesGrupos - Combinación única
            modelBuilder.Entity<ActividadesGrupos>()
                .HasIndex(ag => new { ag.IdActividad, ag.IdGrupo })
                .IsUnique();

            // Entregas - Combinación única
            modelBuilder.Entity<Entregas>()
                .HasIndex(e => new { e.IdActividad, e.IdUsuario, e.Version })
                .IsUnique();

            // RegistrosAsistencia - Combinación única
            modelBuilder.Entity<RegistrosAsistencia>()
                .HasIndex(r => new { r.IdSesionAsistencia, r.IdUsuario })
                .IsUnique();

            // Canjes - Código único
            modelBuilder.Entity<Canjes>()
                .HasIndex(c => c.CodigoCanje)
                .IsUnique();

            // SesionesEvaluacion - Código único
            modelBuilder.Entity<SesionesEvaluacion>()
                .HasIndex(s => s.CodigoSesion)
                .IsUnique();

            // LogrosUsuario - Combinación única
            modelBuilder.Entity<LogrosUsuario>()
                .HasIndex(l => new { l.IdLogro, l.IdUsuario })
                .IsUnique();

            // TransferenciasPuntos - Código único
            modelBuilder.Entity<TransferenciasPuntos>()
                .HasIndex(t => t.CodigoTransferencia)
                .IsUnique();

            // EstilosAprendizaje - IdUsuario único (1:1)
            modelBuilder.Entity<EstilosAprendizaje>()
                .HasIndex(e => e.IdUsuario)
                .IsUnique();

            // Calificaciones - IdEntrega único (1:1)
            modelBuilder.Entity<Calificaciones>()
                .HasIndex(c => c.IdEntrega)
                .IsUnique();

            // ========================================
            // CONVERSIÓN DE ENUMS A STRING
            // ========================================

            // Usuario - ProveedorAuthPrincipal
            modelBuilder.Entity<Usuario>()
                .Property(u => u.ProveedorAuthPrincipal)
                .HasConversion<string>();

            // MetodosAuthUsers - Proveedor
            modelBuilder.Entity<MetodosAuthUsers>()
                .Property(m => m.ProveedorAuth)
                .HasConversion<string>();

            // MiembrosCursos - Rol
            modelBuilder.Entity<MiembrosCursos>()
                .Property(m => m.Rol)
                .HasConversion<string>();

            // InvitacionesCurso - Tipo
            modelBuilder.Entity<InvitacionesCurso>()
                .Property(i => i.Tipo)
                .HasConversion<string>();

            // Actividades - Estatus
            modelBuilder.Entity<Actividades>()
                .Property(a => a.Estatus)
                .HasConversion<string>();

            // MaterialesActividad - Tipo
            modelBuilder.Entity<MaterialesActividad>()
                .Property(m => m.Tipo)
                .HasConversion<string>();

            // MaterialesAnuncio - Tipo
            modelBuilder.Entity<MaterialesAnuncio>()
                .Property(m => m.Tipo)
                .HasConversion<string>();

            // Entregas - Estado
            modelBuilder.Entity<Entregas>()
                .Property(e => e.Estado)
                .HasConversion<string>();

            // RecursosEntrega - Tipo
            modelBuilder.Entity<RecursosEntrega>()
                .Property(r => r.Tipo)
                .HasConversion<string>();

            // RegistrosAsistencia - MetodoUsado
            modelBuilder.Entity<RegistrosAsistencia>()
                .Property(r => r.MetodoUsado)
                .HasConversion<string>();

            // TransaccionesPuntos - Tipo y Origen
            modelBuilder.Entity<TransaccionesPuntos>()
                .Property(t => t.Tipo)
                .HasConversion<string>();

            modelBuilder.Entity<TransaccionesPuntos>()
                .Property(t => t.Origen)
                .HasConversion<string>();

            // Canjes - Estado
            modelBuilder.Entity<Canjes>()
                .Property(c => c.Estado)
                .HasConversion<string>();

            // Evaluaciones - Modo y Estado
            modelBuilder.Entity<Evaluaciones>()
                .Property(e => e.Modo)
                .HasConversion<string>();

            modelBuilder.Entity<Evaluaciones>()
                .Property(e => e.Estado)
                .HasConversion<string>();

            // PreguntasEvaluacion - Tipo
            modelBuilder.Entity<PreguntasEvaluacion>()
                .Property(p => p.Tipo)
                .HasConversion<string>();

            // ========================================
            // VALORES POR DEFECTO
            // ========================================

            // Usuario - Estatus por defecto
            modelBuilder.Entity<Usuario>()
                .Property(u => u.Estatus)
                .HasDefaultValue(true);

            // Cursos - Estatus por defecto
            modelBuilder.Entity<Cursos>()
                .Property(c => c.Estatus)
                .HasDefaultValue(true);

            // Grupos - Estatus por defecto
            modelBuilder.Entity<Grupos>()
                .Property(g => g.Estatus)
                .HasDefaultValue(true);

            // MiembrosCursos - Estatus por defecto
            modelBuilder.Entity<MiembrosCursos>()
                .Property(m => m.Estatus)
                .HasDefaultValue(true);

            // InvitacionesCurso - Estatus por defecto
            modelBuilder.Entity<InvitacionesCurso>()
                .Property(i => i.Estatus)
                .HasDefaultValue(true);

            // Actividades - PuntosGamificacionMaximos por defecto
            modelBuilder.Entity<Actividades>()
                .Property(a => a.PuntosGamificacionMaximos)
                .HasDefaultValue(0);

            // Actividades - PermiteEntregasTardias por defecto
            modelBuilder.Entity<Actividades>()
                .Property(a => a.PermiteEntregasTardias)
                .HasDefaultValue(false);

            // Anuncios - EsImportante por defecto
            modelBuilder.Entity<Anuncios>()
                .Property(a => a.EsImportante)
                .HasDefaultValue(false);

            // Anuncios - Estatus por defecto
            modelBuilder.Entity<Anuncios>()
                .Property(a => a.Estatus)
                .HasDefaultValue(true);

            // SesionesAsistencia - IntervaloQr por defecto
            modelBuilder.Entity<SesionesAsistencia>()
                .Property(s => s.IntervaloQr)
                .HasDefaultValue(20);

            // SesionesAsistencia - Estatus por defecto
            modelBuilder.Entity<SesionesAsistencia>()
                .Property(s => s.Estatus)
                .HasDefaultValue(true);

            // Entregas - EsTardia por defecto
            modelBuilder.Entity<Entregas>()
                .Property(e => e.EsTardia)
                .HasDefaultValue(false);

            // Entregas - Version por defecto
            modelBuilder.Entity<Entregas>()
                .Property(e => e.Version)
                .HasDefaultValue(1);

            // MetodosAuthUsers - TelefonoVerificado por defecto
            modelBuilder.Entity<MetodosAuthUsers>()
                .Property(m => m.TelefonoVerificado)
                .HasDefaultValue(false);

            // MetodosAuthUsers - Verificado por defecto
            modelBuilder.Entity<MetodosAuthUsers>()
                .Property(m => m.Verificado)
                .HasDefaultValue(false);

            // Recompensas - Destacado por defecto
            modelBuilder.Entity<Recompensas>()
                .Property(r => r.Destacado)
                .HasDefaultValue(false);

            // Recompensas - Estatus por defecto
            modelBuilder.Entity<Recompensas>()
                .Property(r => r.Estatus)
                .HasDefaultValue(true);

            // Evaluaciones - AfectaCalificacion por defecto
            modelBuilder.Entity<Evaluaciones>()
                .Property(e => e.AfectaCalificacion)
                .HasDefaultValue(false);

            // Evaluaciones - PuntosGamificacion por defecto
            modelBuilder.Entity<Evaluaciones>()
                .Property(e => e.PuntosGamificacion)
                .HasDefaultValue(0);

            // Evaluaciones - MostrarRankingVivo por defecto
            modelBuilder.Entity<Evaluaciones>()
                .Property(e => e.MostrarRankingVivo)
                .HasDefaultValue(false);

            // Evaluaciones - PermitirEntradaTardia por defecto
            modelBuilder.Entity<Evaluaciones>()
                .Property(e => e.PermitirEntradaTardia)
                .HasDefaultValue(false);

            // PreguntasEvaluacion - EnBancoPreguntas por defecto
            modelBuilder.Entity<PreguntasEvaluacion>()
                .Property(p => p.EnBancoPreguntas)
                .HasDefaultValue(false);

            // SesionesEvaluacion - Estatus por defecto
            modelBuilder.Entity<SesionesEvaluacion>()
                .Property(s => s.Estatus)
                .HasDefaultValue(true);

            // Logros - Estatus por defecto
            modelBuilder.Entity<Logros>()
                .Property(l => l.Estatus)
                .HasDefaultValue(true);

            // TransferenciasPuntos - Anonima por defecto
            modelBuilder.Entity<TransferenciasPuntos>()
                .Property(t => t.Anonima)
                .HasDefaultValue(false);

            // ========================================
            // REFRESH TOKENS
            // ========================================

            // RefreshToken - Token único
            modelBuilder.Entity<RefreshToken>()
                .HasIndex(r => r.Token)
                .IsUnique();

            // RefreshToken - Revocado por defecto
            modelBuilder.Entity<RefreshToken>()
                .Property(r => r.Revocado)
                .HasDefaultValue(false);

            // RefreshToken - Relación con Usuario
            modelBuilder.Entity<RefreshToken>()
                .HasOne(r => r.Usuario)
                .WithMany()
                .HasForeignKey(r => r.IdUsuario)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}