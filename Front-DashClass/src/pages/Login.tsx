import "../styles/Login.css";
import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { API } from "../api";
import regla from "../assets/login/img_login_1.png";
import globo from "../assets/login/img_login_2.png";
import lapiz from "../assets/login/img_login_3.png";
import libros from "../assets/login/img_login_4.png";
import google from "../assets/login/img_login_google.png";
import facebook from "../assets/login/img_login_facebook.png";
import microsoft from "../assets/login/img_login_microsoft.png";

export default function Login() {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();

  const handleLogin = async () => {
    setError("");

    if (!email || !password) {
      setError("Por favor llena todos los campos");
      return;
    }

    setLoading(true);

    try {
      const response = await fetch(API.login, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ email, contrasena: password }),
      });

      if (!response.ok) throw new Error("Credenciales incorrectas");

      const data = await response.json();
      localStorage.setItem("token", data.data.accessToken);
      localStorage.setItem("refreshToken", data.data.refreshToken);
      localStorage.setItem("usuario", JSON.stringify(data.data.usuario));

      navigate("/dashboard");
    } catch (err: any) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="login-container">
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

      <div className="login-card">
        <h2 className="titulo-form">Inicio de Sesión</h2>

        {error && <p className="texto-error">{error}</p>}

        <div className="campo">
          <label>Correo electrónico</label>
          <input
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
          />
        </div>

        <div className="campo">
          <label>Contraseña</label>
          <input
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
          />
        </div>

        <div className="contenedor-logos">
          <img
            className="img-google"
            src={google}
            alt="google"
            draggable="false"
          />
          <img
            className="img-facebook"
            src={facebook}
            alt="facebook"
            draggable="false"
          />
          <img
            className="img-microsoft"
            src={microsoft}
            alt="microsoft"
            draggable="false"
          />
        </div>

        <button
          className="btn-principal"
          onClick={handleLogin}
          disabled={loading}
          style={{ opacity: loading ? 0.7 : 1 }}
        >
          {loading ? "Entrando..." : "Iniciar Sesión"}
        </button>

        <p className="texto-registro">No tienes una cuenta?</p>
        <button
          className="btn-secundario"
          onClick={() => navigate("/register")}
        >
          Regístrate
        </button>
      </div>
    </div>
  );
}
