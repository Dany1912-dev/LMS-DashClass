CREATE DATABASE IF NOT EXISTS lms_gamificacion;
USE lms_gamificacion;

#==========================
#-----SIN DEPENDENCIAS-----
#==========================
CREATE TABLE usuarios (
	id_usuario INT PRIMARY KEY AUTO_INCREMENT,
    email VARCHAR(100) UNIQUE NOT NULL,
    proveedor_auth_principal ENUM('Google', 'Facebook', 'Microsoft', 'Telefono', 'Apple', 'Local') NOT NULL,
    nombre VARCHAR(100) NOT NULL,
    apellidos VARCHAR(100) NOT NULL,
    foto_perfil_url VARCHAR(100),
    biografia TEXT,
    fecha_creacion DATETIME DEFAULT CURRENT_TIMESTAMP,
	ultimo_acceso DATETIME,
    estatus BOOLEAN DEFAULT TRUE,
    
    INDEX index_email (email)
) ENGINE=InnoDB;

#===========================
#-SOLO DEPENDEN DE USUARIOS-
#===========================
CREATE TABLE cursos(
	id_curso INT PRIMARY KEY AUTO_INCREMENT,
    codigo VARCHAR(50) UNIQUE NOT NULL,
    nombre VARCHAR(255) NOT NULL,
    descripcion TEXT,
    imagen_banner VARCHAR(500),
    id_usuario INT NOT NULL,
    fecha_creacion DATETIME DEFAULT CURRENT_TIMESTAMP,
    activo BOOLEAN DEFAULT TRUE,
    
    FOREIGN KEY (id_usuario) REFERENCES usuarios(id_usuario),
    INDEX index_codigo (codigo),
    INDEX index_id_usuario (id_usuario)
)ENGINE=InnoDB;

CREATE TABLE metodos_auth_users (
	id_metodo INT PRIMARY KEY AUTO_INCREMENT,
    id_usuario INT NOT NULL,
    proveedor ENUM('Google', 'Facebook', 'Microsoft', 'Telefono', 'Apple', 'Local'),
    id_usuario_proveedor VARCHAR(255),
    contrasena_hash VARCHAR(255),
    telefono VARCHAR(20),
    telefono_verificado BOOLEAN DEFAULT FALSE,
    email VARCHAR(255),
    verificado BOOLEAN DEFAULT FALSE,
    vinculado_en DATETIME DEFAULT CURRENT_TIMESTAMP,
    ultimo_uso DATETIME,
    
    FOREIGN KEY (id_usuario) REFERENCES usuarios(id_usuario) ON DELETE CASCADE,
    UNIQUE KEY unique_proveedor_usuario (proveedor, id_usuario_proveedor),
    UNIQUE KEY unique_telefono (telefono),
    INDEX index_usuario (id_usuario),
    INDEX index_proveedor (proveedor)
)ENGINE=InnoDB;

#============================
#-----DEPENDEN DE CURSOS-----
#============================
CREATE TABLE grupos(
	id_grupo INT PRIMARY KEY AUTO_INCREMENT,
    id_curso INT NOT NULL,
    nombre VARCHAR(100) NOT NULL,
    descripcion TEXT,
    fecha_creacion DATETIME DEFAULT CURRENT_TIMESTAMP,
    estatus BOOLEAN DEFAULT TRUE,
    
    FOREIGN KEY (id_curso) REFERENCES cursos(id_curso),
    INDEX index_curso (id_curso)
    )ENGINE=InnoDB;
    
#===============================================
#-----DEPENDEN DE USUARIOS, CURSOS Y GRUPOS-----
#===============================================
CREATE TABLE miembros_curso (
	id_miembro_curso INT PRIMARY KEY AUTO_INCREMENT,
    id_curso INT NOT NULL,
    id_usuario INT NOT NULL,
    id_grupo INT,
    rol ENUM('Profesor', 'Estudiante') NOT NULL,
    fecha_inscripcion DATETIME DEFAULT CURRENT_TIMESTAMP,
    estatus BOOLEAN DEFAULT TRUE,
    
	FOREIGN KEY (id_curso) REFERENCES cursos (id_curso),
    FOREIGN KEY (id_usuario) REFERENCES usuarios(id_usuario),
    FOREIGN KEY (id_grupo) REFERENCES grupos(id_grupo),
    UNIQUE KEY unique_miembro (id_curso, id_usuario, id_grupo),
    INDEX index_curso (id_curso),
    INDEX index_usuario (id_usuario),
    INDEX index_grupo (id_grupo)
)ENGINE=InnoDB;
    
