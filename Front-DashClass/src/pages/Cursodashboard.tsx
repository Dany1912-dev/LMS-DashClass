import "../styles/Cursodashboard.css";
import { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import Layout from "../components/Layout";
import { API } from "../api";
import { Users, Users2, GraduationCap, Pencil, Plus, Tag, Copy, Check, ArrowRight } from "lucide-react";
import banner1 from "../assets/banners/banner_1.jpg";
import banner2 from "../assets/banners/banner_2.jpg";
import banner3 from "../assets/banners/banner_3.jpg";
import banner4 from "../assets/banners/banner_4.jpeg";
import banner5 from "../assets/banners/banner_5.jpg";
import banner6 from "../assets/banners/banner_6.jpg";

const banners = [banner1, banner2, banner3, banner4, banner5, banner6];

interface Invitacion {
  codigo: string;
  token: string;
}

interface Grupo {
  idGrupo: number;
  nombre: string;
  descripcion: string;
  invitacion: Invitacion;
}

interface Miembro {
  idMiembroCurso: number;
  idUsuario: number;
  nombreCompleto: string;
  email: string;
  fotoPerfilUrl?: string;
  rol: string;
  nombreGrupo?: string | null;
  fechaInscripcion: string;
  estatus: boolean;
}

interface Curso {
  idCurso: number;
  nombre: string;
  descripcion: string;
  imagenBanner: string;
  idUsuario: number;
  nombreProfesor: string;
  grupos: Grupo[];
}

interface Categoria {
  idCategoria: number;
  nombre: string;
  peso: number;
  descripcion?: string;
  totalActividades: number;
}

export default function CursoDashboard() {
  const { idCurso } = useParams<{ idCurso: string }>();
  const navigate = useNavigate();
  const usuario  = JSON.parse(localStorage.getItem("usuario") || "{}");

  const [curso,      setCurso]      = useState<Curso | null>(null);
  const [miembros,   setMiembros]   = useState<Miembro[]>([]);
  const [categorias, setCategorias] = useState<Categoria[]>([]);
  const [loading,    setLoading]    = useState(true);
  const [tab,        setTab]        = useState<"grupos" | "miembros">("grupos");

  // Modal editar curso
  const [modalEditar,   setModalEditar]   = useState(false);
  const [errorEditar,   setErrorEditar]   = useState("");
  const [loadingEditar, setLoadingEditar] = useState(false);
  const [formEditar,    setFormEditar]    = useState({ nombre: "", descripcion: "", banner: banner1 });

  // Modal nuevo grupo
  const [modalGrupo,   setModalGrupo]   = useState(false);
  const [errorGrupo,   setErrorGrupo]   = useState("");
  const [loadingGrupo, setLoadingGrupo] = useState(false);
  const [formGrupo,    setFormGrupo]    = useState({ nombre: "", descripcion: "" });

  // Copiar código
  const [copiado, setCopiado] = useState<number | null>(null);

  // Modal nueva actividad
  const [modalActividad,      setModalActividad]      = useState(false);
  const [errorActividad,      setErrorActividad]      = useState("");
  const [loadingActividad,    setLoadingActividad]    = useState(false);
  const [asignarATodos,       setAsignarATodos]       = useState(true);
  const [gruposSeleccionados, setGruposSeleccionados] = useState<number[]>([]);
  const [formActividad, setFormActividad] = useState({
    titulo: "",
    descripcion: "",
    puntosMaximos: 100,
    puntosGamificacion: 0,
    fechaLimite: "",
    permiteEntregasTardias: false,
    estatus: "Borrador" as "Borrador" | "Publicado",
    idCategoria: null as number | null,
  });

  // Modal categorías
  const [modalCategorias,      setModalCategorias]      = useState(false);
  const [errorCategoria,       setErrorCategoria]       = useState("");
  const [loadingCategoria,     setLoadingCategoria]     = useState(false);
  const [formCategoria, setFormCategoria] = useState({ nombre: "", peso: "", descripcion: "" });

  const esMaestro = curso?.idUsuario === usuario.idUsuario;

  const handleCrearActividad = async () => {
    setErrorActividad("");
    if (!formActividad.titulo.trim()) {
      setErrorActividad("El título es obligatorio");
      return;
    }
    if (formActividad.puntosMaximos < 1 || formActividad.puntosMaximos > 100) {
      setErrorActividad("Los puntos máximos deben estar entre 1 y 100");
      return;
    }
    if (!asignarATodos && gruposSeleccionados.length === 0) {
      setErrorActividad("Selecciona al menos un grupo");
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
        idGrupos: asignarATodos ? [] : gruposSeleccionados,
      };
      const res = await fetch(`${import.meta.env.VITE_API_URL ?? "http://localhost:5000"}/api/actividades`, {
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
        idCategoria: null,
      });
      setAsignarATodos(true);
      setGruposSeleccionados([]);
    } catch (err: any) {
      setErrorActividad(err.message);
    } finally {
      setLoadingActividad(false);
    }
  };

  const handleCrearCategoria = async () => {
    setErrorCategoria("");
    if (!formCategoria.nombre.trim()) { setErrorCategoria("El nombre es obligatorio"); return; }
    const peso = Number(formCategoria.peso);
    if (!peso || peso <= 0 || peso > 100) { setErrorCategoria("El peso debe ser entre 1 y 100"); return; }
    setLoadingCategoria(true);
    try {
      const res = await fetch(API.crearCategoria, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${localStorage.getItem("token")}`,
        },
        body: JSON.stringify({
          idCurso: Number(idCurso),
          nombre: formCategoria.nombre.trim(),
          peso,
          descripcion: formCategoria.descripcion.trim() || null,
        }),
      });
      if (!res.ok) {
        const data = await res.json();
        throw new Error(data.message || "Error al crear categoría");
      }
      setFormCategoria({ nombre: "", peso: "", descripcion: "" });
      setErrorCategoria("");
      // Recargar solo categorías
      const resCats = await fetch(API.categoriasCurso(Number(idCurso)), {
        headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
      });
      const dataCats = await resCats.json();
      setCategorias(dataCats.categorias ?? []);
    } catch (err: any) {
      setErrorCategoria(err.message);
    } finally {
      setLoadingCategoria(false);
    }
  };

  const toggleGrupo = (idGrupo: number) => {
    setGruposSeleccionados(prev =>
      prev.includes(idGrupo) ? prev.filter(id => id !== idGrupo) : [...prev, idGrupo]
    );
  };

  const getBanner = (imagenBanner: string) => {
    const index = Number(imagenBanner);
    if (!isNaN(index) && index >= 1 && index <= banners.length) return banners[index - 1];
    const match = imagenBanner?.match(/banner_(\d+)\.(jpg|jpeg)/);
    if (match) {
      const i = Number(match[1]);
      if (i >= 1 && i <= banners.length) return banners[i - 1];
    }
    return null;
  };

  const cargarDatos = async () => {
    try {
      const [resCurso, resMiembros] = await Promise.all([
        fetch(API.cursoPorId(Number(idCurso)), {
          headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
        }),
        fetch(API.miembrosCurso(Number(idCurso)), {
          headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
        }),
      ]);
      const dataCurso    = await resCurso.json();
      const dataMiembros = await resMiembros.json();
      setCurso(dataCurso);
      setMiembros(dataMiembros.miembros ?? []);
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }

    // Categorías se cargan por separado — si fallan no rompen el curso
    try {
      const resCategorias = await fetch(API.categoriasCurso(Number(idCurso)), {
        headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
      });
      if (resCategorias.ok) {
        const dataCategorias = await resCategorias.json();
        setCategorias(dataCategorias.categorias ?? []);
      }
    } catch (err) {
      console.warn("No se pudieron cargar las categorías:", err);
    }
  };

  useEffect(() => { cargarDatos(); }, [idCurso]);

  const handleCopiar = (codigo: string, idGrupo: number) => {
    navigator.clipboard.writeText(codigo);
    setCopiado(idGrupo);
    setTimeout(() => setCopiado(null), 2000);
  };

  const abrirEditar = () => {
    if (!curso) return;
    const bannerIndex = Number(curso.imagenBanner);
    const bannerActual = !isNaN(bannerIndex) && bannerIndex >= 1 && bannerIndex <= banners.length
      ? banners[bannerIndex - 1]
      : banner1;
    setFormEditar({ nombre: curso.nombre, descripcion: curso.descripcion ?? "", banner: bannerActual });
    setModalEditar(true);
  };

  const handleEditarCurso = async () => {
    setErrorEditar("");
    if (!formEditar.nombre.trim()) { setErrorEditar("El nombre es obligatorio"); return; }
    setLoadingEditar(true);
    try {
      const res = await fetch(API.editarCurso(Number(idCurso)), {
        method: "PUT",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${localStorage.getItem("token")}`,
        },
        body: JSON.stringify({
          nombre: formEditar.nombre.trim(),
          descripcion: formEditar.descripcion.trim(),
          imagenBanner: String(banners.indexOf(formEditar.banner) + 1),
        }),
      });
      if (!res.ok) {
        const data = await res.json();
        throw new Error(data.message || "Error al editar");
      }
      setModalEditar(false);
      cargarDatos();
    } catch (err: any) {
      setErrorEditar(err.message);
    } finally {
      setLoadingEditar(false);
    }
  };

  const handleCrearGrupo = async () => {
    setErrorGrupo("");
    if (!formGrupo.nombre.trim()) { setErrorGrupo("El nombre del grupo es obligatorio"); return; }
    setLoadingGrupo(true);
    try {
      const res = await fetch(API.agregarGrupo(Number(idCurso)), {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${localStorage.getItem("token")}`,
        },
        body: JSON.stringify({ nombre: formGrupo.nombre.trim(), descripcion: formGrupo.descripcion.trim() }),
      });
      if (!res.ok) {
        const data = await res.json();
        throw new Error(data.message || "Error al crear grupo");
      }
      setModalGrupo(false);
      setFormGrupo({ nombre: "", descripcion: "" });
      cargarDatos();
    } catch (err: any) {
      setErrorGrupo(err.message);
    } finally {
      setLoadingGrupo(false);
    }
  };

  if (loading) {
    return (
      <Layout titulo="Curso">
        <p style={{ color: "var(--text-muted)", textAlign: "center", padding: "3rem" }}>Cargando...</p>
      </Layout>
    );
  }

  if (!curso) {
    return (
      <Layout titulo="Curso">
        <p style={{ color: "var(--text-muted)", textAlign: "center", padding: "3rem" }}>Curso no encontrado.</p>
      </Layout>
    );
  }

  // Si es alumno, redirigir automáticamente a su grupo
  const esAlumno = curso.idUsuario !== usuario.idUsuario;
  if (esAlumno) {
    const miembro = miembros.find(m => m.idUsuario === usuario.idUsuario);
    const grupoDelAlumno = curso.grupos.find(g => g.nombre === miembro?.nombreGrupo);
    if (grupoDelAlumno) {
      navigate(`/cursos/${idCurso}/grupos/${grupoDelAlumno.idGrupo}`, { replace: true });
      return null;
    }
  }

  const estudiantes = miembros.filter(m => m.rol === "Estudiante");
  const profesores  = miembros.filter(m => m.rol === "Profesor");

  return (
    <Layout titulo={curso.nombre}>
      <div className="curso-page">

        {/* Course header banner */}
        <div className="curso-hero">
          <div className="curso-hero-img">
            {getBanner(curso.imagenBanner)
              ? <img src={getBanner(curso.imagenBanner)!} alt="banner" />
              : <div className="curso-hero-placeholder"></div>}
          </div>
          <div className="curso-hero-overlay">
            <div className="curso-hero-info">
              <div className="curso-hero-meta">
                <span className={`curso-hero-badge ${esMaestro ? "badge-maestro" : "badge-alumno"}`}>
                  {esMaestro ? "Maestro" : "Alumno"}
                </span>
                <span className="curso-hero-grupos">{curso.grupos.length} grupo{curso.grupos.length !== 1 ? "s" : ""}</span>
              </div>
              <h1 className="curso-hero-nombre">{curso.nombre}</h1>
              {curso.descripcion && <p className="curso-hero-desc">{curso.descripcion}</p>}
              {!esMaestro && <p className="curso-hero-profesor">Prof. {curso.nombreProfesor}</p>}
            </div>
            {esMaestro && (
              <div className="curso-hero-acciones">
                <button className="btn-hero-secundario" onClick={abrirEditar}><Pencil size={14} strokeWidth={1.5} /> Editar</button>
                <button className="btn-hero-secundario" onClick={() => setModalGrupo(true)}><Plus size={14} strokeWidth={1.5} /> Grupo</button>
                <button className="btn-hero-secundario" onClick={() => setModalCategorias(true)}><Tag size={14} strokeWidth={1.5} /> Categorías</button>
                <button className="btn-hero-principal" onClick={() => setModalActividad(true)}><Plus size={14} strokeWidth={1.5} /> Actividad</button>
              </div>
            )}
          </div>
        </div>

        {/* Stats */}
        <div className="curso-stats">
          <div className="stat-card">
            <div className="stat-icon"><Users size={22} strokeWidth={1.5} /></div>
            <div className="stat-info">
              <p className="stat-label">Estudiantes</p>
              <p className="stat-value">{estudiantes.length}</p>
            </div>
          </div>
          <div className="stat-card">
            <div className="stat-icon"><Users2 size={22} strokeWidth={1.5} /></div>
            <div className="stat-info">
              <p className="stat-label">Grupos</p>
              <p className="stat-value">{curso.grupos.length}</p>
            </div>
          </div>
          <div className="stat-card">
            <div className="stat-icon"><GraduationCap size={22} strokeWidth={1.5} /></div>
            <div className="stat-info">
              <p className="stat-label">Profesores</p>
              <p className="stat-value">{profesores.length}</p>
            </div>
          </div>
        </div>

        {/* Tabs */}
        <div className="curso-tabs-wrap">
          <div className="curso-tabs">
            <button className={`curso-tab ${tab === "grupos" ? "active" : ""}`} onClick={() => setTab("grupos")}>
              <Users2 size={14} strokeWidth={1.5} /> Grupos
            </button>
            <button className={`curso-tab ${tab === "miembros" ? "active" : ""}`} onClick={() => setTab("miembros")}>
              <Users size={14} strokeWidth={1.5} /> Miembros ({miembros.length})
            </button>
          </div>
        </div>

        {/* Tab: Grupos */}
        {tab === "grupos" && (
          <div className="curso-grupos-grid">
            {curso.grupos.map((grupo) => (
              <div key={grupo.idGrupo} className="grupo-card">
                <div className="grupo-card-header">
                  <h3 className="grupo-card-nombre">{grupo.nombre}</h3>
                  {grupo.descripcion && <p className="grupo-card-desc">{grupo.descripcion}</p>}
                </div>
                {esMaestro && grupo.invitacion && (
                  <div className="grupo-card-codigo">
                    <p className="codigo-label">Código de invitación</p>
                    <div className="codigo-row">
                      <span className="codigo-valor">{grupo.invitacion.codigo}</span>
                      <button
                        className={`codigo-copiar ${copiado === grupo.idGrupo ? "copiado" : ""}`}
                        onClick={() => handleCopiar(grupo.invitacion.codigo, grupo.idGrupo)}
                      >
                        {copiado === grupo.idGrupo ? <><Check size={13} strokeWidth={2} /> Copiado</> : <><Copy size={13} strokeWidth={1.5} /> Copiar</>}
                      </button>
                    </div>
                  </div>
                )}
                <div className="grupo-card-footer">
                  <span className="grupo-miembros-count">
                    {miembros.filter(m => m.nombreGrupo === grupo.nombre && m.rol === "Estudiante").length} estudiantes
                  </span>
                  <button
                    className="grupo-card-entrar"
                    onClick={() => navigate(`/cursos/${idCurso}/grupos/${grupo.idGrupo}`)}
                  >
                    Entrar <ArrowRight size={14} strokeWidth={1.5} />
                  </button>
                </div>
              </div>
            ))}
          </div>
        )}

        {/* Tab: Miembros */}
        {tab === "miembros" && (
          <div className="curso-miembros">
            {profesores.length > 0 && (
              <>
                <p className="miembros-seccion-titulo">Profesores</p>
                {profesores.map(m => (
                  <div key={m.idUsuario} className="miembro-row">
                    <div className="miembro-avatar">
                      {m.fotoPerfilUrl
                        ? <img src={m.fotoPerfilUrl} alt={m.nombreCompleto} />
                        : m.nombreCompleto.charAt(0).toUpperCase()}
                    </div>
                    <div className="miembro-info">
                      <p className="miembro-nombre">{m.nombreCompleto}</p>
                      {m.nombreGrupo && <p className="miembro-grupo">{m.nombreGrupo}</p>}
                    </div>
                    <span className="miembro-rol rol-prof">Profesor</span>
                  </div>
                ))}
              </>
            )}
            {estudiantes.length > 0 && (
              <>
                <p className="miembros-seccion-titulo" style={{ marginTop: "1rem" }}>Estudiantes</p>
                {estudiantes.map(m => (
                  <div key={m.idUsuario} className="miembro-row">
                    <div className="miembro-avatar">
                      {m.fotoPerfilUrl
                        ? <img src={m.fotoPerfilUrl} alt={m.nombreCompleto} />
                        : m.nombreCompleto.charAt(0).toUpperCase()}
                    </div>
                    <div className="miembro-info">
                      <p className="miembro-nombre">{m.nombreCompleto}</p>
                      {m.nombreGrupo && <p className="miembro-grupo">{m.nombreGrupo}</p>}
                    </div>
                    <span className="miembro-rol rol-est">Estudiante</span>
                  </div>
                ))}
              </>
            )}
            {miembros.length === 0 && (
              <p style={{ color: "var(--text-muted)", fontSize: "0.85rem", textAlign: "center", padding: "2rem" }}>
                No hay miembros todavía.
              </p>
            )}
          </div>
        )}
      </div>

      {/* Modal editar curso */}
      {modalEditar && (
        <div className="modal-overlay" onClick={() => setModalEditar(false)}>
          <div className="modal-card" onClick={e => e.stopPropagation()}>
            <h2 className="modal-titulo">Editar curso</h2>
            {errorEditar && <p className="modal-error">{errorEditar}</p>}
            <div className="modal-campo">
              <label>Nombre del curso</label>
              <input className="modal-input-texto" type="text" value={formEditar.nombre} onChange={e => setFormEditar({ ...formEditar, nombre: e.target.value })} />
            </div>
            <div className="modal-campo">
              <label>Descripción</label>
              <input className="modal-input-texto" type="text" value={formEditar.descripcion} onChange={e => setFormEditar({ ...formEditar, descripcion: e.target.value })} />
            </div>
            <div className="modal-campo">
              <label>Banner</label>
              <div className="banner-grid">
                {banners.map((banner, i) => (
                  <img key={i} src={banner} alt={`banner ${i+1}`} draggable="false"
                    className={`banner-opcion ${formEditar.banner === banner ? "banner-seleccionado" : ""}`}
                    onClick={() => setFormEditar({ ...formEditar, banner })}
                  />
                ))}
              </div>
            </div>
            <div className="modal-botones">
              <button className="modal-btn-cancelar" onClick={() => { setModalEditar(false); setErrorEditar(""); }}>Cancelar</button>
              <button className="modal-btn-confirmar" onClick={handleEditarCurso} disabled={loadingEditar} style={{ opacity: loadingEditar ? 0.7 : 1 }}>
                {loadingEditar ? "Guardando..." : "Guardar"}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Modal nuevo grupo */}
      {modalGrupo && (
        <div className="modal-overlay" onClick={() => setModalGrupo(false)}>
          <div className="modal-card" onClick={e => e.stopPropagation()}>
            <h2 className="modal-titulo">Agregar grupo</h2>
            {errorGrupo && <p className="modal-error">{errorGrupo}</p>}
            <div className="modal-campo">
              <label>Nombre del grupo</label>
              <input className="modal-input-texto" type="text" placeholder="Ej: Grupo B" value={formGrupo.nombre} onChange={e => setFormGrupo({ ...formGrupo, nombre: e.target.value })} />
            </div>
            <div className="modal-campo">
              <label>Descripción</label>
              <input className="modal-input-texto" type="text" placeholder="Ej: Turno vespertino" value={formGrupo.descripcion} onChange={e => setFormGrupo({ ...formGrupo, descripcion: e.target.value })} />
            </div>
            <div className="modal-botones">
              <button className="modal-btn-cancelar" onClick={() => { setModalGrupo(false); setErrorGrupo(""); setFormGrupo({ nombre: "", descripcion: "" }); }}>Cancelar</button>
              <button className="modal-btn-confirmar" onClick={handleCrearGrupo} disabled={loadingGrupo} style={{ opacity: loadingGrupo ? 0.7 : 1 }}>
                {loadingGrupo ? "Creando..." : "Crear grupo"}
              </button>
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

            <div className="modal-campo">
              <label>Categoría <span style={{color:"var(--text-muted)", fontWeight:400}}>(afecta calificación final)</span></label>
              {categorias.length === 0 ? (
                <p className="modal-sin-categorias">
                  No hay categorías configuradas.{" "}
                  <button
                    type="button"
                    className="modal-link-btn"
                    onClick={() => { setModalActividad(false); setModalCategorias(true); }}
                  >
                    Crear una
                  </button>
                </p>
              ) : (
                <select
                  className="modal-input-texto"
                  value={formActividad.idCategoria ?? ""}
                  onChange={e => setFormActividad({
                    ...formActividad,
                    idCategoria: e.target.value ? Number(e.target.value) : null
                  })}
                >
                  <option value="">Sin categoría (no afecta calificación)</option>
                  {categorias.map(c => (
                    <option key={c.idCategoria} value={c.idCategoria}>
                      {c.nombre} — {c.peso}%
                    </option>
                  ))}
                </select>
              )}
            </div>

            {/* Asignación de grupos */}
            <div className="modal-campo">
              <label>Asignar a</label>
              <div className="modal-grupos-opciones">
                <button
                  type="button"
                  className={`modal-estatus-btn ${asignarATodos ? "activo" : ""}`}
                  onClick={() => { setAsignarATodos(true); setGruposSeleccionados([]); }}
                >
                  Todos los grupos
                </button>
                <button
                  type="button"
                  className={`modal-estatus-btn ${!asignarATodos ? "activo" : ""}`}
                  onClick={() => setAsignarATodos(false)}
                >
                  Grupos específicos
                </button>
              </div>

              {!asignarATodos && (
                <div className="modal-grupos-lista">
                  {curso.grupos.map(g => (
                    <label key={g.idGrupo} className="modal-check-label">
                      <input
                        type="checkbox"
                        checked={gruposSeleccionados.includes(g.idGrupo)}
                        onChange={() => toggleGrupo(g.idGrupo)}
                      />
                      <span>{g.nombre}</span>
                    </label>
                  ))}
                </div>
              )}
            </div>

            <div className="modal-botones">
              <button
                className="modal-btn-cancelar"
                onClick={() => {
                  setModalActividad(false);
                  setErrorActividad("");
                  setFormActividad({ titulo: "", descripcion: "", puntosMaximos: 100, puntosGamificacion: 0, fechaLimite: "", permiteEntregasTardias: false, estatus: "Borrador", idCategoria: null });
                  setAsignarATodos(true);
                  setGruposSeleccionados([]);
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
      {/* Modal categorías */}
      {modalCategorias && (
        <div className="modal-overlay" onClick={() => setModalCategorias(false)}>
          <div className="modal-card modal-actividad" onClick={e => e.stopPropagation()}>
            <h2 className="modal-titulo">Categorías de actividad</h2>
            <p className="modal-descripcion">
              Define las categorías y su peso porcentual. El total debe sumar 100%.
            </p>

            {/* Lista de categorías existentes */}
            {categorias.length > 0 && (
              <div className="categorias-lista">
                {categorias.map(c => (
                  <div key={c.idCategoria} className="categoria-item">
                    <div className="categoria-item-info">
                      <span className="categoria-item-nombre">{c.nombre}</span>
                      {c.descripcion && <span className="categoria-item-desc">{c.descripcion}</span>}
                    </div>
                    <div className="categoria-item-derecha">
                      <span className="categoria-item-peso">{c.peso}%</span>
                      <span className="categoria-item-actividades">{c.totalActividades} actividades</span>
                    </div>
                  </div>
                ))}
                <div className="categorias-peso-total">
                  <span>Peso total configurado:</span>
                  <span className={categorias.reduce((s, c) => s + c.peso, 0) === 100 ? "peso-completo" : "peso-incompleto"}>
                    {categorias.reduce((s, c) => s + c.peso, 0)}%
                  </span>
                </div>
              </div>
            )}

            {/* Formulario nueva categoría */}
            <div className="categorias-nueva-titulo">Nueva categoría</div>
            {errorCategoria && <p className="modal-error">{errorCategoria}</p>}

            <div className="modal-fila-dos">
              <div className="modal-campo">
                <label>Nombre *</label>
                <input
                  className="modal-input-texto"
                  type="text"
                  placeholder="Ej: Tareas, Exámenes"
                  value={formCategoria.nombre}
                  onChange={e => setFormCategoria({ ...formCategoria, nombre: e.target.value })}
                />
              </div>
              <div className="modal-campo">
                <label>Peso (%) *</label>
                <input
                  className="modal-input-texto"
                  type="number"
                  min={1} max={100}
                  placeholder="Ej: 30"
                  value={formCategoria.peso}
                  onChange={e => setFormCategoria({ ...formCategoria, peso: e.target.value })}
                />
              </div>
            </div>
            <div className="modal-campo">
              <label>Descripción</label>
              <input
                className="modal-input-texto"
                type="text"
                placeholder="Ej: Tareas semanales"
                value={formCategoria.descripcion}
                onChange={e => setFormCategoria({ ...formCategoria, descripcion: e.target.value })}
              />
            </div>

            <div className="modal-botones">
              <button className="modal-btn-cancelar" onClick={() => { setModalCategorias(false); setErrorCategoria(""); setFormCategoria({ nombre: "", peso: "", descripcion: "" }); }}>
                Cerrar
              </button>
              <button className="modal-btn-confirmar" onClick={handleCrearCategoria} disabled={loadingCategoria} style={{ opacity: loadingCategoria ? 0.7 : 1 }}>
                {loadingCategoria ? "Guardando..." : "Agregar categoría"}
              </button>
            </div>
          </div>
        </div>
      )}
    </Layout>
  );
}