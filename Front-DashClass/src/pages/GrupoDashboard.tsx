import "../styles/GrupoDashboard.css";
import { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import Layout from "../components/Layout";
import { API } from "../api";
import { Users, ClipboardList, Trophy, Tag, Calendar, Copy, Check, Plus, Zap, Eye, Archive, RotateCcw, Trash2, ChevronLeft } from "lucide-react";

interface Miembro {
  idMiembroCurso: number;
  idUsuario: number;
  nombreCompleto: string;
  email: string;
  fotoPerfilUrl?: string;
  rol: string;
  nombreGrupo?: string | null;
}

interface GrupoActividad {
  idGrupo: number;
  nombre: string;
}

interface Actividad {
  idActividad: number;
  titulo: string;
  descripcion?: string;
  puntosMaximos: number;
  puntosGamificacionMaximos: number;
  fechaLimite?: string;
  permiteEntregasTardias: boolean;
  estatus: string;
  fechaCreacion: string;
  nombreProfesor: string;
  nombreCategoria?: string;
  pesoCategoria?: number;
  esParaTodos: boolean;
  grupos: GrupoActividad[];
}

interface Invitacion {
  codigo?: string;
  token?: string;
}

interface Grupo {
  idGrupo: number;
  nombre: string;
  descripcion?: string;
  invitacion: Invitacion;
  totalMiembros: number;
}

interface Curso {
  idCurso: number;
  nombre: string;
  imagenBanner: string;
  idUsuario: number;
  grupos: Grupo[];
}

import banner1 from "../assets/banners/banner_1.jpg";
import banner2 from "../assets/banners/banner_2.jpg";
import banner3 from "../assets/banners/banner_3.jpg";
import banner4 from "../assets/banners/banner_4.jpeg";
import banner5 from "../assets/banners/banner_5.jpg";
import banner6 from "../assets/banners/banner_6.jpg";

const banners = [banner1, banner2, banner3, banner4, banner5, banner6];

export default function GrupoDashboard() {
  const { idCurso, idGrupo } = useParams<{ idCurso: string; idGrupo: string }>();
  const navigate = useNavigate();
  const usuario  = JSON.parse(localStorage.getItem("usuario") || "{}");

  const [curso,       setCurso]       = useState<Curso | null>(null);
  const [grupo,       setGrupo]       = useState<Grupo | null>(null);
  const [miembros,    setMiembros]    = useState<Miembro[]>([]);
  const [actividades, setActividades] = useState<Actividad[]>([]);
  const [categorias,  setCategorias]  = useState<{idCategoria: number; nombre: string; peso: number}[]>([]);
  const [loading,     setLoading]     = useState(true);
  const [tab,         setTab]         = useState<"actividades" | "miembros" | "puntos">("actividades");
  const [copiado,     setCopiado]     = useState(false);
  const [menuAbierto, setMenuAbierto] = useState<number | null>(null);

  // Puntos (alumno)
  const [balance,      setBalance]      = useState<any>(null);
  const [historial,    setHistorial]    = useState<any[]>([]);
  const [ranking,      setRanking]      = useState<any[]>([]);

  // Modal nueva actividad
  const [modalActividad,      setModalActividad]      = useState(false);
  const [errorActividad,      setErrorActividad]      = useState("");
  const [loadingActividad,    setLoadingActividad]    = useState(false);
  const [formActividad, setFormActividad] = useState({
    titulo: "",
    descripcion: "",
    puntosMaximos: 100,
    puntosGamificacion: 0,
    fechaLimite: "",
    permiteEntregasTardias: false,
    estatus: "Borrador" as "Borrador" | "Publicado",
    idCategoria: null as number | null,
    soloEsteGrupo: true,
  });

  const esMaestro = curso?.idUsuario === usuario.idUsuario;

  const cargarDatos = async () => {
    try {
      const [resCurso, resMiembros, resActividades] = await Promise.all([
        fetch(API.cursoPorId(Number(idCurso)), {
          headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
        }),
        fetch(API.miembrosCurso(Number(idCurso)), {
          headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
        }),
        fetch(API.actividadesGrupo(Number(idCurso), Number(idGrupo)), {
          headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
        }),
      ]);

      const dataCurso       = await resCurso.json();
      const dataMiembros    = await resMiembros.json();
      const dataActividades = await resActividades.json();

      setCurso(dataCurso);

      // Encontrar el grupo actual dentro del curso
      const grupoActual = dataCurso?.grupos?.find(
        (g: Grupo) => g.idGrupo === Number(idGrupo)
      ) ?? null;
      setGrupo(grupoActual);

      // Filtrar miembros de este grupo
      const miembrosGrupo = (dataMiembros.miembros ?? []).filter(
        (m: Miembro) => m.nombreGrupo === grupoActual?.nombre || m.rol === "Profesor"
      );
      setMiembros(miembrosGrupo);

      setActividades(dataActividades.actividades ?? []);
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }

    // Categorías — separadas para no romper la carga principal
    try {
      const resCats = await fetch(API.categoriasCurso(Number(idCurso)), {
        headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
      });
      if (resCats.ok) {
        const data = await resCats.json();
        setCategorias(data.categorias ?? []);
      }
    } catch (err) {
      console.warn("No se pudieron cargar categorías:", err);
    }

    // Puntos del alumno — solo si no es maestro
    const esMaestroActual = curso?.idUsuario === usuario.idUsuario;
    if (!esMaestroActual) {
      try {
        const [resBalance, resHistorial, resRanking] = await Promise.all([
          fetch(API.balancePuntos(usuario.idUsuario, Number(idCurso)), {
            headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
          }),
          fetch(`${import.meta.env.VITE_API_URL ?? "http://localhost:5000"}/api/points/transactions/${usuario.idUsuario}/${idCurso}?limit=20`, {
            headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
          }),
          fetch(API.rankingCurso(Number(idCurso)), {
            headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
          }),
        ]);
        if (resBalance.ok)   { const d = await resBalance.json();   setBalance(d); }
        if (resHistorial.ok) { const d = await resHistorial.json(); setHistorial(d.transactions ?? []); }
        if (resRanking.ok)   { const d = await resRanking.json();   setRanking(d.ranking ?? []); }
      } catch (err) {
        console.warn("No se pudieron cargar puntos:", err);
      }
    }
  };

  useEffect(() => { cargarDatos(); }, [idCurso, idGrupo]);

  const handleCopiar = () => {
    if (!grupo?.invitacion?.codigo) return;
    navigator.clipboard.writeText(grupo.invitacion.codigo);
    setCopiado(true);
    setTimeout(() => setCopiado(false), 2000);
  };

  const handleCrearActividad = async () => {
    setErrorActividad("");
    if (!formActividad.titulo.trim()) { setErrorActividad("El título es obligatorio"); return; }
    if (formActividad.puntosMaximos < 1 || formActividad.puntosMaximos > 100) {
      setErrorActividad("Los puntos máximos deben estar entre 1 y 100");
      return;
    }
    setLoadingActividad(true);
    try {
      const body = {
        idCurso: Number(idCurso),
        idUsuario: usuario.idUsuario,
        idCategoria: formActividad.idCategoria,
        titulo: formActividad.titulo.trim(),
        descripcion: formActividad.descripcion.trim() || null,
        puntosMaximos: formActividad.puntosMaximos,
        puntosGamificacionMaximos: formActividad.puntosGamificacion,
        fechaLimite: formActividad.fechaLimite || null,
        permiteEntregasTardias: formActividad.permiteEntregasTardias,
        estatus: formActividad.estatus,
        idGrupos: formActividad.soloEsteGrupo ? [Number(idGrupo)] : [],
      };
      const res = await fetch(API.crearActividad, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${localStorage.getItem("token")}`,
        },
        body: JSON.stringify(body),
      });
      if (!res.ok) {
        const data = await res.json();
        throw new Error(data.message || "Error al crear actividad");
      }
      setModalActividad(false);
      setFormActividad({
        titulo: "", descripcion: "", puntosMaximos: 100,
        puntosGamificacion: 0, fechaLimite: "",
        permiteEntregasTardias: false, estatus: "Borrador",
        idCategoria: null, soloEsteGrupo: true,
      });
      cargarDatos();
    } catch (err: any) {
      setErrorActividad(err.message);
    } finally {
      setLoadingActividad(false);
    }
  };

  const handleCambiarEstatus = async (idActividad: number, estatus: string) => {
    try {
      const res = await fetch(
        `${import.meta.env.VITE_API_URL ?? "http://localhost:5000"}/api/actividades/${idActividad}/estatus`,
        {
          method: "PATCH",
          headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${localStorage.getItem("token")}`,
          },
          body: JSON.stringify(estatus),
        }
      );
      if (!res.ok) throw new Error("Error al cambiar estatus");
      setMenuAbierto(null);
      cargarDatos();
    } catch (err) {
      console.error(err);
    }
  };

  const handleEliminar = async (idActividad: number) => {
    if (!confirm("¿Seguro que quieres eliminar esta actividad? Se archivará y dejará de ser visible para los alumnos.")) return;
    await handleCambiarEstatus(idActividad, "Archivado");
  };

  const getBanner = (imagenBanner: string) => {
    const index = Number(imagenBanner);
    if (!isNaN(index) && index >= 1 && index <= banners.length) return banners[index - 1];
    return null;
  };

  const formatFecha = (fecha?: string) => {
    if (!fecha) return null;
    return new Date(fecha).toLocaleDateString("es-MX", {
      day: "2-digit", month: "short", year: "numeric",
      hour: "2-digit", minute: "2-digit",
    });
  };

  const getEstatusColor = (estatus: string) => {
    switch (estatus) {
      case "Publicado":  return "estatus-publicado";
      case "Borrador":   return "estatus-borrador";
      case "Archivado":  return "estatus-archivado";
      case "Programado": return "estatus-programado";
      default: return "";
    }
  };

  if (loading) {
    return (
      <Layout titulo="Grupo">
        <p style={{ color: "var(--text-muted)", textAlign: "center", padding: "3rem" }}>Cargando...</p>
      </Layout>
    );
  }

  if (!grupo || !curso) {
    return (
      <Layout titulo="Grupo">
        <p style={{ color: "var(--text-muted)", textAlign: "center", padding: "3rem" }}>Grupo no encontrado.</p>
      </Layout>
    );
  }

  const estudiantes = miembros.filter(m => m.rol === "Estudiante");
  const profesores  = miembros.filter(m => m.rol === "Profesor");

  return (
    <Layout titulo={grupo.nombre}>
      <div className="grupo-page">

        {/* Breadcrumb */}
        <div className="grupo-breadcrumb">
          <button className="breadcrumb-btn" onClick={() => navigate(`/cursos/${idCurso}`)}>
            <ChevronLeft size={14} strokeWidth={1.5} /> {curso.nombre}
          </button>
          <span className="breadcrumb-sep">/</span>
          <span className="breadcrumb-actual">{grupo.nombre}</span>
        </div>

        {/* Header */}
        <div className="grupo-header">
          <div className="grupo-header-info">
            <h1 className="grupo-titulo">{grupo.nombre}</h1>
            {grupo.descripcion && <p className="grupo-desc">{grupo.descripcion}</p>}
            <div className="grupo-header-meta">
              <span className="grupo-meta-item"><Users size={13} strokeWidth={1.5} /> {estudiantes.length} estudiantes</span>
              <span className="grupo-meta-item"><ClipboardList size={13} strokeWidth={1.5} /> {actividades.length} actividades</span>
            </div>
          </div>

          <div className="grupo-header-acciones">
            {/* Código de invitación */}
            {esMaestro && grupo.invitacion?.codigo && (
              <div className="grupo-codigo-wrap">
                <span className="grupo-codigo-label">Código</span>
                <div className="grupo-codigo-row">
                  <span className="grupo-codigo-valor">{grupo.invitacion.codigo}</span>
                  <button
                    className={`grupo-codigo-copiar ${copiado ? "copiado" : ""}`}
                    onClick={handleCopiar}
                  >
                    {copiado ? <><Check size={13} strokeWidth={2} /> Copiado</> : <><Copy size={13} strokeWidth={1.5} /> Copiar</>}
                  </button>
                </div>
              </div>
            )}
            {esMaestro && (
              <button className="btn-nueva-actividad" onClick={() => setModalActividad(true)}>
                + Actividad
              </button>
            )}
          </div>
        </div>

        {/* Tabs */}
        <div className="grupo-tabs-wrap">
          <div className="grupo-tabs">
            <button
              className={`grupo-tab ${tab === "actividades" ? "active" : ""}`}
              onClick={() => setTab("actividades")}
            >
              <ClipboardList size={14} strokeWidth={1.5} /> Actividades ({actividades.length})
            </button>
            <button
              className={`grupo-tab ${tab === "miembros" ? "active" : ""}`}
              onClick={() => setTab("miembros")}
            >
              <Users size={14} strokeWidth={1.5} /> Miembros ({estudiantes.length})
            </button>
            {!esMaestro && (
              <button
                className={`grupo-tab ${tab === "puntos" ? "active" : ""}`}
                onClick={() => setTab("puntos")}
              >
                <Trophy size={14} strokeWidth={1.5} /> Mis Puntos
              </button>
            )}
          </div>
        </div>

        {/* Tab: Actividades */}
        {tab === "actividades" && (
          <div className="grupo-actividades">
            {(() => {
              const actividadesVisibles = esMaestro
                ? actividades
                : actividades.filter(a => a.estatus === "Publicado");

              return actividadesVisibles.length === 0 ? (
              <div className="grupo-vacio">
                <p>No hay actividades asignadas a este grupo.</p>
                {esMaestro && (
                  <button className="btn-nueva-actividad" onClick={() => setModalActividad(true)}>
                    + Crear primera actividad
                  </button>
                )}
              </div>
            ) : (
              <>{actividadesVisibles.map(a => (
                <div key={a.idActividad} className="actividad-card"
                  onClick={() => navigate(`/cursos/${idCurso}/grupos/${idGrupo}/actividades/${a.idActividad}`)}
                >
                  <div className="actividad-card-header">
                    <div className="actividad-card-izq">
                      <div className="actividad-card-titulo-row">
                        <h3 className="actividad-card-titulo">{a.titulo}</h3>
                        {esMaestro && (
                          <span className={`actividad-estatus ${getEstatusColor(a.estatus)}`}>
                            {a.estatus}
                          </span>
                        )}
                      </div>
                      {a.descripcion && (
                        <p className="actividad-card-desc">{a.descripcion}</p>
                      )}
                      <div className="actividad-card-meta">
                        {a.nombreCategoria && (
                          <span className="actividad-meta-cat">
                            <Tag size={12} strokeWidth={1.5} /> {a.nombreCategoria} ({a.pesoCategoria}%)
                          </span>
                        )}
                        {a.fechaLimite && (
                          <span className="actividad-meta-fecha">
                            <Calendar size={12} strokeWidth={1.5} /> {formatFecha(a.fechaLimite)}
                          </span>
                        )}
                        {a.permiteEntregasTardias && (
                          <span className="actividad-meta-tardia">Acepta tardías</span>
                        )}
                      </div>
                    </div>
                    <div className="actividad-card-der">
                      <div className="actividad-puntos">
                        <span className="actividad-puntos-valor">{a.puntosMaximos}</span>
                        <span className="actividad-puntos-label">pts</span>
                      </div>
                      {a.puntosGamificacionMaximos > 0 && (
                        <div className="actividad-puntos-gam">
                          <span><Zap size={12} strokeWidth={1.5} /> {a.puntosGamificacionMaximos}</span>
                        </div>
                      )}
                      {/* Menú 3 puntos — solo maestro */}
                      {esMaestro && (
                        <div className="actividad-menu-wrap" onClick={e => e.stopPropagation()}>
                          <button
                            className="actividad-menu-btn"
                            onClick={() => setMenuAbierto(menuAbierto === a.idActividad ? null : a.idActividad)}
                          >
                            ⋮
                          </button>
                          {menuAbierto === a.idActividad && (
                            <div className="actividad-menu-dropdown">
                              {a.estatus === "Borrador" && (
                                <button className="actividad-menu-item" onClick={() => handleCambiarEstatus(a.idActividad, "Publicado")}>
                                  <Eye size={13} strokeWidth={1.5} /> Publicar
                                </button>
                              )}
                              {a.estatus === "Publicado" && (
                                <button className="actividad-menu-item" onClick={() => handleCambiarEstatus(a.idActividad, "Archivado")}>
                                  <Archive size={13} strokeWidth={1.5} /> Archivar
                                </button>
                              )}
                              {a.estatus === "Archivado" && (
                                <button className="actividad-menu-item" onClick={() => handleCambiarEstatus(a.idActividad, "Publicado")}>
                                  <RotateCcw size={13} strokeWidth={1.5} /> Restaurar
                                </button>
                              )}
                              <button
                                className="actividad-menu-item actividad-menu-item-danger"
                                onClick={() => handleEliminar(a.idActividad)}
                              >
                                <Trash2 size={13} strokeWidth={1.5} /> Eliminar
                              </button>
                            </div>
                          )}
                        </div>
                      )}
                    </div>
                  </div>
                </div>
              ))}</>
            );
            })()}
          </div>
        )}

        {/* Tab: Miembros */}
        {tab === "miembros" && (
          <div className="grupo-miembros-lista">
            {profesores.length > 0 && (
              <>
                <p className="grupo-miembros-seccion">Profesores</p>
                {profesores.map(m => (
                  <div key={m.idUsuario} className="grupo-miembro-row">
                    <div className="grupo-miembro-avatar">
                      {m.fotoPerfilUrl
                        ? <img src={m.fotoPerfilUrl} alt={m.nombreCompleto} />
                        : m.nombreCompleto.charAt(0).toUpperCase()}
                    </div>
                    <div className="grupo-miembro-info">
                      <p className="grupo-miembro-nombre">{m.nombreCompleto}</p>
                      <p className="grupo-miembro-email">{m.email}</p>
                    </div>
                    <span className="grupo-miembro-rol rol-prof">Profesor</span>
                  </div>
                ))}
              </>
            )}
            {estudiantes.length > 0 && (
              <>
                <p className="grupo-miembros-seccion" style={{ marginTop: "1rem" }}>
                  Estudiantes
                </p>
                {estudiantes.map(m => (
                  <div key={m.idUsuario} className="grupo-miembro-row">
                    <div className="grupo-miembro-avatar">
                      {m.fotoPerfilUrl
                        ? <img src={m.fotoPerfilUrl} alt={m.nombreCompleto} />
                        : m.nombreCompleto.charAt(0).toUpperCase()}
                    </div>
                    <div className="grupo-miembro-info">
                      <p className="grupo-miembro-nombre">{m.nombreCompleto}</p>
                      <p className="grupo-miembro-email">{m.email}</p>
                    </div>
                    <span className="grupo-miembro-rol rol-est">Estudiante</span>
                  </div>
                ))}
              </>
            )}
            {miembros.length === 0 && (
              <p style={{ color: "var(--text-muted)", textAlign: "center", padding: "2rem", fontSize: "0.85rem" }}>
                No hay miembros en este grupo todavía.
              </p>
            )}
          </div>
        )}
      </div>

        {/* Tab: Puntos (alumno) */}
        {tab === "puntos" && !esMaestro && (
          <div className="puntos-page">
            <div className="puntos-balance-card">
              <div className="puntos-balance-izq">
                <p className="puntos-balance-label">Mis puntos en este curso</p>
                <p className="puntos-balance-valor">
                  {balance ? balance.puntosActuales : "—"}
                  <span className="puntos-balance-unit">pts</span>
                </p>
                {balance && (
                  <div className="puntos-balance-stats">
                    <span>↑ {balance.puntosGanados} ganados</span>
                    <span>↓ {balance.puntosGastados} gastados</span>
                  </div>
                )}
              </div>
              <div className="puntos-balance-icon"></div>
            </div>

            <div className="puntos-columnas">
              <div className="puntos-seccion">
                <h3 className="puntos-seccion-titulo">Historial reciente</h3>
                {historial.length === 0 ? (
                  <p className="puntos-vacio">Sin transacciones todavía.</p>
                ) : (
                  <div className="puntos-historial">
                    {historial.map((t: any) => (
                      <div key={t.idTransaccion} className="transaccion-row">
                        <div className={`transaccion-icon ${t.cantidad > 0 ? "positivo" : "negativo"}`}>
                          {t.cantidad > 0 ? "+" : "−"}
                        </div>
                        <div className="transaccion-info">
                          <p className="transaccion-desc">{t.descripcion || t.origen}</p>
                          <p className="transaccion-fecha">
                            {new Date(t.fechaCreacion).toLocaleDateString("es-MX", {
                              day: "2-digit", month: "short", hour: "2-digit", minute: "2-digit"
                            })}
                          </p>
                        </div>
                        <span className={`transaccion-cantidad ${t.cantidad > 0 ? "positivo" : "negativo"}`}>
                          {t.cantidad > 0 ? "+" : ""}{t.cantidad}
                        </span>
                      </div>
                    ))}
                  </div>
                )}
              </div>

              <div className="puntos-seccion">
                <h3 className="puntos-seccion-titulo">Ranking del curso</h3>
                {ranking.length === 0 ? (
                  <p className="puntos-vacio">Sin datos de ranking.</p>
                ) : (
                  <div className="puntos-ranking">
                    {ranking.map((r: any, index: number) => (
                      <div key={r.idUsuario}
                        className={`ranking-row ${r.idUsuario === usuario.idUsuario ? "ranking-yo" : ""}`}
                      >
                        <span className="ranking-pos">
                          {index === 0 ? "" : index === 1 ? "" : index === 2 ? "" : `#${index + 1}`}
                        </span>
                        <div className="ranking-avatar">
                          {r.fotoPerfilUrl
                            ? <img src={r.fotoPerfilUrl} alt={r.nombreCompleto} />
                            : r.nombreCompleto?.charAt(0).toUpperCase()}
                        </div>
                        <p className="ranking-nombre">
                          {r.idUsuario === usuario.idUsuario ? "Tú" : r.nombreCompleto}
                        </p>
                        <span className="ranking-puntos">{r.puntosActuales} pts</span>
                      </div>
                    ))}
                  </div>
                )}
              </div>
            </div>
          </div>
        )}

      {/* Modal nueva actividad */}
      {modalActividad && (
        <div className="modal-overlay" onClick={() => setModalActividad(false)}>
          <div className="modal-card modal-actividad" onClick={e => e.stopPropagation()}>
            <h2 className="modal-titulo">Nueva actividad</h2>
            {errorActividad && <p className="modal-error">{errorActividad}</p>}

            <div className="modal-campo">
              <label>Título *</label>
              <input
                className="modal-input-texto"
                type="text"
                placeholder="Ej: Tarea 1 - Introducción"
                value={formActividad.titulo}
                onChange={e => setFormActividad({ ...formActividad, titulo: e.target.value })}
              />
            </div>

            <div className="modal-campo">
              <label>Descripción</label>
              <textarea
                className="modal-input-texto modal-textarea"
                placeholder="Instrucciones de la actividad..."
                value={formActividad.descripcion}
                onChange={e => setFormActividad({ ...formActividad, descripcion: e.target.value })}
                rows={3}
              />
            </div>

            <div className="modal-fila-dos">
              <div className="modal-campo">
                <label>Puntos máximos (1–100) *</label>
                <input
                  className="modal-input-texto"
                  type="number"
                  min={1} max={100}
                  value={formActividad.puntosMaximos}
                  onChange={e => setFormActividad({ ...formActividad, puntosMaximos: Number(e.target.value) })}
                />
              </div>
              <div className="modal-campo">
                <label>Puntos de gamificación</label>
                <input
                  className="modal-input-texto"
                  type="number"
                  min={0}
                  value={formActividad.puntosGamificacion}
                  onChange={e => setFormActividad({ ...formActividad, puntosGamificacion: Number(e.target.value) })}
                />
              </div>
            </div>

            <div className="modal-campo">
              <label>Fecha límite</label>
              <input
                className="modal-input-texto"
                type="datetime-local"
                value={formActividad.fechaLimite}
                onChange={e => setFormActividad({ ...formActividad, fechaLimite: e.target.value })}
              />
            </div>

            <div className="modal-fila-checks">
              <label className="modal-check-label">
                <input
                  type="checkbox"
                  checked={formActividad.permiteEntregasTardias}
                  onChange={e => setFormActividad({ ...formActividad, permiteEntregasTardias: e.target.checked })}
                />
                <span>Permitir entregas tardías</span>
              </label>
            </div>

            <div className="modal-campo">
              <label>Categoría <span style={{ color: "var(--text-muted)", fontWeight: 400 }}>(afecta calificación final)</span></label>
              {categorias.length === 0 ? (
                <p className="modal-sin-categorias">No hay categorías. Agrégalas desde el curso.</p>
              ) : (
                <select
                  className="modal-input-texto"
                  value={formActividad.idCategoria ?? ""}
                  onChange={e => setFormActividad({
                    ...formActividad,
                    idCategoria: e.target.value ? Number(e.target.value) : null
                  })}
                >
                  <option value="">Sin categoría</option>
                  {categorias.map(c => (
                    <option key={c.idCategoria} value={c.idCategoria}>
                      {c.nombre} — {c.peso}%
                    </option>
                  ))}
                </select>
              )}
            </div>

            <div className="modal-campo">
              <label>Asignar a</label>
              <div className="modal-estatus-opciones">
                <button
                  type="button"
                  className={`modal-estatus-btn ${formActividad.soloEsteGrupo ? "activo" : ""}`}
                  onClick={() => setFormActividad({ ...formActividad, soloEsteGrupo: true })}
                >
                  Solo {grupo.nombre}
                </button>
                <button
                  type="button"
                  className={`modal-estatus-btn ${!formActividad.soloEsteGrupo ? "activo" : ""}`}
                  onClick={() => setFormActividad({ ...formActividad, soloEsteGrupo: false })}
                >
                  Todos los grupos
                </button>
              </div>
            </div>

            <div className="modal-campo">
              <label>Publicar como</label>
              <div className="modal-estatus-opciones">
                {(["Borrador", "Publicado"] as const).map(op => (
                  <button
                    key={op}
                    type="button"
                    className={`modal-estatus-btn ${formActividad.estatus === op ? "activo" : ""}`}
                    onClick={() => setFormActividad({ ...formActividad, estatus: op })}
                  >
                    {op === "Borrador" ? " Borrador" : " Publicar ahora"}
                  </button>
                ))}
              </div>
            </div>

            <div className="modal-botones">
              <button
                className="modal-btn-cancelar"
                onClick={() => {
                  setModalActividad(false);
                  setErrorActividad("");
                  setFormActividad({
                    titulo: "", descripcion: "", puntosMaximos: 100,
                    puntosGamificacion: 0, fechaLimite: "",
                    permiteEntregasTardias: false, estatus: "Borrador",
                    idCategoria: null, soloEsteGrupo: true,
                  });
                }}
              >
                Cancelar
              </button>
              <button
                className="modal-btn-confirmar"
                onClick={handleCrearActividad}
                disabled={loadingActividad}
                style={{ opacity: loadingActividad ? 0.7 : 1 }}
              >
                {loadingActividad ? "Creando..." : formActividad.estatus === "Publicado" ? "Publicar" : "Guardar borrador"}
              </button>
            </div>
          </div>
        </div>
      )}
    </Layout>
  );
}