CREATE TABLE invitaciones_curso (
    id_invitacion INT PRIMARY KEY AUTO_INCREMENT,
    id_curso INT NOT NULL,
    id_grupo INT,
    tipo ENUM('Codigo', 'Enlace', 'Email') NOT NULL,
    codigo VARCHAR(6),
    token VARCHAR(255),
    fecha_expiracion DATETIME,
    fecha_creacion DATETIME DEFAULT CURRENT_TIMESTAMP,
    estatus BOOLEAN DEFAULT TRUE,
    
    FOREIGN KEY (id_curso) REFERENCES cursos(id_curso),
    FOREIGN KEY (id_grupo) REFERENCES grupos(id_grupo),
    INDEX index_curso (id_curso),
    INDEX index_codigo (codigo),
    INDEX index_token (token)
)ENGINE=InnoDB;

CREATE TABLE actividades (
    id_actividad INT PRIMARY KEY AUTO_INCREMENT,
    id_curso INT NOT NULL,
    titulo VARCHAR(255) NOT NULL,
    descripcion TEXT,
    puntos_maximos INT NOT NULL CHECK (puntos_maximos >= 1 AND puntos_maximos <= 100),
    puntos_gamificacion_maximos INT DEFAULT 0,
    fecha_limite DATETIME,
    permite_entregas_tardias BOOLEAN DEFAULT FALSE,
    estado ENUM('Borrador', 'Publicado', 'Programado') DEFAULT 'Borrador',
    fecha_publicacion DATETIME,
    id_usuario INT NOT NULL,
    fecha_creacion DATETIME DEFAULT CURRENT_TIMESTAMP,
    
    FOREIGN KEY (id_curso) REFERENCES cursos(id_curso),
    FOREIGN KEY (id_usuario) REFERENCES usuarios(id_usuario),
    INDEX index_curso (id_curso),
    INDEX index_estado (estado),
    INDEX index_usuario (id_usuario)
)ENGINE=InnoDB;

CREATE TABLE anuncios (
    id_anuncio INT PRIMARY KEY AUTO_INCREMENT,
    id_curso INT NOT NULL,
    id_grupo INT,
    titulo VARCHAR(255) NOT NULL,
    contenido TEXT NOT NULL,
    destacado BOOLEAN DEFAULT FALSE,
    estatus BOOLEAN DEFAULT TRUE,
    id_usuario INT NOT NULL,
    fecha_publicacion DATETIME DEFAULT CURRENT_TIMESTAMP,
    fecha_ultima_edicion DATETIME,
    
    FOREIGN KEY (id_curso) REFERENCES cursos(id_curso),
    FOREIGN KEY (id_grupo) REFERENCES grupos(id_grupo),
    FOREIGN KEY (id_usuario) REFERENCES usuarios(id_usuario),
    INDEX index_curso (id_curso),
    INDEX index_grupo (id_grupo),
    INDEX index_destacado (destacado)
)ENGINE=InnoDB;

CREATE TABLE sesiones_asistencia (
    id_sesion INT PRIMARY KEY AUTO_INCREMENT,
    id_curso INT NOT NULL,
    id_grupo INT,
    nombre VARCHAR(255) NOT NULL,
    clave_secreta VARCHAR(255) NOT NULL,
    codigo_verificacion VARCHAR(6) NOT NULL,
    intervalo_qr INT DEFAULT 10,
    fecha_creacion DATETIME DEFAULT CURRENT_TIMESTAMP,
    fecha_expiracion DATETIME NOT NULL,
    requiere_geolocalizacion BOOLEAN DEFAULT FALSE,
    latitud DECIMAL(10, 8),
    longitud DECIMAL(11, 8),
    radio_metros INT,
    id_usuario INT NOT NULL,
    estatus BOOLEAN DEFAULT TRUE,
    
    FOREIGN KEY (id_curso) REFERENCES cursos(id_curso),
    FOREIGN KEY (id_grupo) REFERENCES grupos(id_grupo),
    FOREIGN KEY (id_usuario) REFERENCES usuarios(id_usuario),
    INDEX index_curso (id_curso),
    INDEX index_grupo (id_grupo),
    INDEX index_codigo (codigo_verificacion)
)ENGINE=InnoDB;

