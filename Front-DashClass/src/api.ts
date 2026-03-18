const BASE_URL = import.meta.env.VITE_API_URL ?? "http://localhost:5000";

export const API = {
  // Auth
  login:          `${BASE_URL}/api/auth/login`,
  register:       `${BASE_URL}/api/auth/register`,
  verificarEmail: `${BASE_URL}/api/auth/verificar-email`,
  verificar2FA:   `${BASE_URL}/api/auth/verificar-2fa`,
  refresh:        `${BASE_URL}/api/auth/refresh`,
  logout:         `${BASE_URL}/api/auth/logout`,
  googleLogin:    `${BASE_URL}/api/auth/google`,
  microsoftLogin: `${BASE_URL}/api/auth/microsoft`,

  // Usuarios
  usuarioPerfil:    (id: number) => `${BASE_URL}/api/usuarios/${id}`,
  usuarioCursos:    (id: number) => `${BASE_URL}/api/usuarios/${id}/cursos`,
  usuarioLogros:    (id: number) => `${BASE_URL}/api/usuarios/${id}/logros`,
  usuarioEstadisticas: (id: number) => `${BASE_URL}/api/usuarios/${id}/estadisticas`,

  // Cursos
  crearCurso:         `${BASE_URL}/api/cursos`,
  unirseACurso:       `${BASE_URL}/api/cursos/unirse`,
  cursosPorUsuario:   (id: number) => `${BASE_URL}/api/cursos/usuario/${id}`,
  cursoPorId:         (id: number) => `${BASE_URL}/api/cursos/${id}`,
  editarCurso:        (id: number) => `${BASE_URL}/api/cursos/${id}`,
  cambiarEstatusCurso:(id: number) => `${BASE_URL}/api/cursos/${id}/estatus`,
  miembrosCurso:      (id: number) => `${BASE_URL}/api/cursos/${id}/miembros`,
  gruposCurso:        (id: number) => `${BASE_URL}/api/cursos/${id}/grupos`,
  agregarGrupo:       (id: number) => `${BASE_URL}/api/cursos/${id}/grupos`,
  invitacionesCurso:  (id: number) => `${BASE_URL}/api/cursos/${id}/invitaciones`,
  crearInvitacion:    (id: number) => `${BASE_URL}/api/cursos/${id}/invitaciones`,

  // Actividades
  crearActividad:             `${BASE_URL}/api/actividades`,
  actividadesCurso:           (idCurso: number) => `${BASE_URL}/api/actividades/curso/${idCurso}`,
  actividadesGrupo:           (idCurso: number, idGrupo: number) => `${BASE_URL}/api/actividades/curso/${idCurso}/grupo/${idGrupo}`,
  actividadPorId:             (id: number) => `${BASE_URL}/api/actividades/${id}`,
  editarActividad:            (id: number) => `${BASE_URL}/api/actividades/${id}`,
  cambiarEstatusActividad:    (id: number) => `${BASE_URL}/api/actividades/${id}/estatus`,
  eliminarActividad:          (id: number) => `${BASE_URL}/api/actividades/${id}`,

  // Entregas
  crearEntrega:               `${BASE_URL}/api/entregas`,
  marcarEntregada:            `${BASE_URL}/api/entregas/marcar`,
  subirEntrega:               `${BASE_URL}/api/entregas/subir`,
  entregaEstudiante:          (idActividad: number, idUsuario: number) => `${BASE_URL}/api/entregas/actividad/${idActividad}/estudiante/${idUsuario}`,
  entregasActividad:          (idActividad: number) => `${BASE_URL}/api/entregas/actividad/${idActividad}`,
  calificarEntrega:           (idEntrega: number) => `${BASE_URL}/api/entregas/${idEntrega}/calificar`,
  calificacionEntrega:        (idEntrega: number) => `${BASE_URL}/api/entregas/${idEntrega}/calificacion`,

  // Categorías
  crearCategoria:             `${BASE_URL}/api/categorias`,
  categoriasCurso:            (idCurso: number) => `${BASE_URL}/api/categorias/curso/${idCurso}`,
  calificacionFinal:          (idUsuario: number, idCurso: number) => `${BASE_URL}/api/categorias/calificacion/usuario/${idUsuario}/curso/${idCurso}`,
  calificacionesCurso:        (idCurso: number) => `${BASE_URL}/api/categorias/calificacion/curso/${idCurso}`,
  balancePuntos:   (userId: number, courseId: number) => `${BASE_URL}/api/points/balance/${userId}/${courseId}`,
  rankingCurso:    (courseId: number) => `${BASE_URL}/api/points/ranking/${courseId}`,

  // Recompensas
  recompensasActivas: (cursoId: number) => `${BASE_URL}/api/recompensas/curso/${cursoId}/activas`,

  // Logros
  logrosCurso:    (cursoId: number) => `${BASE_URL}/api/logros/curso/${cursoId}/activos`,
  logrosUsuario:  (userId: number, courseId: number) => `${BASE_URL}/api/logros/usuario/${userId}/curso/${courseId}`,
};