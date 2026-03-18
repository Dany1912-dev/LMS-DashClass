const BASE_URL = import.meta.env.VITE_API_URL;

export const API = {
  register: `${BASE_URL}/api/Auth/register`,
  login: `${BASE_URL}/api/Auth/login`,
  googleLogin: `${BASE_URL}/api/Auth/google`,
  microsoftLogin: `${BASE_URL}/api/Auth/microsoft`,
  cursosPorUsuario: (idUsuario: number) =>
    `${BASE_URL}/api/Cursos/usuario/${idUsuario}`,
  cursoPorId: (idCurso: number) => `${BASE_URL}/api/Cursos/${idCurso}`,
  logout: `${BASE_URL}/api/Auth/logout`,
  unirseACurso: `${BASE_URL}/api/Cursos/unirse`,
  crearCurso: `${BASE_URL}/api/Cursos`,
  editarCurso: (idCurso: number) => `${BASE_URL}/api/Cursos/${idCurso}`,
  agregarGrupo: (idCurso: number) => `${BASE_URL}/api/Cursos/${idCurso}/grupos`,
};
