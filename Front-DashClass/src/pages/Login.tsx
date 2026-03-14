import "../styles/Login.css";
import regla from "../assets/img_login_1.png";
import globo from "../assets/img_login_2.png";
import lapiz from "../assets/img_login_3.png";
import libros from "../assets/img_login_4.png";
import google from "../assets/img_login_google.png";
import facebook from "../assets/img_login_facebook.png";
import microsoft from "../assets/img_login_microsoft.png";

export default function Login() {
  return (
    <div className="login-container">
      <img className="img-fondo img-libros" src={libros} alt="libros" />
      <img className="img-fondo img-lapiz" src={lapiz} alt="lapiz" />
      <img className="img-fondo img-globo" src={globo} alt="globo" />
      <img className="img-fondo img-regla" src={regla} alt="regla" />

      <h1 className="titulo-principal">Dash Class</h1>

      <div className="login-card">
        <h2 className="titulo-form">Inicio de Sesión</h2>

        <div className="campo">
          <label>Correo electrónico</label>
          <input type="email" />
        </div>

        <div className="campo">
          <label>Contraseña</label>
          <input type="password" />
        </div>

        <div className="contenedor-logos">
          <img className="img-google" src={google} alt="google" />
          <img className="img-facebook" src={facebook} alt="facebook" />
          <img className="img-microsoft" src={microsoft} alt="microsoft" />
        </div>

        <button className="btn-principal">Iniciar Sesión</button>

        <p className="texto-registro">No tienes una cuenta?</p>
        <button className="btn-secundario">Regístrate</button>
      </div>
    </div>
  );
}
