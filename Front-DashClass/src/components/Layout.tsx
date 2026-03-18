import "../styles/Layout.css";
import { useState, useEffect, useRef } from "react";
import { useNavigate, useLocation } from "react-router-dom";
import { API } from "../api";

interface Props {
  children: React.ReactNode;
  titulo?: string;
}

export default function Layout({ children, titulo }: Props) {
  const [collapsed, setCollapsed] = useState(false);
  const [menuAbierto, setMenuAbierto] = useState(false);
  const menuRef = useRef<HTMLDivElement>(null);
  const navigate = useNavigate();
  const location = useLocation();

  const usuario = JSON.parse(localStorage.getItem("usuario") || "{}");
  const inicial = usuario.nombre ? usuario.nombre.charAt(0).toUpperCase() : "?";

  useEffect(() => {
    const handler = (e: MouseEvent) => {
      if (menuRef.current && !menuRef.current.contains(e.target as Node)) {
        setMenuAbierto(false);
      }
    };
    document.addEventListener("mousedown", handler);
    return () => document.removeEventListener("mousedown", handler);
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
      console.error(err);
    } finally {
      localStorage.removeItem("token");
      localStorage.removeItem("refreshToken");
      localStorage.removeItem("usuario");
      navigate("/login");
    }
  };

  const navItems = [
    { label: "Panel",  icon: "⊞", path: "/dashboard" },
    { label: "Cursos", icon: "📚", path: "/cursos" },
  ];

  const isActive = (path: string) =>
    path === "/cursos"
      ? location.pathname.startsWith("/cursos")
      : location.pathname === path;

  return (
    <div className={`layout-container ${collapsed ? "sidebar-collapsed" : ""}`}>

      {/* ── SIDEBAR ── */}
      <aside className="layout-sidebar">
        {/* Brand + toggle */}
        <div className="sidebar-brand">
          {!collapsed && <span className="sidebar-brand-name">Dash Class</span>}
          <button
            className="sidebar-toggle-btn"
            onClick={() => setCollapsed(!collapsed)}
            title={collapsed ? "Expandir" : "Colapsar"}
          >
            {collapsed ? "→" : "←"}
          </button>
        </div>

        {/* Nav links */}
        <nav className="sidebar-nav">
          {navItems.map((item) => (
            <button
              key={item.path}
              className={`sidebar-nav-item ${isActive(item.path) ? "active" : ""}`}
              onClick={() => navigate(item.path)}
            >
              <span className="sidebar-nav-icon">{item.icon}</span>
              {!collapsed && <span>{item.label}</span>}
            </button>
          ))}
        </nav>

        {/* User at bottom */}
        <div className="sidebar-bottom" ref={menuRef}>
          <div className="sidebar-user" onClick={() => setMenuAbierto(!menuAbierto)}>
            <div className="sidebar-user-avatar">
              {usuario.fotoPerfilUrl
                ? <img src={usuario.fotoPerfilUrl} alt="foto" />
                : inicial}
            </div>
            {!collapsed && (
              <div className="sidebar-user-info">
                <p className="sidebar-user-name">{usuario.nombre} {usuario.apellidos}</p>
                <p className="sidebar-user-email">{usuario.email}</p>
              </div>
            )}
          </div>

          {menuAbierto && (
            <div className="perfil-menu">
              <div className="perfil-menu-header">
                <p className="perfil-menu-nombre">{usuario.nombre} {usuario.apellidos}</p>
                <p className="perfil-menu-email">{usuario.email}</p>
              </div>
              <div className="perfil-menu-divider" />
              <button className="perfil-menu-item">Mi perfil</button>
              <div className="perfil-menu-divider" />
              <button className="perfil-menu-item perfil-menu-logout" onClick={handleLogout}>
                Cerrar sesión
              </button>
            </div>
          )}
        </div>
      </aside>

      {/* ── MAIN ── */}
      <div className="layout-main">
        {/* Topbar */}
        <div className="layout-topbar">
          <span className="topbar-titulo">{titulo ?? "Dash Class"}</span>
          <div className="topbar-derecha">
            <span className="topbar-usuario">Hola, {usuario.nombre}!</span>
          </div>
        </div>

        {/* Page content */}
        <div className="layout-content">
          {children}
        </div>
      </div>
    </div>
  );
}