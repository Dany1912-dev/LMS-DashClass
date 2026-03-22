import { clearSession, isTokenExpired } from "../hooks/useAuth";

const BASE = import.meta.env.VITE_API_URL ?? "http://localhost:5000";

let isRefreshing  = false;
let refreshQueue: Array<(token: string | null) => void> = [];

/**
 * Intenta renovar el accessToken usando el refreshToken almacenado.
 * Si falla, limpia la sesión y redirige al login.
 */
async function renovarToken(): Promise<string | null> {
  const refreshToken = localStorage.getItem("refreshToken");
  if (!refreshToken) return null;

  try {
    const res = await fetch(`${BASE}/api/auth/refresh`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ refreshToken }),
    });

    if (!res.ok) return null;

    const data = await res.json();
    const nuevoToken = data.data?.accessToken ?? data.accessToken;
    if (!nuevoToken) return null;

    localStorage.setItem("token", nuevoToken);
    return nuevoToken;
  } catch {
    return null;
  }
}

/**
 * Wrapper sobre fetch que:
 * 1. Agrega el Authorization header automáticamente
 * 2. Renueva el token si está por expirar antes de hacer el request
 * 3. Si recibe 401, intenta renovar el token una sola vez y reintenta
 * 4. Si la renovación falla, cierra sesión y redirige al login
 */
export async function apiFetch(
  input: string,
  init: RequestInit = {}
): Promise<Response> {
  let token = localStorage.getItem("token");

  // Renovar proactivamente si el token está por expirar
  if (token && isTokenExpired(token)) {
    token = await manejarRefresh();
    if (!token) {
      redirigirLogin();
      return Promise.reject(new Error("Sesión expirada"));
    }
  }

  const headers = buildHeaders(init.headers, token);
  let response = await fetch(input, { ...init, headers });

  // Si recibe 401, intentar refresh una sola vez
  if (response.status === 401) {
    token = await manejarRefresh();

    if (!token) {
      redirigirLogin();
      return Promise.reject(new Error("Sesión expirada"));
    }

    // Reintentar el request original con el nuevo token
    const headersRetry = buildHeaders(init.headers, token);
    response = await fetch(input, { ...init, headers: headersRetry });

    // Si sigue siendo 401, la sesión es inválida
    if (response.status === 401) {
      redirigirLogin();
      return Promise.reject(new Error("Sesión inválida"));
    }
  }

  return response;
}

/**
 * Maneja el refresh con cola para evitar múltiples requests simultáneos
 */
async function manejarRefresh(): Promise<string | null> {
  if (isRefreshing) {
    // Encolar y esperar al refresh en curso
    return new Promise((resolve) => {
      refreshQueue.push(resolve);
    });
  }

  isRefreshing = true;

  const nuevoToken = await renovarToken();

  // Resolver toda la cola con el nuevo token (o null si falló)
  refreshQueue.forEach((resolve) => resolve(nuevoToken));
  refreshQueue = [];
  isRefreshing = false;

  if (!nuevoToken) {
    clearSession();
  }

  return nuevoToken;
}

/**
 * Construye los headers fusionando los del caller con Authorization
 */
function buildHeaders(
  existingHeaders: HeadersInit | undefined,
  token: string | null
): Headers {
  const headers = new Headers(existingHeaders);
  if (token) {
    headers.set("Authorization", `Bearer ${token}`);
  }
  return headers;
}

/**
 * Redirige al login limpiando la sesión
 */
function redirigirLogin() {
  clearSession();
  // Usar window.location para salir del contexto de React Router
  if (window.location.pathname !== "/login") {
    window.location.href = "/login";
  }
}