CREATE TABLE transacciones_puntos (
    id_transaccion INT PRIMARY KEY AUTO_INCREMENT,
    id_usuario INT NOT NULL,
    id_curso INT NOT NULL,
    tipo ENUM('Ganado', 'Gastado', 'Transferido', 'Manual') NOT NULL,
    origen ENUM('Calificacion', 'Asistencia', 'Evaluacion', 'Social', 'Recompensa', 'Manual') NOT NULL,
    cantidad INT NOT NULL,
    balance_despues INT NOT NULL,
    id_referencia INT,
    descripcion VARCHAR(500),
    fecha_creacion DATETIME DEFAULT CURRENT_TIMESTAMP,
    
    FOREIGN KEY (id_usuario) REFERENCES usuarios(id_usuario),
    FOREIGN KEY (id_curso) REFERENCES cursos(id_curso),
    INDEX index_usuario (id_usuario),
    INDEX index_curso (id_curso),
    INDEX index_tipo (tipo),
    INDEX index_origen (origen)
)ENGINE=InnoDB;

CREATE TABLE recompensas (
    id_recompensa INT PRIMARY KEY AUTO_INCREMENT,
    id_curso INT NOT NULL,
    nombre VARCHAR(255) NOT NULL,
    descripcion TEXT,
    url_imagen VARCHAR(500),
    costo INT NOT NULL,
    limite_por_usuario INT,
    periodo_limite ENUM('Dia', 'Semana', 'Mes', 'Semestre'),
    cantidad_por_periodo INT,
    stock_global INT,
    stock_restante INT,
    disponible_desde DATETIME,
    disponible_hasta DATETIME,
    destacado BOOLEAN DEFAULT FALSE,
    estatus BOOLEAN DEFAULT TRUE,
    id_usuario INT NOT NULL,
    fecha_creacion DATETIME DEFAULT CURRENT_TIMESTAMP,
    
    FOREIGN KEY (id_curso) REFERENCES cursos(id_curso),
    FOREIGN KEY (id_usuario) REFERENCES usuarios(id_usuario),
    INDEX index_curso (id_curso),
    INDEX index_destacado (destacado)
)ENGINE=InnoDB;

CREATE TABLE evaluaciones (
    id_evaluacion INT PRIMARY KEY AUTO_INCREMENT,
    id_curso INT NOT NULL,
    id_grupo INT,
    titulo VARCHAR(255) NOT NULL,
    descripcion TEXT,
    modo ENUM('Kahoot', 'Formulario') NOT NULL,
    afecta_calificacion BOOLEAN DEFAULT FALSE,
    puntos_academicos_maximos INT,
    puntos_gamificacion INT DEFAULT 0,
    puntuacion_base INT DEFAULT 1000,
    penalidad_tiempo INT DEFAULT 10,
    mostrar_ranking_vivo BOOLEAN DEFAULT FALSE,
    permitir_entrada_tardia BOOLEAN DEFAULT FALSE,
    estado ENUM('Borrador', 'Listo', 'Activo', 'Completado') DEFAULT 'Borrador',
    id_usuario INT NOT NULL,
    fecha_creacion DATETIME DEFAULT CURRENT_TIMESTAMP,
    
    FOREIGN KEY (id_curso) REFERENCES cursos(id_curso),
    FOREIGN KEY (id_grupo) REFERENCES grupos(id_grupo),
    FOREIGN KEY (id_usuario) REFERENCES usuarios(id_usuario),
    INDEX index_curso (id_curso),
    INDEX index_grupo (id_grupo),
    INDEX index_estado (estado)
)ENGINE=InnoDB;

