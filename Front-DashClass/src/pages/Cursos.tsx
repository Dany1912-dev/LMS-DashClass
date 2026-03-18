import "../styles/Cursos.css";
import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import Layout from "../components/Layout";
import { API } from "../api";
import banner1 from "../assets/banners/banner_1.jpg";
import banner2 from "../assets/banners/banner_2.jpg";
import banner3 from "../assets/banners/banner_3.jpg";
import banner4 from "../assets/banners/banner_4.jpeg";
import banner5 from "../assets/banners/banner_5.jpg";
import banner6 from "../assets/banners/banner_6.jpg";

const banners = [banner1, banner2, banner3, banner4, banner5, banner6];

interface Grupo {
  idGrupo: number;
  nombre: string;
  invitacion: { codigo: string; token: string };
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

export default function Cursos() {
  const [cursos, setCursos]   = useState<Curso[]>([]);
  const [loading, setLoading] = useState(true);
  const [tab, setTab]         = useState<"todos" | "alumno" | "maestro">("todos");

  const [modalUnirse, setModalUnirse]   = useState(false);
  const [codigoInput, setCodigoInput]   = useState("");
  const [errorUnirse, setErrorUnirse]   = useState("");
  const [loadingUnirse, setLoadingUnirse] = useState(false);

  const [modalCrear, setModalCrear]     = useState(false);
  const [errorCrear, setErrorCrear]     = useState("");
  const [loadingCrear, setLoadingCrear] = useState(false);
  const [formCurso, setFormCurso]       = useState({
    nombre: "", descripcion: "", nombreGrupo: "", descripcionGrupo: "", banner: banner1,
  });

  const navigate = useNavigate();
  const usuario  = JSON.parse(localStorage.getItem("usuario") || "{}");

  const cargarCursos = async () => {
    try {
      const res = await fetch(API.cursosPorUsuario(usuario.idUsuario), {
        headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
      });
      const data = await res.json();
      setCursos(data.cursos ?? []);
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { cargarCursos(); }, []);

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

  const cursosAlumno  = cursos.filter(c => c.idUsuario !== usuario.idUsuario);
  const cursosMaestro = cursos.filter(c => c.idUsuario === usuario.idUsuario);
  const cursosFiltrados = tab === "alumno" ? cursosAlumno : tab === "maestro" ? cursosMaestro : cursos;

  const handleUnirse = async () => {
    setErrorUnirse("");
    if (!codigoInput.trim()) { setErrorUnirse("Por favor ingresa un código"); return; }
    setLoadingUnirse(true);
    try {
      const res = await fetch(API.unirseACurso, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${localStorage.getItem("token")}`,
        },
        body: JSON.stringify({ idUsuario: usuario.idUsuario, codigoOToken: codigoInput.trim() }),
      });
      if (!res.ok) {
        const data = await res.json();
        throw new Error(data.message || "Código inválido");
      }
      setModalUnirse(false);
      setCodigoInput("");
      setLoading(true);
      cargarCursos();
    } catch (err: any) {
      setErrorUnirse(err.message);
    } finally {
      setLoadingUnirse(false);
    }
  };

  const handleCrearCurso = async () => {
    setErrorCrear("");
    if (!formCurso.nombre.trim() || !formCurso.nombreGrupo.trim()) {
      setErrorCrear("El nombre del curso y del grupo son obligatorios");
      return;
    }
    setLoadingCrear(true);
    try {
      const res = await fetch(API.crearCurso, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${localStorage.getItem("token")}`,
        },
        body: JSON.stringify({
          nombre: formCurso.nombre.trim(),
          descripcion: formCurso.descripcion.trim(),
          imagenBanner: String(banners.indexOf(formCurso.banner) + 1),
          idUsuario: usuario.idUsuario,
          grupos: [{ nombre: formCurso.nombreGrupo.trim(), descripcion: formCurso.descripcionGrupo.trim() }],
        }),
      });
      if (!res.ok) {
        const data = await res.json();
        throw new Error(data.message || "Error al crear el curso");
      }
      setModalCrear(false);
      setFormCurso({ nombre: "", descripcion: "", nombreGrupo: "", descripcionGrupo: "", banner: banner1 });
      setLoading(true);
      cargarCursos();
    } catch (err: any) {
      setErrorCrear(err.message);
    } finally {
      setLoadingCrear(false);
    }
  };

