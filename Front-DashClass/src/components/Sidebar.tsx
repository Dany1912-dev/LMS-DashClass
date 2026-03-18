import { useNavigate, useLocation } from "react-router-dom";
import "../styles/Sidebar.css";
import logo from "../assets/general/logo_largo.png";

export default function Sidebar() {
  const navigate = useNavigate();
  const location = useLocation();

  const usuario = JSON.parse(localStorage.getItem("usuario") || "{}");
  const inicial = usuario.nombre ? usuario.nombre.charAt(0).toUpperCase() : "?";

  const handleLogout = async () => {
    try {
      const refreshToken = localStorage.getItem("refreshToken");
      await fetch(`${import.meta.env.VITE_API_URL}/api/Auth/logout`, {
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

  const isActive = (path: string) => location.pathname === path;

  return (
    <div className="sidebar">
      {/* Logo */}
      <div className="sidebar-logo">
        <img src={logo} alt="logo" draggable="false" />
      </div>

      {/* Navegación */}
      <nav className="sidebar-nav">
        <button
          className={`sidebar-nav-item ${isActive("/dashboard") ? "sidebar-nav-item-active" : ""}`}
          onClick={() => navigate("/dashboard")}
        >
          <span className="sidebar-nav-icon">⊞</span>
          Panel
        </button>
        <button
          className={`sidebar-nav-item ${location.pathname.startsWith("/curso") ? "sidebar-nav-item-active" : ""}`}
          onClick={() => navigate("/dashboard")}
        >
          <span className="sidebar-nav-icon">📚</span>
          Cursos
        </button>
      </nav>

      {/* Usuario abajo */}
      <div className="sidebar-footer">
        <div className="sidebar-usuario">
          {usuario.fotoPerfilUrl ? (
            <img
              className="sidebar-foto"
              src={usuario.fotoPerfilUrl}
              alt="foto"
            />
          ) : (
            <div className="sidebar-foto-placeholder">{inicial}</div>
          )}
          <div className="sidebar-usuario-info">
            <p className="sidebar-usuario-nombre">
              {usuario.nombre} {usuario.apellidos}
            </p>
            <p className="sidebar-usuario-email">{usuario.email}</p>
          </div>
        </div>
        <button className="sidebar-logout" onClick={handleLogout}>
          🚪 Cerrar sesión
        </button>
      </div>
    </div>
  );
}