CREATE TABLE logros (
    id_logro INT PRIMARY KEY AUTO_INCREMENT,
    id_curso INT NOT NULL,
    nombre VARCHAR(255) NOT NULL,
    descripcion TEXT,
    url_icono VARCHAR(500),
    criterios JSON,
    estatus BOOLEAN DEFAULT TRUE,
    id_usuario INT NOT NULL,
    fecha_creacion DATETIME DEFAULT CURRENT_TIMESTAMP,
    
    FOREIGN KEY (id_curso) REFERENCES cursos(id_curso),
    FOREIGN KEY (id_usuario) REFERENCES usuarios(id_usuario),
    INDEX index_curso (id_curso)
)ENGINE=InnoDB;

CREATE TABLE transferencias_puntos (
    id_transferencia INT PRIMARY KEY AUTO_INCREMENT,
    desde_id_usuario INT NOT NULL,
    hacia_id_usuario INT NOT NULL,
    id_curso INT NOT NULL,
    cantidad INT NOT NULL,
    mensaje TEXT,
    anonima BOOLEAN DEFAULT FALSE,
    codigo_transferencia VARCHAR(20) UNIQUE NOT NULL,
    fecha_transferencia DATETIME DEFAULT CURRENT_TIMESTAMP,
    
    FOREIGN KEY (desde_id_usuario) REFERENCES usuarios(id_usuario),
    FOREIGN KEY (hacia_id_usuario) REFERENCES usuarios(id_usuario),
    FOREIGN KEY (id_curso) REFERENCES cursos(id_curso),
    INDEX index_desde_usuario (desde_id_usuario),
    INDEX index_hacia_usuario (hacia_id_usuario),
    INDEX index_curso (id_curso),
    INDEX index_codigo (codigo_transferencia)
)ENGINE=InnoDB;

CREATE TABLE estilos_aprendizaje (
    id_estilo INT PRIMARY KEY AUTO_INCREMENT,
    id_usuario INT UNIQUE NOT NULL,
    porcentaje_activo DECIMAL(5,2) NOT NULL,
    porcentaje_reflexivo DECIMAL(5,2) NOT NULL,
    porcentaje_sensorial DECIMAL(5,2) NOT NULL,
    porcentaje_intuitivo DECIMAL(5,2) NOT NULL,
    porcentaje_visual DECIMAL(5,2) NOT NULL,
    porcentaje_verbal DECIMAL(5,2) NOT NULL,
    porcentaje_secuencial DECIMAL(5,2) NOT NULL,
    porcentaje_global DECIMAL(5,2) NOT NULL,
    fecha_evaluacion DATETIME DEFAULT CURRENT_TIMESTAMP,
    respuestas_cuestionario JSON,
    
    FOREIGN KEY (id_usuario) REFERENCES usuarios(id_usuario),
    INDEX index_usuario (id_usuario)
)ENGINE=InnoDB;

#================================================
#-----DEPENDEN DE TABLAS DEL NIVEL ANTERIOR-----
#================================================
CREATE TABLE actividades_grupos (
    id_actividad_grupo INT PRIMARY KEY AUTO_INCREMENT,
    id_actividad INT NOT NULL,
    id_grupo INT NOT NULL,
    
    FOREIGN KEY (id_actividad) REFERENCES actividades(id_actividad),
    FOREIGN KEY (id_grupo) REFERENCES grupos(id_grupo),
    UNIQUE KEY unique_actividad_grupo (id_actividad, id_grupo),
    INDEX index_actividad (id_actividad),
    INDEX index_grupo (id_grupo)
)ENGINE=InnoDB;

CREATE TABLE materiales_actividad (
    id_material INT PRIMARY KEY AUTO_INCREMENT,
    id_actividad INT NOT NULL,
    tipo ENUM('Archivo', 'Enlace') NOT NULL,
    nombre VARCHAR(255) NOT NULL,
    url_archivo VARCHAR(500),
    tamano_archivo BIGINT,
    url_externa VARCHAR(500),
    fecha_subida DATETIME DEFAULT CURRENT_TIMESTAMP,
    
    FOREIGN KEY (id_actividad) REFERENCES actividades(id_actividad) ON DELETE CASCADE,
    INDEX index_actividad (id_actividad)
)ENGINE=InnoDB;

