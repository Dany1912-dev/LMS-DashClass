import "../styles/Dashboard.css";
import { useState, useEffect, useRef } from "react";
import { useNavigate } from "react-router-dom";
import { API } from "../api";
import logo from "../assets/general/logo_largo.png";
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
  invitacion: Invitacion;
}

interface Curso {
  idCurso: number;
  nombre: string;
  descripcion: string;
  imagenBanner: string;
  idUsuario: number;
  codigo: string;
  nombreProfesor: string;
  grupos: Grupo[];
}

export default function Dashboard() {
  const [cursos, setCursos] = useState<Curso[]>([]);
  const [loading, setLoading] = useState(true);
  const [menuAbierto, setMenuAbierto] = useState(false);

  const [modalUnirse, setModalUnirse] = useState(false);
  const [codigoInput, setCodigoInput] = useState("");
  const [errorUnirse, setErrorUnirse] = useState("");
  const [loadingUnirse, setLoadingUnirse] = useState(false);

  const [modalCrear, setModalCrear] = useState(false);
  const [errorCrear, setErrorCrear] = useState("");
  const [loadingCrear, setLoadingCrear] = useState(false);
  const [formCurso, setFormCurso] = useState({
    nombre: "",
    descripcion: "",
    nombreGrupo: "",
    descripcionGrupo: "",
    banner: banner1,
  });

  const [modalEditar, setModalEditar] = useState(false);
  const [cursoEditando, setCursoEditando] = useState<Curso | null>(null);
  const [errorEditar, setErrorEditar] = useState("");
  const [loadingEditar, setLoadingEditar] = useState(false);
  const [formEditar, setFormEditar] = useState({
    nombre: "",
    descripcion: "",
    banner: banner1,
  });

  const menuRef = useRef<HTMLDivElement>(null);
  const navigate = useNavigate();

  const usuario = JSON.parse(localStorage.getItem("usuario") || "{}");
  const inicial = usuario.nombre ? usuario.nombre.charAt(0).toUpperCase() : "?";

  useEffect(() => {
    const handleClickFuera = (e: MouseEvent) => {
      if (menuRef.current && !menuRef.current.contains(e.target as Node)) {
        setMenuAbierto(false);
      }
    };
    document.addEventListener("mousedown", handleClickFuera);
    return () => document.removeEventListener("mousedown", handleClickFuera);
  }, []);

  const cargarCursos = async () => {
    try {
      const response = await fetch(API.cursosPorUsuario(usuario.idUsuario), {
        headers: {
          Authorization: `Bearer ${localStorage.getItem("token")}`,
        },
      });
      const data = await response.json();
      setCursos(data.cursos);
    } catch (err) {
      console.error("Error al cargar cursos:", err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    cargarCursos();
  }, []);

  const handleLogout = async () => {
    try {
      const refreshToken = localStorage.getItem("refreshToken");
      await fetch(API.logout, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(refreshToken),
      });
    } catch (err) {
      console.error("Error al cerrar sesión:", err);
    } finally {
      localStorage.removeItem("token");
      localStorage.removeItem("refreshToken");
      localStorage.removeItem("usuario");
      navigate("/login");
    }
  };

  const handleUnirse = async () => {
    setErrorUnirse("");
    if (!codigoInput.trim()) {
      setErrorUnirse("Por favor ingresa un código");
      return;
    }
    setLoadingUnirse(true);
    try {
      const response = await fetch(API.unirseACurso, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${localStorage.getItem("token")}`,
        },
        body: JSON.stringify({
          idUsuario: usuario.idUsuario,
          codigoOToken: codigoInput.trim(),
        }),
      });
      if (!response.ok) {
        const data = await response.json();
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
      const response = await fetch(API.crearCurso, {
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
          grupos: [
            {
              nombre: formCurso.nombreGrupo.trim(),
              descripcion: formCurso.descripcionGrupo.trim(),
            },
          ],
        }),
      });
      if (!response.ok) {
        const data = await response.json();
        throw new Error(data.message || "Error al crear el curso");
      }
      setModalCrear(false);
      setFormCurso({
        nombre: "",
        descripcion: "",
        nombreGrupo: "",
        descripcionGrupo: "",
        banner: banner1,
      });
      setLoading(true);
      cargarCursos();
    } catch (err: any) {
      setErrorCrear(err.message);
    } finally {
      setLoadingCrear(false);
    }
  };

  const abrirEditar = (curso: Curso) => {
    setCursoEditando(curso);
    const bannerIndex = Number(curso.imagenBanner);
    const bannerActual =
      !isNaN(bannerIndex) && bannerIndex >= 1 && bannerIndex <= banners.length
        ? banners[bannerIndex - 1]
        : banner1;
    setFormEditar({
      nombre: curso.nombre,
      descripcion: curso.descripcion,
      banner: bannerActual,
    });
    setModalEditar(true);
  };

  const handleEditarCurso = async () => {
    if (!cursoEditando) return;
    setErrorEditar("");
    if (!formEditar.nombre.trim()) {
      setErrorEditar("El nombre del curso es obligatorio");
      return;
    }
    setLoadingEditar(true);
    try {
      const response = await fetch(API.editarCurso(cursoEditando.idCurso), {
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
      if (!response.ok) {
        const data = await response.json();
        throw new Error(data.message || "Error al editar el curso");
      }
      setModalEditar(false);
      setCursoEditando(null);
      setLoading(true);
      cargarCursos();
    } catch (err: any) {
      setErrorEditar(err.message);
    } finally {
      setLoadingEditar(false);
    }
  };

  const getBanner = (imagenBanner: string) => {
    const index = Number(imagenBanner);
    if (!isNaN(index) && index >= 1 && index <= banners.length) {
      return banners[index - 1];
    }
    const match = imagenBanner.match(/banner_(\d+)\.(jpg|jpeg)/);
    if (match) {
      const bannerIndex = Number(match[1]);
      if (bannerIndex >= 1 && bannerIndex <= banners.length) {
        return banners[bannerIndex - 1];
      }
    }
    return null;
  };

  const cursosAlumno = cursos.filter(
    (curso) => curso.idUsuario !== usuario.idUsuario,
  );

  const cursosMaestro = cursos.filter(
    (curso) => curso.idUsuario === usuario.idUsuario,
  );

  return (
    <div className="dashboard-container">
      {/* Navbar */}
      <div className="dashboard-navbar">
        <div className="navbar-izquierda">
          <div className="navbar-perfil" ref={menuRef}>
            <div
              className="navbar-foto-wrapper"
              onClick={() => setMenuAbierto(!menuAbierto)}
            >
              {usuario.fotoPerfilUrl ? (
                <img
                  className="navbar-foto"
                  src={usuario.fotoPerfilUrl}
                  alt="foto"
                />
              ) : (
                <div className="navbar-foto-placeholder">{inicial}</div>
              )}
            </div>
            {menuAbierto && (
              <div className="perfil-menu">
                <div className="perfil-menu-header">
                  <p className="perfil-menu-nombre">
                    {usuario.nombre} {usuario.apellidos}
                  </p>
                  <p className="perfil-menu-email">{usuario.email}</p>
                </div>
                <div className="perfil-menu-divider" />
                <button className="perfil-menu-item">👤 Mi perfil</button>
                <div className="perfil-menu-divider" />
                <button
                  className="perfil-menu-item perfil-menu-logout"
                  onClick={handleLogout}
                >
                  🚪 Cerrar sesión
                </button>
              </div>
            )}
          </div>
          <span className="navbar-saludo">Hola, {usuario.nombre}!</span>
        </div>

        <div className="navbar-centro">
          <img
            src={logo}
            alt="logo"
            className="navbar-logo"
            draggable="false"
          />
        </div>

        <div className="navbar-derecha">
          <span className="navbar-titulo">Mis Cursos</span>
        </div>
      </div>

      {/* Modal unirse */}
      {modalUnirse && (
        <div className="modal-overlay" onClick={() => setModalUnirse(false)}>
          <div className="modal-card" onClick={(e) => e.stopPropagation()}>
            <h2 className="modal-titulo">Unirse a un curso</h2>
            <p className="modal-descripcion">
              Ingresa el código de 6 dígitos que te compartió tu maestro.
            </p>
            {errorUnirse && <p className="modal-error">{errorUnirse}</p>}
            <input
              className="modal-input"
              type="text"
              placeholder="Ej: 123700"
              value={codigoInput}
              onChange={(e) => setCodigoInput(e.target.value)}
              maxLength={6}
            />
            <div className="modal-botones">
              <button
                className="modal-btn-cancelar"
                onClick={() => {
                  setModalUnirse(false);
                  setCodigoInput("");
                  setErrorUnirse("");
                }}
              >
                Cancelar
              </button>
              <button
                className="modal-btn-confirmar"
                onClick={handleUnirse}
                disabled={loadingUnirse}
                style={{ opacity: loadingUnirse ? 0.7 : 1 }}
              >
                {loadingUnirse ? "Uniéndose..." : "Unirse"}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Modal crear curso */}
      {modalCrear && (
        <div className="modal-overlay" onClick={() => setModalCrear(false)}>
          <div className="modal-card" onClick={(e) => e.stopPropagation()}>
            <h2 className="modal-titulo">Crear curso</h2>
            {errorCrear && <p className="modal-error">{errorCrear}</p>}
            <div className="modal-campo">
              <label>Nombre del curso</label>
              <input
                className="modal-input-texto"
                type="text"
                placeholder="Ej: Matemáticas"
                value={formCurso.nombre}
                onChange={(e) =>
                  setFormCurso({ ...formCurso, nombre: e.target.value })
                }
              />
            </div>
            <div className="modal-campo">
              <label>Descripción del curso</label>
              <input
                className="modal-input-texto"
                type="text"
                placeholder="Ej: Curso de álgebra básica"
                value={formCurso.descripcion}
                onChange={(e) =>
                  setFormCurso({ ...formCurso, descripcion: e.target.value })
                }
              />
            </div>
            <div className="modal-campo">
              <label>Nombre del grupo</label>
              <input
                className="modal-input-texto"
                type="text"
                placeholder="Ej: Grupo A"
                value={formCurso.nombreGrupo}
                onChange={(e) =>
                  setFormCurso({ ...formCurso, nombreGrupo: e.target.value })
                }
              />
            </div>
            <div className="modal-campo">
              <label>Descripción del grupo</label>
              <input
                className="modal-input-texto"
                type="text"
                placeholder="Ej: Turno matutino"
                value={formCurso.descripcionGrupo}
                onChange={(e) =>
                  setFormCurso({
                    ...formCurso,
                    descripcionGrupo: e.target.value,
                  })
                }
              />
            </div>
            <div className="modal-campo">
              <label>Banner del curso</label>
              <div className="banner-grid">
                {banners.map((banner, index) => (
                  <img
                    key={index}
                    src={banner}
                    alt={`banner ${index + 1}`}
                    draggable="false"
                    className={`banner-opcion ${formCurso.banner === banner ? "banner-seleccionado" : ""}`}
                    onClick={() => setFormCurso({ ...formCurso, banner })}
                  />
                ))}
              </div>
            </div>
            <div className="modal-botones">
              <button
                className="modal-btn-cancelar"
                onClick={() => {
                  setModalCrear(false);
                  setErrorCrear("");
                  setFormCurso({
                    nombre: "",
                    descripcion: "",
                    nombreGrupo: "",
                    descripcionGrupo: "",
                    banner: banner1,
                  });
                }}
              >
                Cancelar
              </button>
              <button
                className="modal-btn-confirmar"
                onClick={handleCrearCurso}
                disabled={loadingCrear}
                style={{ opacity: loadingCrear ? 0.7 : 1 }}
              >
                {loadingCrear ? "Creando..." : "Crear"}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Modal editar curso */}
      {modalEditar && (
        <div className="modal-overlay" onClick={() => setModalEditar(false)}>
          <div className="modal-card" onClick={(e) => e.stopPropagation()}>
            <h2 className="modal-titulo">Editar curso</h2>
            {errorEditar && <p className="modal-error">{errorEditar}</p>}
            <div className="modal-campo">
              <label>Nombre del curso</label>
              <input
                className="modal-input-texto"
                type="text"
                value={formEditar.nombre}
                onChange={(e) =>
                  setFormEditar({ ...formEditar, nombre: e.target.value })
                }
              />
            </div>
            <div className="modal-campo">
              <label>Descripción del curso</label>
              <input
                className="modal-input-texto"
                type="text"
                value={formEditar.descripcion}
                onChange={(e) =>
                  setFormEditar({ ...formEditar, descripcion: e.target.value })
                }
              />
            </div>
            <div className="modal-campo">
              <label>Banner del curso</label>
              <div className="banner-grid">
                {banners.map((banner, index) => (
                  <img
                    key={index}
                    src={banner}
                    alt={`banner ${index + 1}`}
                    draggable="false"
                    className={`banner-opcion ${formEditar.banner === banner ? "banner-seleccionado" : ""}`}
                    onClick={() => setFormEditar({ ...formEditar, banner })}
                  />
                ))}
              </div>
            </div>
            <div className="modal-botones">
              <button
                className="modal-btn-cancelar"
                onClick={() => {
                  setModalEditar(false);
                  setErrorEditar("");
                }}
              >
                Cancelar
              </button>
              <button
                className="modal-btn-confirmar"
                onClick={handleEditarCurso}
                disabled={loadingEditar}
                style={{ opacity: loadingEditar ? 0.7 : 1 }}
              >
                {loadingEditar ? "Guardando..." : "Guardar"}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Contenido */}
      <div className="dashboard-contenido">
        {loading ? (
          <div className="dashboard-loading">Cargando cursos...</div>
        ) : (
          <>
            <div className="seccion">
              <div className="seccion-header">
                <h2 className="seccion-titulo">Alumno</h2>
                <button
                  className="btn-seccion"
                  onClick={() => setModalUnirse(true)}
                >
                  +
                </button>
              </div>
              {cursosAlumno.length === 0 ? (
                <p className="seccion-vacia">
                  No estás inscrito en ningún curso.
                </p>
              ) : (
                <div className="cursos-grid">
                  {cursosAlumno.map((curso) => (
                    <div key={curso.idCurso} className="curso-card">
                      {getBanner(curso.imagenBanner) ? (
                        <img
                          src={getBanner(curso.imagenBanner)!}
                          alt="banner"
                          className="curso-card-banner"
                        />
                      ) : (
                        <div className="curso-card-banner-placeholder">📚</div>
                      )}
                      <div className="curso-card-info">
                        <p className="curso-card-nombre">{curso.nombre}</p>
                        <p className="curso-card-profesor">
                          {curso.nombreProfesor}
                        </p>
                        {curso.grupos.length > 0 && (
                          <p className="curso-card-grupo">
                            Grupo: {curso.grupos[0].nombre}
                          </p>
                        )}
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </div>

            <div className="seccion">
              <div className="seccion-header">
                <h2 className="seccion-titulo">Maestro</h2>
                <button
                  className="btn-seccion"
                  onClick={() => setModalCrear(true)}
                >
                  +
                </button>
              </div>
              {cursosMaestro.length === 0 ? (
                <p className="seccion-vacia">No has creado ningún curso.</p>
              ) : (
                <div className="cursos-grid">
                  {cursosMaestro.map((curso) => (
                    <div key={curso.idCurso} className="curso-card">
                      {getBanner(curso.imagenBanner) ? (
                        <img
                          src={getBanner(curso.imagenBanner)!}
                          alt="banner"
                          className="curso-card-banner"
                        />
                      ) : (
                        <div className="curso-card-banner-placeholder">📚</div>
                      )}
                      <button
                        className="curso-card-menu-btn"
                        onClick={(e) => {
                          e.stopPropagation();
                          abrirEditar(curso);
                        }}
                      >
                        ⋮
                      </button>
                      <div className="curso-card-info">
                        <p className="curso-card-nombre">{curso.nombre}</p>
                        {curso.grupos.length > 0 && (
                          <p className="curso-card-profesor">
                            Código: {curso.grupos[0].invitacion.codigo}
                          </p>
                        )}
                        {curso.grupos.length > 0 && (
                          <p className="curso-card-grupo">
                            Grupo: {curso.grupos[0].nombre}
                          </p>
                        )}
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </div>
          </>
        )}
      </div>
    </div>
  );
}
