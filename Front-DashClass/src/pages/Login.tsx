import "../styles/Login.css";
import { useState, useRef, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { GoogleLogin } from "@react-oauth/google";
import { useMsal } from "@azure/msal-react";
import { API } from "../api";
import { isTokenExpired, is2FAValido, guardar2FATimestamp } from "../hooks/useAuth";
import regla    from "../assets/login/img_login_1.png";
import globo    from "../assets/login/img_login_2.png";
import lapiz    from "../assets/login/img_login_3.png";
import libros   from "../assets/login/img_login_4.png";
import google   from "../assets/login/img_login_google.png";
import facebook from "../assets/login/img_login_facebook.png";
import microsoft from "../assets/login/img_login_microsoft.png";

type Paso = "credenciales" | "2fa";

export default function Login() {
  const [paso,     setPaso]     = useState<Paso>("credenciales");
  const [email,    setEmail]    = useState("");
  const [password, setPassword] = useState("");
  const [error,    setError]    = useState("");
  const [loading,  setLoading]  = useState(false);

  // 2FA
  const [codigo,         setCodigo]         = useState(["", "", "", "", "", ""]);
  const [error2FA,       setError2FA]       = useState("");
  const [loading2FA,     setLoading2FA]     = useState(false);
  const [loadingReenvio, setLoadingReenvio] = useState(false);
  const [mensajeReenvio, setMensajeReenvio] = useState("");
  const [cooldown,       setCooldown]       = useState(0);

  const inputsRef = useRef<Array<HTMLInputElement | null>>([]);
  const navigate  = useNavigate();
  const { instance } = useMsal();

  // Si ya hay sesión activa, redirigir al dashboard
  useEffect(() => {
    const token = localStorage.getItem("token");
    if (token && !isTokenExpired(token)) {
      navigate("/dashboard", { replace: true });
    }
  }, []);

  // Countdown reenvío
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

  // ── Guardar sesión y navegar ──
  const guardarSesion = (data: any) => {
    localStorage.setItem("token",        data.data.accessToken);
    localStorage.setItem("refreshToken", data.data.refreshToken);
    localStorage.setItem("usuario",      JSON.stringify(data.data.usuario));
    guardar2FATimestamp();
    navigate("/dashboard", { replace: true });
  };

  // ── Solicitar código 2FA al backend ──
  const solicitarCodigo2FA = async (): Promise<boolean> => {
    try {
      const res = await fetch(API.login, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ email, contrasena: password }),
      });
      // El backend con Enable2FA: false devuelve tokens directo
      // Con Enable2FA: true devuelve solo mensaje
      // Para forzar el envío del código usamos el endpoint de login
      // que internamente envía el correo cuando Enable2FA está activo
      return res.ok;
    } catch {
      return false;
    }
  };

  // ── Login local ──
  const handleLogin = async () => {
    setError("");
    if (!email || !password) { setError("Por favor llena todos los campos"); return; }
    setLoading(true);
    try {
      // Verificar si ya hizo 2FA recientemente en este dispositivo
      if (is2FAValido(7)) {
        // 2FA válido — hacer login directo
        const res = await fetch(API.login, {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({ email, contrasena: password }),
        });
        if (!res.ok) throw new Error("Credenciales incorrectas");
        const data = await res.json();

        if (data.data?.accessToken) {
          guardarSesion(data);
          return;
        }
        // Si el backend aún pide 2FA (Enable2FA: true), caer al flujo normal
      }

      // 2FA no válido — validar credenciales primero
      const resValidar = await fetch(API.login, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ email, contrasena: password }),
      });

      if (!resValidar.ok) throw new Error("Credenciales incorrectas");
      const dataValidar = await resValidar.json();

      // Si el backend devuelve tokens directo (Enable2FA: false en backend)
      // pero is2FAValido() es false, igual pedimos el 2FA desde el frontend
      if (dataValidar.data?.accessToken && !is2FAValido(7)) {
        // Guardar temporalmente los tokens para usarlos después de verificar 2FA
        sessionStorage.setItem("pending_token",        dataValidar.data.accessToken);
        sessionStorage.setItem("pending_refreshToken", dataValidar.data.refreshToken);
        sessionStorage.setItem("pending_usuario",      JSON.stringify(dataValidar.data.usuario));

        // Enviar código 2FA al correo
        await fetch(`${import.meta.env.VITE_API_URL ?? "http://localhost:5000"}/api/auth/solicitar-2fa`, {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({ email }),
        });

        setPaso("2fa");
        setCooldown(60);
        setTimeout(() => inputsRef.current[0]?.focus(), 100);
        return;
      }

      // Backend con Enable2FA: true — tokens aún no disponibles
      if (!dataValidar.data?.accessToken) {
        setPaso("2fa");
        setCooldown(60);
        setTimeout(() => inputsRef.current[0]?.focus(), 100);
        return;
      }

      guardarSesion(dataValidar);
    } catch (err: any) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  // ── Google ──
  const handleGoogleSuccess = async (credentialResponse: any) => {
    setLoading(true);
    try {
      const res = await fetch(API.googleLogin, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ idToken: credentialResponse.credential }),
      });
      if (!res.ok) throw new Error("Error al iniciar sesion con Google");
      guardarSesion(await res.json());
    } catch (err: any) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  // ── Microsoft ──
  const handleMicrosoftLogin = async () => {
    try {
      const result = await instance.loginPopup({
        scopes: ["openid", "profile", "email"],
        redirectUri: "http://localhost:5173/blank.html",
      });
      setLoading(true);
      const res = await fetch(API.microsoftLogin, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ idToken: result.idToken }),
      });
      if (!res.ok) throw new Error("Error al iniciar sesion con Microsoft");
      guardarSesion(await res.json());
    } catch (err: any) {
      if (err.errorCode !== "user_cancelled") setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  // ── Manejo inputs 2FA ──
  const handleCodigoChange = (index: number, value: string) => {
    if (value && !/^\d$/.test(value)) return;
    const nuevo = [...codigo];
    nuevo[index] = value;
    setCodigo(nuevo);
    if (value && index < 5) inputsRef.current[index + 1]?.focus();
    if (value && index === 5) {
      const completo = [...nuevo].join("");
      if (completo.length === 6) verificar2FA(completo);
    }
  };

  const handleCodigoPaste = (e: React.ClipboardEvent) => {
    e.preventDefault();
    const pegado = e.clipboardData.getData("text").replace(/\D/g, "").slice(0, 6);
    if (!pegado.length) return;
    const nuevo = ["", "", "", "", "", ""];
    pegado.split("").forEach((c, i) => { nuevo[i] = c; });
    setCodigo(nuevo);
    inputsRef.current[Math.min(pegado.length - 1, 5)]?.focus();
    if (pegado.length === 6) verificar2FA(pegado);
  };

  const handleCodigoKeyDown = (index: number, e: React.KeyboardEvent) => {
    if (e.key === "Backspace" && !codigo[index] && index > 0)
      inputsRef.current[index - 1]?.focus();
  };

  // ── Verificar código 2FA ──
  const verificar2FA = async (codigoStr?: string) => {
    const codigoFinal = codigoStr ?? codigo.join("");
    if (codigoFinal.length < 6) { setError2FA("Ingresa los 6 digitos del codigo"); return; }

    setError2FA("");
    setLoading2FA(true);
    try {
      // Verificar el código con el backend
      const res = await fetch(API.verificar2FA, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ email, codigo: codigoFinal }),
      });

      if (!res.ok) {
        const data = await res.json();
        throw new Error(data.message || "Codigo incorrecto");
      }

      const data = await res.json();

      // Si el backend devuelve tokens (Enable2FA: true), usarlos
      if (data.data?.accessToken) {
        guardarSesion(data);
        return;
      }

      // Si los tokens estaban pendientes en sessionStorage (flujo frontend)
      const pendingToken        = sessionStorage.getItem("pending_token");
      const pendingRefreshToken = sessionStorage.getItem("pending_refreshToken");
      const pendingUsuario      = sessionStorage.getItem("pending_usuario");

      if (pendingToken && pendingRefreshToken && pendingUsuario) {
        localStorage.setItem("token",        pendingToken);
        localStorage.setItem("refreshToken", pendingRefreshToken);
        localStorage.setItem("usuario",      pendingUsuario);
        sessionStorage.removeItem("pending_token");
        sessionStorage.removeItem("pending_refreshToken");
        sessionStorage.removeItem("pending_usuario");
        guardar2FATimestamp();
        navigate("/dashboard", { replace: true });
        return;
      }

      navigate("/dashboard", { replace: true });
    } catch (err: any) {
      setError2FA(err.message);
      setCodigo(["", "", "", "", "", ""]);
      setTimeout(() => inputsRef.current[0]?.focus(), 50);
    } finally {
      setLoading2FA(false);
    }
  };

  // ── Reenviar código 2FA ──
  const handleReenviar = async () => {
    if (cooldown > 0) return;
    setMensajeReenvio("");
    setError2FA("");
    setLoadingReenvio(true);
    try {
      const res = await fetch(API.login, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ email, contrasena: password }),
      });
      if (!res.ok) throw new Error("Error al reenviar el codigo");
      setCodigo(["", "", "", "", "", ""]);
      setMensajeReenvio("Codigo reenviado. Revisa tu correo.");
      setCooldown(60);
      setTimeout(() => inputsRef.current[0]?.focus(), 100);
    } catch (err: any) {
      setError2FA(err.message);
    } finally {
      setLoadingReenvio(false);
    }
  };

  return (
    <div className="login-container">
      <img className="img-fondo img-libros" src={libros}    alt="libros"    draggable="false" />
      <img className="img-fondo img-lapiz"  src={lapiz}     alt="lapiz"     draggable="false" />
      <img className="img-fondo img-globo"  src={globo}     alt="globo"     draggable="false" />
      <img className="img-fondo img-regla"  src={regla}     alt="regla"     draggable="false" />

      <h1 className="titulo-principal">Dash Class</h1>

      {/* ── PASO 1: Credenciales ── */}
      {paso === "credenciales" && (
        <div className="login-card">
          <h2 className="titulo-form">Inicio de Sesion</h2>

          {error && <p className="texto-error">{error}</p>}

          <div className="campo">
            <label>Correo electronico</label>
            <input
              type="email"
              value={email}
              onChange={e => setEmail(e.target.value)}
              onKeyDown={e => e.key === "Enter" && handleLogin()}
            />
          </div>
          <div className="campo">
            <label>Contrasena</label>
            <input
              type="password"
              value={password}
              onChange={e => setPassword(e.target.value)}
              onKeyDown={e => e.key === "Enter" && handleLogin()}
            />
          </div>

          <div className="contenedor-logos">
            <div className="google-btn-wrapper">
              <GoogleLogin
                onSuccess={handleGoogleSuccess}
                onError={() => setError("Error al iniciar sesion con Google")}
                type="icon" shape="circle" size="large"
              />
              <img className="img-google" src={google} alt="google" draggable="false" />
            </div>
            <img className="img-facebook" src={facebook} alt="facebook" draggable="false" />
            <img
              className="img-microsoft" src={microsoft} alt="microsoft" draggable="false"
              onClick={handleMicrosoftLogin}
              style={{ cursor: "pointer", pointerEvents: "auto" }}
            />
          </div>

          <button
            className="btn-principal"
            onClick={handleLogin}
            disabled={loading}
            style={{ opacity: loading ? 0.7 : 1 }}
          >
            {loading ? "Entrando..." : "Iniciar Sesion"}
          </button>

          <p className="texto-registro">No tienes una cuenta?</p>
          <button className="btn-secundario" onClick={() => navigate("/register")}>
            Registrate
          </button>
        </div>
      )}

      {/* ── PASO 2: Código 2FA ── */}
      {paso === "2fa" && (
        <div className="login-card verificacion-card">
          <div className="verificacion-icono">&#9993;</div>
          <h2 className="titulo-form">Codigo de acceso</h2>
          <p className="verificacion-desc">
            Enviamos un codigo de 6 digitos a<br />
            <strong>{email}</strong>
          </p>

          {error2FA       && <p className="texto-error">{error2FA}</p>}
          {mensajeReenvio && <p className="texto-exito">{mensajeReenvio}</p>}

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
                disabled={loading2FA}
              />
            ))}
          </div>

          <button
            className="btn-principal"
            onClick={() => verificar2FA()}
            disabled={loading2FA || codigo.join("").length < 6}
            style={{ opacity: (loading2FA || codigo.join("").length < 6) ? 0.7 : 1 }}
          >
            {loading2FA ? "Verificando..." : "Verificar"}
          </button>

          <div className="reenvio-wrap">
            <span className="reenvio-texto">No te llego el correo? </span>
            {cooldown > 0 ? (
              <span className="reenvio-cooldown">Reenviar en {cooldown}s</span>
            ) : (
              <button className="reenvio-btn" onClick={handleReenviar} disabled={loadingReenvio}>
                {loadingReenvio ? "Enviando..." : "Reenviar codigo"}
              </button>
            )}
          </div>

          <button
            className="btn-secundario"
            onClick={() => {
              setPaso("credenciales");
              setCodigo(["", "", "", "", "", ""]);
              setError2FA("");
              sessionStorage.removeItem("pending_token");
              sessionStorage.removeItem("pending_refreshToken");
              sessionStorage.removeItem("pending_usuario");
            }}
          >
            Volver
          </button>
        </div>
      )}
    </div>
  );
}