CREATE TABLE materiales_anuncio (
    id_material INT PRIMARY KEY AUTO_INCREMENT,
    id_anuncio INT NOT NULL,
    tipo ENUM('Archivo', 'Enlace') NOT NULL,
    nombre VARCHAR(255) NOT NULL,
    url_archivo VARCHAR(500),
    tamano_archivo BIGINT,
    url_externa VARCHAR(500),
    fecha_subida DATETIME DEFAULT CURRENT_TIMESTAMP,
    
    FOREIGN KEY (id_anuncio) REFERENCES anuncios(id_anuncio) ON DELETE CASCADE,
    INDEX index_anuncio (id_anuncio)
)ENGINE=InnoDB;

CREATE TABLE entregas (
    id_entrega INT PRIMARY KEY AUTO_INCREMENT,
    id_actividad INT NOT NULL,
    id_usuario INT NOT NULL,
    comentarios TEXT,
    fecha_entrega DATETIME DEFAULT CURRENT_TIMESTAMP,
    es_tardia BOOLEAN DEFAULT FALSE,
    version INT DEFAULT 1,
    estado ENUM('Entregada', 'Calificada', 'Reemplazada') DEFAULT 'Entregada',
    
    FOREIGN KEY (id_actividad) REFERENCES actividades(id_actividad),
    FOREIGN KEY (id_usuario) REFERENCES usuarios(id_usuario),
    UNIQUE KEY unique_entrega (id_actividad, id_usuario, version),
    INDEX index_actividad (id_actividad),
    INDEX index_usuario (id_usuario),
    INDEX index_estado (estado)
)ENGINE=InnoDB;

CREATE TABLE registros_asistencia (
    id_registro_asistencia INT PRIMARY KEY AUTO_INCREMENT,
    id_sesion_asistencia INT NOT NULL,
    id_usuario INT NOT NULL,
    metodo_usado ENUM('QR', 'Codigo', 'Geolocalizacion') NOT NULL,
    fecha_registro DATETIME DEFAULT CURRENT_TIMESTAMP,
    
    FOREIGN KEY (id_sesion_asistencia) REFERENCES sesiones_asistencia(id_sesion),
    FOREIGN KEY (id_usuario) REFERENCES usuarios(id_usuario),
    UNIQUE KEY unique_asistencia (id_sesion_asistencia, id_usuario),
    INDEX index_sesion (id_sesion_asistencia),
    INDEX index_usuario (id_usuario)
)ENGINE=InnoDB;

CREATE TABLE canjes (
    id_canje INT PRIMARY KEY AUTO_INCREMENT,
    id_recompensa INT NOT NULL,
    id_usuario INT NOT NULL,
    puntos_gastados INT NOT NULL,
    codigo_canje VARCHAR(20) UNIQUE NOT NULL,
    estado ENUM('Pendiente', 'Reclamado', 'Expirado') DEFAULT 'Pendiente',
    fecha_canje DATETIME DEFAULT CURRENT_TIMESTAMP,
    fecha_reclamado DATETIME,
    
    FOREIGN KEY (id_recompensa) REFERENCES recompensas(id_recompensa),
    FOREIGN KEY (id_usuario) REFERENCES usuarios(id_usuario),
    INDEX index_recompensa (id_recompensa),
    INDEX index_usuario (id_usuario),
    INDEX index_codigo (codigo_canje)
)ENGINE=InnoDB;

CREATE TABLE preguntas_evaluacion (
    id_pregunta INT PRIMARY KEY AUTO_INCREMENT,
    id_evaluacion INT NOT NULL,
    texto_pregunta TEXT NOT NULL,
    url_imagen VARCHAR(500),
    tipo ENUM('OpcionMultiple', 'VerdaderoFalso', 'RespuestaCorta') NOT NULL,
    opciones JSON,
    respuestas_correctas JSON,
    tiempo_limite INT,
    orden INT NOT NULL,
    en_banco_preguntas BOOLEAN DEFAULT FALSE,
    
    FOREIGN KEY (id_evaluacion) REFERENCES evaluaciones(id_evaluacion) ON DELETE CASCADE,
    INDEX index_evaluacion (id_evaluacion)
)ENGINE=InnoDB;

