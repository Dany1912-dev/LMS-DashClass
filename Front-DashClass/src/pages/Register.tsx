import "../styles/Login.css";
import "../styles/Register.css";
import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { API } from "../api";
import libros from "../assets/login/img_login_4.png";
import lapiz from "../assets/login/img_login_3.png";
import globo from "../assets/login/img_login_2.png";
import regla from "../assets/login/img_login_1.png";
import "../styles/Login.css";
import "../styles/Register.css";

export default function Register() {
  const [form, setForm] = useState({
    email: "",
    nombre: "",
    apellidos: "",
    contrasena: "",
    confirmarContrasena: "",
  });
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);
  const [exitoso, setExitoso] = useState(false);
  const navigate = useNavigate();

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setForm({ ...form, [e.target.name]: e.target.value });
  };

  const handleRegister = async () => {
    setError("");

    if (
      !form.email ||
      !form.nombre ||
      !form.apellidos ||
      !form.contrasena ||
      !form.confirmarContrasena
    ) {
      setError("Por favor llena todos los campos");
      return;
    }

    if (form.contrasena !== form.confirmarContrasena) {
      setError("Las contraseñas no coinciden");
      return;
    }

    setLoading(true);

    try {
      const response = await fetch(API.register, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(form),
      });

      if (!response.ok) {
        const data = await response.json();
        throw new Error(data.message || "Error al registrarse");
      }

      // Mostramos el modal de éxito
      setExitoso(true);
    } catch (err: any) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="register-container">
      <img
        className="img-fondo img-libros"
        src={libros}
        alt="libros"
        draggable="false"
      />
      <img
        className="img-fondo img-lapiz"
        src={lapiz}
        alt="lapiz"
        draggable="false"
      />
      <img
        className="img-fondo img-globo"
        src={globo}
        alt="globo"
        draggable="false"
      />
      <img
        className="img-fondo img-regla"
        src={regla}
        alt="regla"
        draggable="false"
      />

      <h1 className="titulo-principal">Dash Class</h1>

      <div className="register-card">
        <h2 className="titulo-form">Crear cuenta</h2>

        {error && <p className="texto-error">{error}</p>}

        <div className="campo">
          <label>Nombre</label>
          <input
            type="text"
            name="nombre"
            value={form.nombre}
            onChange={handleChange}
          />
        </div>

        <div className="campo">
          <label>Apellidos</label>
          <input
            type="text"
            name="apellidos"
            value={form.apellidos}
            onChange={handleChange}
          />
        </div>

        <div className="campo">
          <label>Correo electrónico</label>
          <input
            type="email"
            name="email"
            value={form.email}
            onChange={handleChange}
          />
        </div>

        <div className="campo">
          <label>Contraseña</label>
          <input
            type="password"
            name="contrasena"
            value={form.contrasena}
            onChange={handleChange}
          />
        </div>

        <div className="campo">
          <label>Confirmar contraseña</label>
          <input
            type="password"
            name="confirmarContrasena"
            value={form.confirmarContrasena}
            onChange={handleChange}
          />
        </div>

        <button
          className="btn-principal"
          onClick={handleRegister}
          disabled={loading}
          style={{ opacity: loading ? 0.7 : 1 }}
        >
          {loading ? "Registrando..." : "Crear cuenta"}
        </button>

        <p className="texto-registro">
          ¿Ya tienes cuenta?{" "}
          <span onClick={() => navigate("/login")} className="link-login">
            Inicia sesión
          </span>
        </p>
      </div>

      {exitoso && (
        <div className="modal-overlay">
          <div className="modal-card">
            <div className="modal-icono">✓</div>
            <h2 className="modal-titulo">¡Cuenta creada!</h2>
            <p className="modal-mensaje">Tu cuenta fue creada exitosamente.</p>
            <button
              className="btn-principal"
              onClick={() => navigate("/login")}
            >
              Ok
            </button>
          </div>
        </div>
      )}
    </div>
  );
}
