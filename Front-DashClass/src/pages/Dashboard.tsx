import "../styles/Dashboard.css";
import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import Layout from "../components/Layout";
import { API } from "../api";
import { BookMarked, GraduationCap, School, BookOpen, ArrowRight } from "lucide-react";
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

export default function Dashboard() {
  const [cursos, setCursos]   = useState<Curso[]>([]);
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();

  const usuario = JSON.parse(localStorage.getItem("usuario") || "{}");

  useEffect(() => {
    const cargar = async () => {
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
    cargar();
  }, []);

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

  return (
    <Layout titulo="Panel">
      <div className="panel-page">

        {/* Welcome */}
        <div className="panel-welcome">
          <h1 className="panel-titulo">Bienvenido de nuevo, {usuario.nombre} </h1>
          <p className="panel-subtitulo">Resumen General</p>
        </div>

        {/* Stats */}
        <div className="panel-stats">
          <div className="stat-card">
            <div className="stat-icon"><BookMarked size={22} strokeWidth={1.5} /></div>
            <div className="stat-info">
              <p className="stat-label">Cursos Activos</p>
              <p className="stat-value">{cursos.length}</p>
            </div>
          </div>
          <div className="stat-card">
            <div className="stat-icon"><GraduationCap size={22} strokeWidth={1.5} /></div>
            <div className="stat-info">
              <p className="stat-label">Como Alumno</p>
              <p className="stat-value">{cursosAlumno.length}</p>
            </div>
          </div>
          <div className="stat-card">
            <div className="stat-icon"><School size={22} strokeWidth={1.5} /></div>
            <div className="stat-info">
              <p className="stat-label">Como Maestro</p>
              <p className="stat-value">{cursosMaestro.length}</p>
            </div>
          </div>
        </div>

        {/* Cursos list */}
        <div className="panel-seccion">
          <div className="panel-seccion-header">
            <h2 className="panel-seccion-titulo">Cursos Inscritos</h2>
            <button className="panel-ver-todo" onClick={() => navigate("/cursos")}>
              Ver todo <ArrowRight size={14} strokeWidth={1.5} />
            </button>
          </div>

          {loading ? (
            <p className="panel-vacio">Cargando...</p>
          ) : cursos.length === 0 ? (
            <p className="panel-vacio">No tienes cursos todavía.</p>
          ) : (
            <div className="panel-cursos-lista">
              {cursos.slice(0, 6).map((curso) => (
                <div
                  key={curso.idCurso}
                  className="panel-curso-row"
                  onClick={() => navigate(`/cursos/${curso.idCurso}`)}
                >
                  <div className="panel-curso-thumb">
                    {getBanner(curso.imagenBanner)
                      ? <img src={getBanner(curso.imagenBanner)!} alt="banner" />
                      : <span><BookOpen size={20} strokeWidth={1.5} /></span>}
                  </div>
                  <div className="panel-curso-info">
                    <p className="panel-curso-nombre">{curso.nombre}</p>
                    <p className="panel-curso-sub">
                      {curso.idUsuario === usuario.idUsuario
                        ? "Eres el maestro"
                        : `Prof. ${curso.nombreProfesor}`}
                    </p>
                  </div>
                  <span className={`panel-curso-badge ${curso.idUsuario === usuario.idUsuario ? "badge-maestro" : "badge-alumno"}`}>
                    {curso.idUsuario === usuario.idUsuario ? "Maestro" : "Alumno"}
                  </span>
                </div>
              ))}
            </div>
          )}
        </div>

      </div>
    </Layout>
  );
}