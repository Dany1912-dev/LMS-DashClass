import "../styles/GruposCurso.css";
import { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { API } from "../api";
import Sidebar from "../components/Sidebar";
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

interface Curso {
  idCurso: number;
  nombre: string;
  descripcion: string;
  imagenBanner: string;
  nombreProfesor: string;
  grupos: Grupo[];
}

export default function GruposCurso() {
  const { idCurso } = useParams();
  const navigate = useNavigate();

  const [curso, setCurso] = useState<Curso | null>(null);
  const [loading, setLoading] = useState(true);
  const [modalAgregar, setModalAgregar] = useState(false);
  const [errorAgregar, setErrorAgregar] = useState("");
  const [loadingAgregar, setLoadingAgregar] = useState(false);
  const [formGrupo, setFormGrupo] = useState({ nombre: "", descripcion: "" });

  useEffect(() => {
    const cargarCurso = async () => {
      try {
        const response = await fetch(API.cursoPorId(Number(idCurso)), {
          headers: {
            Authorization: `Bearer ${localStorage.getItem("token")}`,
          },
        });
        const data = await response.json();
        setCurso(data);
      } catch (err) {
        console.error("Error al cargar el curso:", err);
      } finally {
        setLoading(false);
      }
    };
    cargarCurso();
  }, [idCurso]);

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

  const handleAgregarGrupo = async () => {
    setErrorAgregar("");
    if (!formGrupo.nombre.trim()) {
      setErrorAgregar("El nombre del grupo es obligatorio");
      return;
    }
    const nombreDuplicado = curso?.grupos.some(
      (g) => g.nombre.toLowerCase() === formGrupo.nombre.trim().toLowerCase(),
    );
    if (nombreDuplicado) {
      setErrorAgregar("Ya existe un grupo con ese nombre en este curso");
      return;
    }
    setLoadingAgregar(true);
    try {
      const response = await fetch(API.agregarGrupo(Number(idCurso)), {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${localStorage.getItem("token")}`,
        },
        body: JSON.stringify({
          nombre: formGrupo.nombre.trim(),
          descripcion: formGrupo.descripcion.trim(),
        }),
      });
      if (!response.ok) {
        const data = await response.json();
        throw new Error(data.message || "Error al agregar el grupo");
      }
      const updatedResponse = await fetch(API.cursoPorId(Number(idCurso)), {
        headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
      });
      const updatedData = await updatedResponse.json();
      setCurso(updatedData);
      setModalAgregar(false);
      setFormGrupo({ nombre: "", descripcion: "" });
    } catch (err: any) {
      setErrorAgregar(err.message);
    } finally {
      setLoadingAgregar(false);
    }
  };

  return (
    <div className="grupos-layout">
      <Sidebar />

      <div className="grupos-main">
        {/* Header */}
        <div className="grupos-header-top">
          <div className="grupos-header-izquierda">
            <button className="btn-exit" onClick={() => navigate("/dashboard")}>
              ← Volver
            </button>
            <div>
              <h1 className="grupos-titulo-curso">
                {curso ? curso.nombre : "Cargando..."}
              </h1>
              <p className="grupos-subtitulo-curso">
                {curso ? curso.descripcion : ""}
              </p>
            </div>
          </div>
          <button className="btn-agregar" onClick={() => setModalAgregar(true)}>
            + Agregar grupo
          </button>
        </div>

        {/* Contenido */}
        <div className="grupos-contenido">
          {loading ? (
            <div className="grupos-loading">Cargando grupos...</div>
          ) : !curso ? (
            <div className="grupos-loading">No se encontró el curso.</div>
          ) : (
            <>
              <div className="grupos-seccion-header">
                <h2 className="grupos-seccion-titulo">Grupos</h2>
                <p className="grupos-seccion-subtitulo">
                  {curso.grupos.length} grupo
                  {curso.grupos.length !== 1 ? "s" : ""} en este curso
                </p>
              </div>

              {curso.grupos.length === 0 ? (
                <p className="grupos-vacio">Este curso no tiene grupos aún.</p>
              ) : (
                <div className="grupos-grid">
                  {curso.grupos.map((grupo) => (
                    <div key={grupo.idGrupo} className="grupo-card">
                      <div className="grupo-card-banner-wrapper">
                        {getBanner(curso.imagenBanner) ? (
                          <img
                            src={getBanner(curso.imagenBanner)!}
                            alt="banner"
                            className="grupo-card-banner"
                          />
                        ) : (
                          <div className="grupo-card-banner-placeholder" />
                        )}
                        <span className="grupo-card-codigo-badge">
                          {grupo.invitacion.codigo}
                        </span>
                      </div>
                      <div className="grupo-card-info">
                        <p className="grupo-card-nombre">{grupo.nombre}</p>
                        {grupo.descripcion && (
                          <p className="grupo-card-descripcion">
                            {grupo.descripcion}
                          </p>
                        )}
                        <p className="grupo-card-codigo-texto">
                          Código: {grupo.invitacion.codigo}
                        </p>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </>
          )}
        </div>
      </div>

      {/* Modal agregar grupo */}
      {modalAgregar && (
        <div className="modal-overlay" onClick={() => setModalAgregar(false)}>
          <div className="modal-card" onClick={(e) => e.stopPropagation()}>
            <h2 className="modal-titulo">Agregar grupo</h2>
            {errorAgregar && <p className="modal-error">{errorAgregar}</p>}
            <div className="modal-campo">
              <label>Nombre del grupo</label>
              <input
                className="modal-input-texto"
                type="text"
                placeholder="Ej: Grupo B"
                value={formGrupo.nombre}
                onChange={(e) =>
                  setFormGrupo({ ...formGrupo, nombre: e.target.value })
                }
              />
            </div>
            <div className="modal-campo">
              <label>Descripción (opcional)</label>
              <input
                className="modal-input-texto"
                type="text"
                placeholder="Ej: Turno vespertino"
                value={formGrupo.descripcion}
                onChange={(e) =>
                  setFormGrupo({ ...formGrupo, descripcion: e.target.value })
                }
              />
            </div>
            <div className="modal-botones">
              <button
                className="modal-btn-cancelar"
                onClick={() => {
                  setModalAgregar(false);
                  setErrorAgregar("");
                  setFormGrupo({ nombre: "", descripcion: "" });
                }}
              >
                Cancelar
              </button>
              <button
                className="modal-btn-confirmar"
                onClick={handleAgregarGrupo}
                disabled={loadingAgregar}
                style={{ opacity: loadingAgregar ? 0.7 : 1 }}
              >
                {loadingAgregar ? "Agregando..." : "Agregar"}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