  return (
    <Layout titulo="Cursos">
      <div className="cursos-page">

        {/* Header */}
        <div className="cursos-header">
          <div>
            <h1 className="cursos-titulo">Mis Cursos</h1>
            <p className="cursos-subtitulo">Explora y gestiona tus cursos activos</p>
          </div>
          <div className="cursos-acciones">
            <button className="btn-unirse" onClick={() => setModalUnirse(true)}>
              + Unirse
            </button>
            <button className="btn-crear" onClick={() => setModalCrear(true)}>
              + Crear curso
            </button>
          </div>
        </div>

        {/* Tabs */}
        <div className="cursos-tabs">
          {(["todos", "alumno", "maestro"] as const).map(t => (
            <button
              key={t}
              className={`cursos-tab ${tab === t ? "active" : ""}`}
              onClick={() => setTab(t)}
            >
              {t === "todos" ? `Todos (${cursos.length})`
                : t === "alumno" ? `Alumno (${cursosAlumno.length})`
                : `Maestro (${cursosMaestro.length})`}
            </button>
          ))}
        </div>

        {/* Grid */}
        {loading ? (
          <p className="cursos-vacio">Cargando cursos...</p>
        ) : cursosFiltrados.length === 0 ? (
          <p className="cursos-vacio">No hay cursos en esta categoría.</p>
        ) : (
          <div className="cursos-grid">
            {cursosFiltrados.map((curso) => (
              <div
                key={curso.idCurso}
                className="curso-card"
                onClick={() => navigate(`/cursos/${curso.idCurso}`)}
              >
                <div className="curso-card-banner-wrap">
                  {getBanner(curso.imagenBanner)
                    ? <img src={getBanner(curso.imagenBanner)!} alt="banner" className="curso-card-banner" />
                    : <div className="curso-card-banner-placeholder"></div>}
                  <span className={`curso-card-rol ${curso.idUsuario === usuario.idUsuario ? "rol-maestro" : "rol-alumno"}`}>
                    {curso.idUsuario === usuario.idUsuario ? "Maestro" : "Alumno"}
                  </span>
                </div>
                <div className="curso-card-info">
                  <p className="curso-card-nombre">{curso.nombre}</p>
                  {curso.descripcion && (
                    <p className="curso-card-desc">{curso.descripcion}</p>
                  )}
                  <p className="curso-card-profesor">
                    {curso.idUsuario === usuario.idUsuario
                      ? `${curso.grupos.length} grupo${curso.grupos.length !== 1 ? "s" : ""}`
                      : `Prof. ${curso.nombreProfesor}`}
                  </p>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>

      {/* Modal unirse */}
      {modalUnirse && (
        <div className="modal-overlay" onClick={() => setModalUnirse(false)}>
          <div className="modal-card" onClick={e => e.stopPropagation()}>
            <h2 className="modal-titulo">Unirse a un curso</h2>
            <p className="modal-descripcion">Ingresa el código de 6 dígitos que te compartió tu maestro.</p>
            {errorUnirse && <p className="modal-error">{errorUnirse}</p>}
            <input
              className="modal-input"
              type="text"
              placeholder="Ej: 123700"
              value={codigoInput}
              onChange={e => setCodigoInput(e.target.value)}
              maxLength={6}
            />
            <div className="modal-botones">
              <button className="modal-btn-cancelar" onClick={() => { setModalUnirse(false); setCodigoInput(""); setErrorUnirse(""); }}>
                Cancelar
              </button>
              <button className="modal-btn-confirmar" onClick={handleUnirse} disabled={loadingUnirse} style={{ opacity: loadingUnirse ? 0.7 : 1 }}>
                {loadingUnirse ? "Uniéndose..." : "Unirse"}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Modal crear */}
      {modalCrear && (
        <div className="modal-overlay" onClick={() => setModalCrear(false)}>
          <div className="modal-card" onClick={e => e.stopPropagation()}>
            <h2 className="modal-titulo">Crear curso</h2>
            {errorCrear && <p className="modal-error">{errorCrear}</p>}
            <div className="modal-campo">
              <label>Nombre del curso</label>
              <input className="modal-input-texto" type="text" placeholder="Ej: Matemáticas" value={formCurso.nombre} onChange={e => setFormCurso({ ...formCurso, nombre: e.target.value })} />
            </div>
            <div className="modal-campo">
              <label>Descripción del curso</label>
              <input className="modal-input-texto" type="text" placeholder="Ej: Curso de álgebra básica" value={formCurso.descripcion} onChange={e => setFormCurso({ ...formCurso, descripcion: e.target.value })} />
            </div>
            <div className="modal-campo">
              <label>Nombre del grupo inicial</label>
              <input className="modal-input-texto" type="text" placeholder="Ej: Grupo A" value={formCurso.nombreGrupo} onChange={e => setFormCurso({ ...formCurso, nombreGrupo: e.target.value })} />
            </div>
            <div className="modal-campo">
              <label>Descripción del grupo</label>
              <input className="modal-input-texto" type="text" placeholder="Ej: Turno matutino" value={formCurso.descripcionGrupo} onChange={e => setFormCurso({ ...formCurso, descripcionGrupo: e.target.value })} />
            </div>
            <div className="modal-campo">
              <label>Banner del curso</label>
              <div className="banner-grid">
                {banners.map((banner, i) => (
                  <img key={i} src={banner} alt={`banner ${i+1}`} draggable="false"
                    className={`banner-opcion ${formCurso.banner === banner ? "banner-seleccionado" : ""}`}
                    onClick={() => setFormCurso({ ...formCurso, banner })}
                  />
                ))}
              </div>
            </div>
            <div className="modal-botones">
              <button className="modal-btn-cancelar" onClick={() => { setModalCrear(false); setErrorCrear(""); setFormCurso({ nombre: "", descripcion: "", nombreGrupo: "", descripcionGrupo: "", banner: banner1 }); }}>
                Cancelar
              </button>
              <button className="modal-btn-confirmar" onClick={handleCrearCurso} disabled={loadingCrear} style={{ opacity: loadingCrear ? 0.7 : 1 }}>
                {loadingCrear ? "Creando..." : "Crear"}
              </button>
            </div>
          </div>
        </div>
      )}
    </Layout>
  );
}