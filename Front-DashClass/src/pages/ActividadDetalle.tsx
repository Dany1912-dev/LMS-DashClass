import "../styles/ActividadDetalle.css";
import { useState, useEffect, useRef } from "react";
import { useParams, useNavigate } from "react-router-dom";
import Layout from "../components/Layout";
import { API } from "../api";
import { Pencil, Paperclip, Upload, CheckCircle, X, Calendar, Tag, Clock, Zap, FileText, Eye, Archive, ChevronLeft, Award } from "lucide-react";

interface Actividad {
  idActividad: number;
  idCurso: number;
  idUsuario: number;
  titulo: string;
  descripcion?: string;
  puntosMaximos: number;
  puntosGamificacionMaximos: number;
  fechaLimite?: string;
  permiteEntregasTardias: boolean;
  estatus: string;
  fechaCreacion: string;
  fechaPublicacion?: string;
  nombreProfesor: string;
  nombreCategoria?: string;
  pesoCategoria?: number;
  idCategoria?: number;
  esParaTodos: boolean;
}

interface Categoria {
  idCategoria: number;
  nombre: string;
  peso: number;
}

const BASE = import.meta.env.VITE_API_URL ?? "http://localhost:5000";

export default function ActividadDetalle() {
  const { idCurso, idGrupo, idActividad } = useParams<{
    idCurso: string; idGrupo: string; idActividad: string;
  }>();
  const navigate = useNavigate();
  const usuario  = JSON.parse(localStorage.getItem("usuario") || "{}");
  const fileInputRef = useRef<HTMLInputElement>(null);

  const [actividad,    setActividad]    = useState<Actividad | null>(null);
  const [categorias,   setCategorias]   = useState<Categoria[]>([]);
  const [loading,      setLoading]      = useState(true);
  const [esMaestro,    setEsMaestro]    = useState(false);
  const [tabMaestro,   setTabMaestro]   = useState<"detalle" | "entregas">("detalle");

  // Edición
  const [editando,    setEditando]    = useState(false);
  const [errorEdit,   setErrorEdit]   = useState("");
  const [loadingEdit, setLoadingEdit] = useState(false);
  const [formEdit,    setFormEdit]    = useState({
    titulo: "", descripcion: "", puntosMaximos: 100,
    puntosGamificacion: 0, fechaLimite: "",
    permiteEntregasTardias: false, estatus: "Borrador" as string,
    idCategoria: null as number | null,
  });

  // Entregas (maestro)
  const [entregas,             setEntregas]             = useState<any[]>([]);
  const [entregaSeleccionada,  setEntregaSeleccionada]  = useState<any>(null);
  const [modalCalificar,       setModalCalificar]       = useState(false);
  const [formCalificar,        setFormCalificar]        = useState({ puntuacion: "", retroalimentacion: "" });
  const [loadingCalificar,     setLoadingCalificar]     = useState(false);
  const [errorCalificar,       setErrorCalificar]       = useState("");

  // Entrega (alumno)
  const [comentario,      setComentario]      = useState("");
  const [archivos,        setArchivos]        = useState<File[]>([]);
  const [loadingEntrega,  setLoadingEntrega]  = useState(false);
  const [errorEntrega,    setErrorEntrega]    = useState("");
  const [entregaActual,   setEntregaActual]   = useState<any>(null);
  const [entregaExitosa,  setEntregaExitosa]  = useState(false);

  //  Carga de datos 
  const cargarDatos = async () => {
    try {
      const [resAct, resCurso] = await Promise.all([
        fetch(`${BASE}/api/actividades/${idActividad}`, {
          headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
        }),
        fetch(API.cursoPorId(Number(idCurso)), {
          headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
        }),
      ]);
      const dataAct   = await resAct.json();
      const dataCurso = await resCurso.json();

      setActividad(dataAct);
      const esMaestroActual = dataCurso.idUsuario === usuario.idUsuario;
      console.log("dataCurso.idUsuario:", dataCurso.idUsuario);
      console.log("usuario.idUsuario:", usuario.idUsuario);
      console.log("esMaestro:", esMaestroActual);
      setEsMaestro(esMaestroActual);

      setFormEdit({
        titulo: dataAct.titulo,
        descripcion: dataAct.descripcion ?? "",
        puntosMaximos: dataAct.puntosMaximos,
        puntosGamificacion: dataAct.puntosGamificacionMaximos,
        fechaLimite: dataAct.fechaLimite
          ? new Date(dataAct.fechaLimite).toISOString().slice(0, 16)
          : "",
        permiteEntregasTardias: dataAct.permiteEntregasTardias,
        estatus: dataAct.estatus,
        idCategoria: dataAct.idCategoria ?? null,
      });

      if (esMaestroActual) {
        try {
          const resEnt = await fetch(`${BASE}/api/entregas/actividad/${idActividad}`, {
            headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
          });
          if (resEnt.ok) {
            const d = await resEnt.json();
            setEntregas(d.entregas ?? []);
          }
        } catch (err) { console.warn(err); }
      } else {
        console.log("Entrando al bloque alumno");
        try {
          const resEnt = await fetch(
            `${BASE}/api/entregas/actividad/${idActividad}/estudiante/${usuario.idUsuario}`,
            { headers: { Authorization: `Bearer ${localStorage.getItem("token")}` } }
          );
          console.log("Status entrega:", resEnt.status);
          if (resEnt.ok) {
            const d = await resEnt.json();
            setEntregaActual(d);
            setEntregaExitosa(true);
          }
        } catch (err) { console.warn(err); }
      }
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }

    try {
      const resCats = await fetch(API.categoriasCurso(Number(idCurso)), {
        headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
      });
      if (resCats.ok) {
        const d = await resCats.json();
        setCategorias(d.categorias ?? []);
      }
    } catch (err) { console.warn(err); }
  };

  useEffect(() => { cargarDatos(); }, [idActividad]);

  //  Editar 
  const handleGuardarEdicion = async () => {
    setErrorEdit("");
    if (!formEdit.titulo.trim()) { setErrorEdit("El título es obligatorio"); return; }
    if (formEdit.puntosMaximos < 1 || formEdit.puntosMaximos > 100) {
      setErrorEdit("Los puntos máximos deben estar entre 1 y 100"); return;
    }
    setLoadingEdit(true);
    try {
      const res = await fetch(`${BASE}/api/actividades/${idActividad}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json", Authorization: `Bearer ${localStorage.getItem("token")}` },
        body: JSON.stringify({
          idCategoria: formEdit.idCategoria,
          titulo: formEdit.titulo.trim(),
          descripcion: formEdit.descripcion.trim() || null,
          puntosMaximos: formEdit.puntosMaximos,
          puntosGamificacionMaximos: formEdit.puntosGamificacion,
          fechaLimite: formEdit.fechaLimite || null,
          permiteEntregasTardias: formEdit.permiteEntregasTardias,
          estatus: formEdit.estatus,
        }),
      });
      if (!res.ok) { const d = await res.json(); throw new Error(d.message || "Error"); }
      setEditando(false);
      cargarDatos();
    } catch (err: any) { setErrorEdit(err.message); }
    finally { setLoadingEdit(false); }
  };

  //  Calificar 
  const abrirCalificar = (entrega: any) => {
    setEntregaSeleccionada(entrega);
    setFormCalificar({
      puntuacion: entrega.calificacion?.puntuacion?.toString() ?? "",
      retroalimentacion: entrega.calificacion?.retroalimentacion ?? "",
    });
    setErrorCalificar("");
    setModalCalificar(true);
  };

  const handleCalificar = async () => {
    setErrorCalificar("");
    const puntuacion = Number(formCalificar.puntuacion);
    if (isNaN(puntuacion) || puntuacion < 0 || puntuacion > (actividad?.puntosMaximos ?? 100)) {
      setErrorCalificar(`La puntuación debe estar entre 0 y ${actividad?.puntosMaximos}`); return;
    }
    setLoadingCalificar(true);
    try {
      const res = await fetch(`${BASE}/api/entregas/${entregaSeleccionada.idEntrega}/calificar`, {
        method: "POST",
        headers: { "Content-Type": "application/json", Authorization: `Bearer ${localStorage.getItem("token")}` },
        body: JSON.stringify({
          idUsuarioProfesor: usuario.idUsuario,
          puntuacion,
          retroalimentacion: formCalificar.retroalimentacion.trim() || null,
        }),
      });
      if (!res.ok) { const d = await res.json(); throw new Error(d.message || "Error al calificar"); }
      setModalCalificar(false);
      cargarDatos();
    } catch (err: any) { setErrorCalificar(err.message); }
    finally { setLoadingCalificar(false); }
  };

  //  Entrega alumno 
  const handleMarcarEntregada = async () => {
    setErrorEntrega("");
    setLoadingEntrega(true);
    try {
      const res = await fetch(`${BASE}/api/entregas/marcar`, {
        method: "POST",
        headers: { "Content-Type": "application/json", Authorization: `Bearer ${localStorage.getItem("token")}` },
        body: JSON.stringify({
          idActividad: Number(idActividad),
          idUsuario: usuario.idUsuario,
          comentarios: comentario.trim() || null,
        }),
      });
      if (!res.ok) { const d = await res.json(); throw new Error(d.message || "Error"); }
      const d = await res.json();
      setEntregaActual(d.data);
      setEntregaExitosa(true);
    } catch (err: any) { setErrorEntrega(err.message); }
    finally { setLoadingEntrega(false); }
  };

  const handleSubirEntrega = async () => {
    setErrorEntrega("");
    if (archivos.length === 0) { setErrorEntrega("Selecciona al menos un archivo"); return; }
    setLoadingEntrega(true);
    try {
      const formData = new FormData();
      formData.append("idActividad", String(idActividad));
      formData.append("idUsuario", String(usuario.idUsuario));
      if (comentario.trim()) formData.append("comentarios", comentario.trim());
      archivos.forEach(f => formData.append("archivos", f));

      const res = await fetch(`${BASE}/api/entregas/subir`, {
        method: "POST",
        headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
        body: formData,
      });
      if (!res.ok) { const d = await res.json(); throw new Error(d.message || "Error"); }
      const d = await res.json();
      setEntregaActual(d.data);
      setEntregaExitosa(true);
      setArchivos([]);
    } catch (err: any) { setErrorEntrega(err.message); }
    finally { setLoadingEntrega(false); }
  };

  const handleAgregarArchivos = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (!e.target.files) return;
    console.log("Archivos seleccionados:", e.target.files.length);
    setArchivos(prev => [...prev, ...Array.from(e.target.files!)]);
    //e.target.value = "";
  };

  const handleQuitarArchivo = (i: number) =>
    setArchivos(prev => prev.filter((_, idx) => idx !== i));

  //  Helpers 
  const formatFecha = (fecha?: string) => {
    if (!fecha) return "Sin fecha límite";
    return new Date(fecha).toLocaleDateString("es-MX", {
      weekday: "long", day: "2-digit", month: "long",
      year: "numeric", hour: "2-digit", minute: "2-digit",
    });
  };

  const formatFechaCorta = (fecha?: string) => {
    if (!fecha) return "";
    return new Date(fecha).toLocaleDateString("es-MX", {
      day: "2-digit", month: "short", hour: "2-digit", minute: "2-digit",
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

  const estaVencida = actividad?.fechaLimite
    ? new Date(actividad.fechaLimite) < new Date()
    : false;

  if (loading) return (
    <Layout titulo="Actividad">
      <p style={{ color: "var(--text-muted)", textAlign: "center", padding: "3rem" }}>Cargando...</p>
    </Layout>
  );

  if (!actividad) return (
    <Layout titulo="Actividad">
      <p style={{ color: "var(--text-muted)", textAlign: "center", padding: "3rem" }}>Actividad no encontrada.</p>
    </Layout>
  );

  return (
    <Layout titulo={actividad.titulo}>
      <div className="detalle-page">

        {/* Breadcrumb */}
        <div className="detalle-breadcrumb">
          <button className="breadcrumb-btn" onClick={() => navigate(`/cursos/${idCurso}`)}><ChevronLeft size={14} strokeWidth={1.5} /> Curso</button>
          <span className="breadcrumb-sep">/</span>
          <button className="breadcrumb-btn" onClick={() => navigate(`/cursos/${idCurso}/grupos/${idGrupo}`)}><ChevronLeft size={14} strokeWidth={1.5} /> Grupo</button>
          <span className="breadcrumb-sep">/</span>
          <span className="breadcrumb-actual">{actividad.titulo}</span>
        </div>

        <div className="detalle-layout">

          {/*  MAIN  */}
          <div className="detalle-main">

            {/* Tabs maestro */}
            {esMaestro && (
              <div className="detalle-tabs-wrap">
                <div className="detalle-tabs">
                  <button className={`detalle-tab ${tabMaestro === "detalle" ? "active" : ""}`} onClick={() => setTabMaestro("detalle")}>
                    Actividad
                  </button>
                  <button className={`detalle-tab ${tabMaestro === "entregas" ? "active" : ""}`} onClick={() => setTabMaestro("entregas")}>
                    Entregas ({entregas.length})
                  </button>
                </div>
              </div>
            )}

            {/*  TAB ENTREGAS (maestro)  */}
            {esMaestro && tabMaestro === "entregas" && (
              <div className="entregas-lista">
                {entregas.length === 0 ? (
                  <div className="entregas-vacio">
                    <p>Ningún estudiante ha entregado todavía.</p>
                  </div>
                ) : (
                  entregas.map(e => (
                    <div key={e.idEntrega} className="entrega-row">
                      <div className="entrega-row-avatar">
                        {e.fotoPerfilUrl
                          ? <img src={e.fotoPerfilUrl} alt={e.nombreEstudiante} />
                          : e.nombreEstudiante.charAt(0).toUpperCase()}
                      </div>
                      <div className="entrega-row-info">
                        <p className="entrega-row-nombre">{e.nombreEstudiante}</p>
                        <div className="entrega-row-meta">
                          <span className={`entrega-row-estado ${e.estado === "Calificada" ? "estado-calificada" : "estado-entregada"}`}>
                            {e.estado}
                          </span>
                          {e.esTardia && <span className="entrega-tardia-badge">Tardía</span>}
                          <span className="entrega-row-fecha">{formatFechaCorta(e.fechaEntrega)}</span>
                        </div>
                        {e.recursos?.length > 0 && (
                          <div className="entrega-row-recursos">
                            {e.recursos.map((r: any) => (
                              <a key={r.idRecurso} href={`${BASE}${r.urlArchivo}`}
                                target="_blank" rel="noreferrer" className="recurso-link-mini">
                                <FileText size={12} strokeWidth={1.5} /> {r.nombre}
                              </a>
                            ))}
                          </div>
                        )}
                        {e.comentarios && (
                          <p className="entrega-row-comentario">"{e.comentarios}"</p>
                        )}
                        {e.calificacion && (
                          <p className="entrega-row-retro">
                             {e.calificacion.retroalimentacion || "Sin retroalimentación"}
                          </p>
                        )}
                      </div>
                      <div className="entrega-row-der">
                        {e.calificacion ? (
                          <div className="calificacion-badge" onClick={() => abrirCalificar(e)}>
                            <span className="calificacion-badge-puntos">
                              {e.calificacion.puntuacion}/{actividad.puntosMaximos}
                            </span>
                            <span className="calificacion-badge-edit">Editar</span>
                          </div>
                        ) : (
                          <button className="btn-calificar" onClick={() => abrirCalificar(e)}>
                            Calificar
                          </button>
                        )}
                      </div>
                    </div>
                  ))
                )}
              </div>
            )}

            {/*  TAB DETALLE (ambos)  */}
            {(!esMaestro || tabMaestro === "detalle") && (
              <>
                {!editando ? (
                  <div className="detalle-card">
                    <div className="detalle-card-header">
                      <div className="detalle-titulo-row">
                        <h1 className="detalle-titulo">{actividad.titulo}</h1>
                        {esMaestro && (
                          <span className={`actividad-estatus ${getEstatusColor(actividad.estatus)}`}>
                            {actividad.estatus}
                          </span>
                        )}
                      </div>
                      <p className="detalle-meta">
                        Por {actividad.nombreProfesor} · {formatFechaCorta(actividad.fechaCreacion)}
                      </p>
                    </div>

                    {actividad.descripcion ? (
                      <div className="detalle-descripcion"><p>{actividad.descripcion}</p></div>
                    ) : (
                      <p className="detalle-sin-desc">Sin descripción.</p>
                    )}

                    {esMaestro && (
                      <div className="detalle-acciones-maestro">
                        <button className="btn-editar-actividad" onClick={() => setEditando(true)}>
                          <Pencil size={14} strokeWidth={1.5} /> Editar actividad
                        </button>
                      </div>
                    )}

                    {/* Entrega alumno */}
                    {!esMaestro && (
                      <div className="detalle-entrega">
                        <h3 className="detalle-entrega-titulo">Tu entrega</h3>

                        {entregaExitosa && entregaActual && (
                          <div className="entrega-actual">
                            <div className="entrega-actual-header">
                              <span className="entrega-actual-badge">
                                {entregaActual.estado === "Calificada"
                                  ? <><Award size={13} strokeWidth={1.5} /> Calificada</>
                                  : <><CheckCircle size={13} strokeWidth={1.5} /> Entregada</>}
                              </span>
                              {entregaActual.esTardia && <span className="entrega-tardia-badge">Tardía</span>}
                              <span className="entrega-actual-fecha">
                                v{entregaActual.version} · {formatFechaCorta(entregaActual.fechaEntrega)}
                              </span>
                            </div>
                            {entregaActual.calificacion && (
                              <div className="entrega-calificacion">
                                <span className="calificacion-puntos">
                                  {entregaActual.calificacion.puntuacion}/{actividad.puntosMaximos}
                                </span>
                                {entregaActual.calificacion.retroalimentacion && (
                                  <p className="calificacion-retro">{entregaActual.calificacion.retroalimentacion}</p>
                                )}
                              </div>
                            )}
                            {entregaActual.recursos?.length > 0 && (
                              <div className="entrega-recursos">
                                {entregaActual.recursos.map((r: any) => (
                                  <a key={r.idRecurso} href={`${BASE}${r.urlArchivo}`}
                                    target="_blank" rel="noreferrer" className="recurso-link">
                                    <FileText size={13} strokeWidth={1.5} /> {r.nombre}
                                  </a>
                                ))}
                              </div>
                            )}
                            <p className="entrega-reentregar-aviso">¿Quieres reemplazar tu entrega?</p>
                          </div>
                        )}

                        {estaVencida && !actividad.permiteEntregasTardias ? (
                          <div className="entrega-vencida">
                            <p>⏰ Esta actividad ya venció y no acepta entregas tardías.</p>
                          </div>
                        ) : (
                          <>
                            {estaVencida && (
                              <p className="entrega-tardia-aviso">
                                 La fecha límite ya pasó. Esta entrega se marcará como tardía.
                              </p>
                            )}
                            {errorEntrega && <p className="modal-error">{errorEntrega}</p>}
                            <div className="modal-campo">
                              <label>Comentarios (opcional)</label>
                              <textarea
                                className="modal-input-texto modal-textarea"
                                placeholder="Agrega comentarios para tu profesor..."
                                value={comentario}
                                onChange={e => setComentario(e.target.value)}
                                rows={3}
                              />
                            </div>
                            {archivos.length > 0 && (
                              <div className="archivos-lista">
                                {archivos.map((f, i) => (
                                  <div key={i} className="archivo-item">
                                    <span className="archivo-nombre"> {f.name}</span>
                                    <span className="archivo-size">{(f.size / 1024).toFixed(0)} KB</span>
                                    <button className="archivo-quitar" onClick={() => handleQuitarArchivo(i)}><X size={13} strokeWidth={2} /></button>
                                  </div>
                                ))}
                              </div>
                            )}
                            <input ref={fileInputRef} type="file" multiple style={{ display: "none" }} onChange={handleAgregarArchivos} />
                            <div className="entrega-botones">
                              <button className="btn-agregar-archivos" onClick={() => fileInputRef.current?.click()}>
                                <Paperclip size={14} strokeWidth={1.5} /> Agregar archivos
                              </button>
                              <button
                                className="btn-entregar-archivos"
                                onClick={handleSubirEntrega}
                                disabled={loadingEntrega || archivos.length === 0}
                                style={{ opacity: (loadingEntrega || archivos.length === 0) ? 0.6 : 1 }}
                              >
                                {loadingEntrega ? "Subiendo..." : <><Upload size={14} strokeWidth={1.5} /> Entregar</>}
                              </button>
                              <button
                                className="btn-marcar-entregada"
                                onClick={handleMarcarEntregada}
                                disabled={loadingEntrega}
                                style={{ opacity: loadingEntrega ? 0.6 : 1 }}
                              >
                                <CheckCircle size={14} strokeWidth={1.5} /> Marcar como entregada
                              </button>
                            </div>
                          </>
                        )}
                      </div>
                    )}
                  </div>
                ) : (
                  /* Formulario edición */
                  <div className="detalle-card">
                    <h2 className="detalle-edit-titulo">Editar actividad</h2>
                    {errorEdit && <p className="modal-error">{errorEdit}</p>}
                    <div className="modal-campo">
                      <label>Título *</label>
                      <input className="modal-input-texto" type="text" value={formEdit.titulo}
                        onChange={e => setFormEdit({ ...formEdit, titulo: e.target.value })} />
                    </div>
                    <div className="modal-campo">
                      <label>Descripción</label>
                      <textarea className="modal-input-texto modal-textarea" rows={4}
                        placeholder="Instrucciones..." value={formEdit.descripcion}
                        onChange={e => setFormEdit({ ...formEdit, descripcion: e.target.value })} />
                    </div>
                    <div className="modal-fila-dos">
                      <div className="modal-campo">
                        <label>Puntos máximos (1–100)</label>
                        <input className="modal-input-texto" type="number" min={1} max={100}
                          value={formEdit.puntosMaximos}
                          onChange={e => setFormEdit({ ...formEdit, puntosMaximos: Number(e.target.value) })} />
                      </div>
                      <div className="modal-campo">
                        <label>Puntos gamificación</label>
                        <input className="modal-input-texto" type="number" min={0}
                          value={formEdit.puntosGamificacion}
                          onChange={e => setFormEdit({ ...formEdit, puntosGamificacion: Number(e.target.value) })} />
                      </div>
                    </div>
                    <div className="modal-campo">
                      <label>Fecha límite</label>
                      <input className="modal-input-texto" type="datetime-local" value={formEdit.fechaLimite}
                        onChange={e => setFormEdit({ ...formEdit, fechaLimite: e.target.value })} />
                    </div>
                    <div className="modal-fila-checks">
                      <label className="modal-check-label">
                        <input type="checkbox" checked={formEdit.permiteEntregasTardias}
                          onChange={e => setFormEdit({ ...formEdit, permiteEntregasTardias: e.target.checked })} />
                        <span>Permitir entregas tardías</span>
                      </label>
                    </div>
                    <div className="modal-campo">
                      <label>Categoría</label>
                      <select className="modal-input-texto" value={formEdit.idCategoria ?? ""}
                        onChange={e => setFormEdit({ ...formEdit, idCategoria: e.target.value ? Number(e.target.value) : null })}>
                        <option value="">Sin categoría</option>
                        {categorias.map(c => (
                          <option key={c.idCategoria} value={c.idCategoria}>{c.nombre} — {c.peso}%</option>
                        ))}
                      </select>
                    </div>
                    <div className="modal-campo">
                      <label>Estatus</label>
                      <div className="modal-estatus-opciones">
                        {(["Borrador", "Publicado", "Archivado"] as const).map(op => (
                          <button key={op} type="button"
                            className={`modal-estatus-btn ${formEdit.estatus === op ? "activo" : ""}`}
                            onClick={() => setFormEdit({ ...formEdit, estatus: op })}>
                            {op === "Borrador" ? "Borrador" : op === "Publicado" ? <><Eye size={13} strokeWidth={1.5} /> Publicado</> : <><Archive size={13} strokeWidth={1.5} /> Archivado</>}
                          </button>
                        ))}
                      </div>
                    </div>
                    <div className="detalle-edit-botones">
                      <button className="modal-btn-cancelar" onClick={() => { setEditando(false); setErrorEdit(""); }}>Cancelar</button>
                      <button className="modal-btn-confirmar" onClick={handleGuardarEdicion}
                        disabled={loadingEdit} style={{ opacity: loadingEdit ? 0.7 : 1 }}>
                        {loadingEdit ? "Guardando..." : "Guardar cambios"}
                      </button>
                    </div>
                  </div>
                )}
              </>
            )}
          </div>

          {/*  SIDEBAR  */}
          <div className="detalle-sidebar">
            <div className="detalle-info-card">
              <h3 className="detalle-info-titulo">Detalles</h3>
              <div className="detalle-info-fila">
                <span className="detalle-info-label">Puntos</span>
                <span className="detalle-info-valor">{actividad.puntosMaximos}</span>
              </div>
              {actividad.puntosGamificacionMaximos > 0 && (
                <div className="detalle-info-fila">
                  <span className="detalle-info-label"><Zap size={13} strokeWidth={1.5} /> Gamificación</span>
                  <span className="detalle-info-valor">{actividad.puntosGamificacionMaximos}</span>
                </div>
              )}
              <div className="detalle-info-fila">
                <span className="detalle-info-label"><Calendar size={13} strokeWidth={1.5} /> Fecha límite</span>
                <span className={`detalle-info-valor ${estaVencida ? "detalle-vencida" : ""}`}>
                  {actividad.fechaLimite ? formatFecha(actividad.fechaLimite) : "Sin límite"}
                </span>
              </div>
              {actividad.nombreCategoria && (
                <div className="detalle-info-fila">
                  <span className="detalle-info-label"><Tag size={13} strokeWidth={1.5} /> Categoría</span>
                  <span className="detalle-info-valor">{actividad.nombreCategoria} ({actividad.pesoCategoria}%)</span>
                </div>
              )}
              <div className="detalle-info-fila">
                <span className="detalle-info-label"><Clock size={13} strokeWidth={1.5} /> Tardías</span>
                <span className="detalle-info-valor">{actividad.permiteEntregasTardias ? "Permitidas" : "No permitidas"}</span>
              </div>
              {esMaestro && (
                <div className="detalle-info-fila">
                  <span className="detalle-info-label">Publicado</span>
                  <span className="detalle-info-valor">
                    {actividad.fechaPublicacion ? formatFechaCorta(actividad.fechaPublicacion) : "—"}
                  </span>
                </div>
              )}
            </div>
          </div>
        </div>
      </div>

      {/*  MODAL CALIFICAR  */}
      {modalCalificar && entregaSeleccionada && (
        <div className="modal-overlay" onClick={() => setModalCalificar(false)}>
          <div className="modal-card" onClick={e => e.stopPropagation()}>
            <h2 className="modal-titulo">Calificar entrega</h2>
            <p className="modal-descripcion">
              {entregaSeleccionada.nombreEstudiante} · v{entregaSeleccionada.version}
            </p>

            {entregaSeleccionada.recursos?.length > 0 && (
              <div className="calificar-recursos">
                <p className="calificar-recursos-titulo">Archivos entregados</p>
                {entregaSeleccionada.recursos.map((r: any) => (
                  <a key={r.idRecurso} href={`${BASE}${r.urlArchivo}`}
                    target="_blank" rel="noreferrer" className="recurso-link">
                    <FileText size={13} strokeWidth={1.5} /> {r.nombre}
                  </a>
                ))}
              </div>
            )}

            {entregaSeleccionada.comentarios && (
              <div className="calificar-comentario">
                <p className="calificar-comentario-label">Comentarios del alumno</p>
                <p className="calificar-comentario-texto">"{entregaSeleccionada.comentarios}"</p>
              </div>
            )}

            {errorCalificar && <p className="modal-error">{errorCalificar}</p>}

            <div className="modal-campo">
              <label>Puntuación (0–{actividad.puntosMaximos}) *</label>
              <input className="modal-input-texto" type="number"
                min={0} max={actividad.puntosMaximos}
                placeholder={`Ej: ${actividad.puntosMaximos}`}
                value={formCalificar.puntuacion}
                onChange={e => setFormCalificar({ ...formCalificar, puntuacion: e.target.value })} />
            </div>
            <div className="modal-campo">
              <label>Retroalimentación (opcional)</label>
              <textarea className="modal-input-texto modal-textarea" rows={3}
                placeholder="Comentarios para el estudiante..."
                value={formCalificar.retroalimentacion}
                onChange={e => setFormCalificar({ ...formCalificar, retroalimentacion: e.target.value })} />
            </div>
            <div className="modal-botones">
              <button className="modal-btn-cancelar" onClick={() => setModalCalificar(false)}>Cancelar</button>
              <button className="modal-btn-confirmar" onClick={handleCalificar}
                disabled={loadingCalificar} style={{ opacity: loadingCalificar ? 0.7 : 1 }}>
                {loadingCalificar ? "Guardando..." : "Guardar calificación"}
              </button>
            </div>
          </div>
        </div>
      )}
    </Layout>
  );
}