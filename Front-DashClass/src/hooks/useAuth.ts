import { useEffect, useState } from "react";
import { jwtDecode } from "jwt-decode";

interface JwtPayload {
  exp: number;
  sub?: string;
  [key: string]: any;
}

interface AuthState {
  isAuthenticated: boolean;
  isLoading: boolean;
  usuario: any | null;
}

const REFRESH_TOKEN_DIAS = 7;

/**
 * Verifica si un token JWT está expirado (con 30s de margen)
 */
export function isTokenExpired(token: string): boolean {
  try {
    const { exp } = jwtDecode<JwtPayload>(token);
    if (!exp) return true;
    return Date.now() >= (exp - 30) * 1000;
  } catch {
    return true;
  }
}

/**
 * Limpia todos los datos de sesión del localStorage incluyendo el timestamp 2FA
 */
export function clearSession() {
  localStorage.removeItem("token");
  localStorage.removeItem("refreshToken");
  localStorage.removeItem("usuario");
  localStorage.removeItem("2fa_ts");
}

/**
 * Guarda el timestamp del momento en que el usuario verificó el 2FA exitosamente
 */
export function guardar2FATimestamp() {
  localStorage.setItem("2fa_ts", String(Date.now()));
}

/**
 * Verifica si el 2FA fue completado en este dispositivo dentro del período
 * del refreshToken. Si es así, no se vuelve a pedir el código.
 */
export function is2FAValido(diasRefreshToken: number = 7): boolean {
  const ts = localStorage.getItem("2fa_ts");
  if (!ts) return false;
  const diasEnMs = diasRefreshToken * 24 * 60 * 60 * 1000;
  return Date.now() - Number(ts) < diasEnMs;
}

/**
 * Hook que expone el estado de autenticación actual
 */
export function useAuth(): AuthState {
  const [state, setState] = useState<AuthState>({
    isAuthenticated: false,
    isLoading: true,
    usuario: null,
  });

  useEffect(() => {
    const token   = localStorage.getItem("token");
    const usuario = localStorage.getItem("usuario");

    if (!token || !usuario || isTokenExpired(token)) {
      clearSession();
      setState({ isAuthenticated: false, isLoading: false, usuario: null });
      return;
    }

    setState({
      isAuthenticated: true,
      isLoading: false,
      usuario: JSON.parse(usuario),
    });
  }, []);

  return state;
}