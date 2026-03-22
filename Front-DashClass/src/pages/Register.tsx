import "../styles/Login.css";
import "../styles/Register.css";
import { useState, useRef, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { API } from "../api";
import libros from "../assets/login/img_login_4.png";
import lapiz from "../assets/login/img_login_3.png";
import globo from "../assets/login/img_login_2.png";
import regla from "../assets/login/img_login_1.png";

type Paso = "formulario" | "verificacion";

export default function Register() {
  const [paso, setPaso] = useState<Paso>("formulario");

  // Formulario
  const [form, setForm] = useState({
    email: "",
    nombre: "",
    apellidos: "",
    contrasena: "",
    confirmarContrasena: "",
  });
  const [error,   setError]   = useState("");
  const [loading, setLoading] = useState(false);

  // Verificación
  const [codigo,          setCodigo]          = useState(["", "", "", "", "", ""]);
  const [errorVerif,      setErrorVerif]      = useState("");
  const [loadingVerif,    setLoadingVerif]    = useState(false);
  const [loadingReenvio,  setLoadingReenvio]  = useState(false);
  const [mensajeReenvio,  setMensajeReenvio]  = useState("");
  const [cooldown,        setCooldown]        = useState(0);

  const inputsRef = useRef<Array<HTMLInputElement | null>>([]);
  const navigate  = useNavigate();

  // Countdown del reenvío
  useEffect(() => {
    if (cooldown <= 0) return;
    const timer = setInterval(() => {
      setCooldown(prev => {
        if (prev <= 1) { clearInterval(timer); return 0; }
        return prev - 1;
      });
    }, 1000);
    return () => clearInterval(timer);
  }, [cooldown]);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setForm({ ...form, [e.target.name]: e.target.value });
  };

  // ── Paso 1: Registro ──
  const handleRegister = async () => {
    setError("");

    if (!form.email || !form.nombre || !form.apellidos || !form.contrasena || !form.confirmarContrasena) {
      setError("Por favor llena todos los campos");
      return;
    }

    if (form.contrasena !== form.confirmarContrasena) {
      setError("Las contraseñas no coinciden");
      return;
    }

    if (form.contrasena.length < 8) {
      setError("La contraseña debe tener al menos 8 caracteres");
      return;
    }

    setLoading(true);
    try {
      const res = await fetch(API.register, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(form),
      });

      if (!res.ok) {
        const data = await res.json();
        throw new Error(data.message || "Error al registrarse");
      }

      // Mostrar pantalla de verificación
      setPaso("verificacion");
      setCooldown(60);
      setTimeout(() => inputsRef.current[0]?.focus(), 100);
    } catch (err: any) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  // ── Manejo de inputs del código ──
  const handleCodigoChange = (index: number, value: string) => {
    // Solo números
    if (value && !/^\d$/.test(value)) return;

    const nuevo = [...codigo];
    nuevo[index] = value;
    setCodigo(nuevo);

    // Avanzar al siguiente input automáticamente
    if (value && index < 5) {
      inputsRef.current[index + 1]?.focus();
    }

    // Verificar automáticamente cuando se completan los 6 dígitos
    if (value && index === 5) {
      const codigoCompleto = [...nuevo].join("");
      if (codigoCompleto.length === 6) {
        verificarCodigo(codigoCompleto);
      }
    }
  };

  const handleCodigoPaste = (e: React.ClipboardEvent) => {
    e.preventDefault();
    const pegado = e.clipboardData.getData("text").replace(/\D/g, "").slice(0, 6);
    if (pegado.length === 0) return;

    const nuevo = ["", "", "", "", "", ""];
    pegado.split("").forEach((char, i) => { nuevo[i] = char; });
    setCodigo(nuevo);

    // Enfocar el último input llenado
    const ultimoIndex = Math.min(pegado.length - 1, 5);
    inputsRef.current[ultimoIndex]?.focus();

    if (pegado.length === 6) {
      verificarCodigo(pegado);
    }
  };

  const handleCodigoKeyDown = (index: number, e: React.KeyboardEvent) => {
    if (e.key === "Backspace" && !codigo[index] && index > 0) {
      inputsRef.current[index - 1]?.focus();
    }
  };

  // ── Paso 2: Verificar código ──
  const verificarCodigo = async (codigoStr?: string) => {
    const codigoFinal = codigoStr ?? codigo.join("");
    if (codigoFinal.length < 6) {
      setErrorVerif("Ingresa los 6 dígitos del código");
      return;
    }

    setErrorVerif("");
    setLoadingVerif(true);
    try {
      const res = await fetch(API.verificarEmail, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ email: form.email, codigo: codigoFinal }),
      });

      if (!res.ok) {
        const data = await res.json();
        throw new Error(data.message || "Código incorrecto");
      }

      const data = await res.json();

      // Si el backend devuelve tokens, guardarlos e ir al dashboard
      if (data.data?.accessToken) {
        localStorage.setItem("token", data.data.accessToken);
        localStorage.setItem("refreshToken", data.data.refreshToken);
        localStorage.setItem("usuario", JSON.stringify(data.data.usuario));
        navigate("/dashboard");
      } else {
        // Si solo devuelve mensaje, ir al login
        navigate("/login");
      }
    } catch (err: any) {
      setErrorVerif(err.message);
      // Limpiar inputs en caso de error
      setCodigo(["", "", "", "", "", ""]);
      setTimeout(() => inputsRef.current[0]?.focus(), 50);
    } finally {
      setLoadingVerif(false);
    }
  };

  // ── Reenviar código ──
  const handleReenviar = async () => {
    if (cooldown > 0) return;
    setMensajeReenvio("");
    setErrorVerif("");
    setLoadingReenvio(true);
    try {
      const res = await fetch(API.register, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(form),
      });

      if (!res.ok) {
        const data = await res.json();
        throw new Error(data.message || "Error al reenviar");
      }

      setCodigo(["", "", "", "", "", ""]);
      setMensajeReenvio("Código reenviado. Revisa tu correo.");
      setCooldown(60);
      setTimeout(() => inputsRef.current[0]?.focus(), 100);
    } catch (err: any) {
      setErrorVerif(err.message);
    } finally {
      setLoadingReenvio(false);
    }
  };

  return (
    <div className="register-container">
      <img className="img-fondo img-libros" src={libros} alt="libros" draggable="false" />
      <img className="img-fondo img-lapiz"  src={lapiz}  alt="lapiz"  draggable="false" />
      <img className="img-fondo img-globo"  src={globo}  alt="globo"  draggable="false" />
      <img className="img-fondo img-regla"  src={regla}  alt="regla"  draggable="false" />

      <h1 className="titulo-principal">Dash Class</h1>

      {/* ── PASO 1: Formulario ── */}
      {paso === "formulario" && (
        <div className="register-card">
          <h2 className="titulo-form">Crear cuenta</h2>

          {error && <p className="texto-error">{error}</p>}

          <div className="campo">
            <label>Nombre</label>
            <input type="text" name="nombre" value={form.nombre} onChange={handleChange} />
          </div>
          <div className="campo">
            <label>Apellidos</label>
            <input type="text" name="apellidos" value={form.apellidos} onChange={handleChange} />
          </div>
          <div className="campo">
            <label>Correo electronico</label>
            <input type="email" name="email" value={form.email} onChange={handleChange} />
          </div>
          <div className="campo">
            <label>Contrasena</label>
            <input type="password" name="contrasena" value={form.contrasena} onChange={handleChange} />
          </div>
          <div className="campo">
            <label>Confirmar contrasena</label>
            <input type="password" name="confirmarContrasena" value={form.confirmarContrasena} onChange={handleChange} />
          </div>

          <button
            className="btn-principal"
            onClick={handleRegister}
            disabled={loading}
            style={{ opacity: loading ? 0.7 : 1 }}
          >
            {loading ? "Enviando..." : "Crear cuenta"}
          </button>

          <p className="texto-registro">
            Ya tienes cuenta?{" "}
            <span onClick={() => navigate("/login")} className="link-login">
              Inicia sesion
            </span>
          </p>
        </div>
      )}

      {/* ── PASO 2: Verificación ── */}
      {paso === "verificacion" && (
        <div className="register-card verificacion-card">
          <div className="verificacion-icono">✉</div>
          <h2 className="titulo-form">Verifica tu correo</h2>
          <p className="verificacion-desc">
            Enviamos un código de 6 dígitos a<br />
            <strong>{form.email}</strong>
          </p>

          {errorVerif   && <p className="texto-error">{errorVerif}</p>}
          {mensajeReenvio && <p className="texto-exito">{mensajeReenvio}</p>}

          {/* Inputs del código */}
          <div className="codigo-inputs" onPaste={handleCodigoPaste}>
            {codigo.map((digito, i) => (
              <input
                key={i}
                ref={el => { inputsRef.current[i] = el; }}
                className="codigo-input"
                type="text"
                inputMode="numeric"
                maxLength={1}
                value={digito}
                onChange={e => handleCodigoChange(i, e.target.value)}
                onKeyDown={e => handleCodigoKeyDown(i, e)}
                disabled={loadingVerif}
              />
            ))}
          </div>

          <button
            className="btn-principal"
            onClick={() => verificarCodigo()}
            disabled={loadingVerif || codigo.join("").length < 6}
            style={{ opacity: (loadingVerif || codigo.join("").length < 6) ? 0.7 : 1 }}
          >
            {loadingVerif ? "Verificando..." : "Verificar"}
          </button>

          {/* Reenviar */}
          <div className="reenvio-wrap">
            <span className="reenvio-texto">No te llegó el correo? </span>
            {cooldown > 0 ? (
              <span className="reenvio-cooldown">Reenviar en {cooldown}s</span>
            ) : (
              <button
                className="reenvio-btn"
                onClick={handleReenviar}
                disabled={loadingReenvio}
              >
                {loadingReenvio ? "Enviando..." : "Reenviar código"}
              </button>
            )}
          </div>

          <button
            className="btn-secundario"
            onClick={() => {
              setPaso("formulario");
              setCodigo(["", "", "", "", "", ""]);
              setErrorVerif("");
            }}
          >
            Cambiar correo
          </button>
        </div>
      )}
    </div>
  );
}