CREATE TABLE sesiones_evaluacion (
    id_sesion_evaluacion INT PRIMARY KEY AUTO_INCREMENT,
    id_evaluacion INT NOT NULL,
    codigo_sesion VARCHAR(6) UNIQUE NOT NULL,
    fecha_inicio DATETIME DEFAULT CURRENT_TIMESTAMP,
    fecha_fin DATETIME,
    estatus BOOLEAN DEFAULT TRUE,
    
    FOREIGN KEY (id_evaluacion) REFERENCES evaluaciones(id_evaluacion),
    INDEX index_evaluacion (id_evaluacion),
    INDEX index_codigo (codigo_sesion)
)ENGINE=InnoDB;

CREATE TABLE logros_usuario (
    id_logro_usuario INT PRIMARY KEY AUTO_INCREMENT,
    id_logro INT NOT NULL,
    id_usuario INT NOT NULL,
    fecha_desbloqueo DATETIME DEFAULT CURRENT_TIMESTAMP,
    
    FOREIGN KEY (id_logro) REFERENCES logros(id_logro),
    FOREIGN KEY (id_usuario) REFERENCES usuarios(id_usuario),
    UNIQUE KEY unique_logro_usuario (id_logro, id_usuario),
    INDEX index_logro (id_logro),
    INDEX index_usuario (id_usuario)
)ENGINE=InnoDB;

#==================================================
#-----DEPENDEN DE TABLAS DEL NIVEL 5 ANTERIOR-----
#==================================================
CREATE TABLE recursos_entrega (
    id_recurso INT PRIMARY KEY AUTO_INCREMENT,
    id_entrega INT NOT NULL,
    tipo ENUM('Archivo', 'Enlace') NOT NULL,
    nombre VARCHAR(255) NOT NULL,
    url_archivo VARCHAR(500),
    tamano_archivo BIGINT,
    url_externa VARCHAR(500),
    fecha_subida DATETIME DEFAULT CURRENT_TIMESTAMP,
    
    FOREIGN KEY (id_entrega) REFERENCES entregas(id_entrega) ON DELETE CASCADE,
    INDEX index_entrega (id_entrega)
)ENGINE=InnoDB;

CREATE TABLE calificaciones (
    id_calificacion INT PRIMARY KEY AUTO_INCREMENT,
    id_entrega INT UNIQUE NOT NULL,
    puntuacion DECIMAL(5,2) NOT NULL CHECK (puntuacion >= 0 AND puntuacion <= 100),
    retroalimentacion TEXT,
    datos_rubrica JSON,
    id_usuario INT NOT NULL,
    fecha_calificacion DATETIME DEFAULT CURRENT_TIMESTAMP,
    
    FOREIGN KEY (id_entrega) REFERENCES entregas(id_entrega),
    FOREIGN KEY (id_usuario) REFERENCES usuarios(id_usuario),
    INDEX index_entrega (id_entrega),
    INDEX index_usuario (id_usuario)
)ENGINE=InnoDB;

CREATE TABLE respuestas_estudiantes (
    id_respuesta INT PRIMARY KEY AUTO_INCREMENT,
    id_sesion_evaluacion INT NOT NULL,
    id_usuario INT NOT NULL,
    id_pregunta INT NOT NULL,
    respuesta_dada TEXT,
    es_correcta BOOLEAN,
    tiempo_tomado INT,
    puntuacion_obtenida INT,
    fecha_respuesta DATETIME DEFAULT CURRENT_TIMESTAMP,
    
    FOREIGN KEY (id_sesion_evaluacion) REFERENCES sesiones_evaluacion(id_sesion_evaluacion),
    FOREIGN KEY (id_usuario) REFERENCES usuarios(id_usuario),
    FOREIGN KEY (id_pregunta) REFERENCES preguntas_evaluacion(id_pregunta),
    INDEX index_sesion (id_sesion_evaluacion),
    INDEX index_usuario (id_usuario),
    INDEX index_pregunta (id_pregunta)
)ENGINE=InnoDB;