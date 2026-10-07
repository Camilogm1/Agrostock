-- =====================================================================
-- AgroStock — Script de Base de Datos (v2, con recomendaciones aplicadas)
-- Motor: MariaDB 10.2+ / MySQL 8.0.16+ (necesario para que CHECK se aplique)
-- Módulos: Producción e Inventario | Comercial | Seguridad (usuarios/roles)
-- Trazabilidad: cada tabla/columna referencia el RF/RNF/HU que satisface
-- (ver Entrega #1 — AgroStock)
--
-- Cambios respecto a la v1:
--   1) Se agrega la columna cultivos.tipo (RF-01 la pide explícitamente;
--      "variedad" se conserva como dato adicional, no la reemplaza).
--   2) Se agrega el trigger trg_cultivo_before_delete para que al intentar
--      eliminar un cultivo con cosechas, el mensaje sea claro (RNF-08) en
--      vez del error genérico de llave foránea.
--   3) Se documenta explícitamente que las reglas RF-07/16/17/18 viven en
--      triggers (decisión de arquitectura) para que el resto de modelos
--      del proyecto (clases, secuencia) se mantengan consistentes con esto.
-- =====================================================================

CREATE DATABASE IF NOT EXISTS agrostock
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE agrostock;

SET FOREIGN_KEY_CHECKS = 0;

-- =====================================================================
-- 1. USUARIOS Y ROLES  →  RNF-01 (autenticación), RNF-02 (roles)
-- =====================================================================
DROP TABLE IF EXISTS usuarios;
CREATE TABLE usuarios (
    id_usuario              INT AUTO_INCREMENT PRIMARY KEY,
    nombre_completo          VARCHAR(120)    NOT NULL,
    nombre_usuario             VARCHAR(50)     NOT NULL,
    contrasena_hash               VARCHAR(255)    NOT NULL,          -- nunca texto plano
    rol                              ENUM('administrador','vendedor') NOT NULL,  -- RNF-02
    activo                             BOOLEAN         NOT NULL DEFAULT TRUE,
    fecha_creacion                       TIMESTAMP       NOT NULL DEFAULT CURRENT_TIMESTAMP,
    fecha_actualizacion                    TIMESTAMP       NOT NULL DEFAULT CURRENT_TIMESTAMP
                                                            ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT uq_usuarios_nombre_usuario UNIQUE (nombre_usuario)
) ENGINE=InnoDB;


-- =====================================================================
-- 2. CULTIVOS  →  RF-01, RF-02, RF-03, RF-04
-- =====================================================================
DROP TABLE IF EXISTS cultivos;
CREATE TABLE cultivos (
    id_cultivo                  INT AUTO_INCREMENT PRIMARY KEY,
    nombre_cultivo                 VARCHAR(100)   NOT NULL,          -- ej: Tomate
    tipo                              VARCHAR(60)    NOT NULL,          -- RF-01: dato explícito del requisito (ej: Hortaliza)
    variedad                            VARCHAR(100),                     -- dato adicional del mockup (ej: Chonto)
    lote                                  VARCHAR(50)    NOT NULL,          -- ej: Lote 1
    cantidad_sembrada                       INT            NOT NULL,          -- ej: 500 plantas
    fecha_siembra                             DATE           NOT NULL,
    fecha_estimada_cosecha                       DATE,
    area_cultivada                                 DECIMAL(10,2),                  -- ej: 200
    unidad_area                                      VARCHAR(10)    NOT NULL DEFAULT 'm2',
    ubicacion                                          VARCHAR(100),                  -- redundante con lote, se deja por el mockup
    estado                                               ENUM('en_crecimiento','listo_para_cosecha',
                                                              'cosechado','cancelado')
                                                        NOT NULL DEFAULT 'en_crecimiento',
    id_usuario_registro                                   INT            NOT NULL,        -- quién lo registró (RNF-02)
    fecha_creacion                                          TIMESTAMP      NOT NULL DEFAULT CURRENT_TIMESTAMP,
    fecha_actualizacion                                       TIMESTAMP      NOT NULL DEFAULT CURRENT_TIMESTAMP
                                                                             ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT fk_cultivos_usuario FOREIGN KEY (id_usuario_registro)
        REFERENCES usuarios(id_usuario),
    CONSTRAINT chk_cultivos_cantidad_sembrada CHECK (cantidad_sembrada >= 0),   -- RNF-06
    CONSTRAINT chk_cultivos_area CHECK (area_cultivada IS NULL OR area_cultivada >= 0),
    CONSTRAINT chk_cultivos_nombre_no_vacio CHECK (CHAR_LENGTH(TRIM(nombre_cultivo)) > 0), -- RNF-06
    CONSTRAINT chk_cultivos_tipo_no_vacio CHECK (CHAR_LENGTH(TRIM(tipo)) > 0),             -- RNF-06
    CONSTRAINT chk_cultivos_lote_no_vacio CHECK (CHAR_LENGTH(TRIM(lote)) > 0)               -- RNF-06
) ENGINE=InnoDB;

CREATE INDEX idx_cultivos_lote ON cultivos(lote);          -- RNF-03 (consultas rápidas)
CREATE INDEX idx_cultivos_estado ON cultivos(estado);
CREATE INDEX idx_cultivos_tipo ON cultivos(tipo);


-- =====================================================================
-- 3. COSECHAS  →  RF-05, RF-06, RF-10, RF-11
-- =====================================================================
DROP TABLE IF EXISTS cosechas;
CREATE TABLE cosechas (
    id_cosecha                 INT AUTO_INCREMENT PRIMARY KEY,
    id_cultivo                   INT            NOT NULL,          -- RF-10: FK obliga a que exista
    fecha_cosecha                   DATE           NOT NULL,
    cantidad_cosechada                DECIMAL(10,2)  NOT NULL,          -- ej: 50 kg
    unidad_medida                       VARCHAR(10)    NOT NULL DEFAULT 'kg',
    calidad                               ENUM('primera','segunda','tercera')
                                        NOT NULL DEFAULT 'primera',
    lote                                    VARCHAR(50),                     -- ej: Lote 1
    observaciones                            VARCHAR(255),                    -- ej: "Sin daños"
    id_usuario_registro                       INT            NOT NULL,
    fecha_creacion                              TIMESTAMP      NOT NULL DEFAULT CURRENT_TIMESTAMP,
    fecha_actualizacion                           TIMESTAMP      NOT NULL DEFAULT CURRENT_TIMESTAMP
                                                                 ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT fk_cosechas_cultivo FOREIGN KEY (id_cultivo)
        REFERENCES cultivos(id_cultivo)
        ON DELETE RESTRICT,                                   -- RF-04: respaldo; el trigger da el mensaje amigable
    CONSTRAINT fk_cosechas_usuario FOREIGN KEY (id_usuario_registro)
        REFERENCES usuarios(id_usuario),
    CONSTRAINT chk_cosechas_cantidad_positiva CHECK (cantidad_cosechada > 0)   -- RF-06 / RNF-06
) ENGINE=InnoDB;

CREATE INDEX idx_cosechas_cultivo ON cosechas(id_cultivo);   -- RF-11 historial + RNF-03
CREATE INDEX idx_cosechas_fecha ON cosechas(fecha_cosecha);


-- =====================================================================
-- 4. INVENTARIO  →  RF-07, RF-08, RF-09
-- =====================================================================
DROP TABLE IF EXISTS inventario;
CREATE TABLE inventario (
    id_inventario             INT AUTO_INCREMENT PRIMARY KEY,
    id_cultivo                  INT            NOT NULL,
    cantidad_disponible            DECIMAL(10,2)  NOT NULL DEFAULT 0,
    unidad_medida                    VARCHAR(10)    NOT NULL DEFAULT 'kg',
    fecha_actualizacion                TIMESTAMP      NOT NULL DEFAULT CURRENT_TIMESTAMP
                                                       ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT fk_inventario_cultivo FOREIGN KEY (id_cultivo)
        REFERENCES cultivos(id_cultivo)
        ON DELETE RESTRICT,
    CONSTRAINT uq_inventario_cultivo UNIQUE (id_cultivo),      -- 1 registro de stock por cultivo
    CONSTRAINT chk_inventario_no_negativo CHECK (cantidad_disponible >= 0) -- RF-16/RF-18
) ENGINE=InnoDB;


-- =====================================================================
-- 5. CLIENTES  →  RF-12, RF-13, RNF-09
-- =====================================================================
DROP TABLE IF EXISTS clientes;
CREATE TABLE clientes (
    id_cliente                 INT AUTO_INCREMENT PRIMARY KEY,
    nombre_cliente                VARCHAR(120)   NOT NULL,
    tipo_identificacion              ENUM('CC','NIT','CE','TI') NOT NULL DEFAULT 'CC',
    numero_identificacion              VARCHAR(20)    NOT NULL,
    telefono                             VARCHAR(20),
    correo                                 VARCHAR(120),
    direccion                               VARCHAR(150),
    fecha_creacion                            TIMESTAMP      NOT NULL DEFAULT CURRENT_TIMESTAMP,
    fecha_actualizacion                         TIMESTAMP      NOT NULL DEFAULT CURRENT_TIMESTAMP
                                                               ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT uq_clientes_identificacion UNIQUE (numero_identificacion),  -- RNF-09
    CONSTRAINT chk_clientes_nombre_no_vacio CHECK (CHAR_LENGTH(TRIM(nombre_cliente)) > 0)
) ENGINE=InnoDB;

CREATE INDEX idx_clientes_nombre ON clientes(nombre_cliente);  -- RF-13 búsqueda


-- =====================================================================
-- 6. VENTAS  →  RF-14 a RF-22
-- =====================================================================
DROP TABLE IF EXISTS ventas;
CREATE TABLE ventas (
    id_venta                 INT AUTO_INCREMENT PRIMARY KEY,
    id_cliente                  INT            NOT NULL,
    id_cultivo                    INT            NOT NULL,          -- producto vendido
    cantidad_vendida                 DECIMAL(10,2)  NOT NULL,
    precio_unitario                     DECIMAL(12,2)  NOT NULL DEFAULT 0,
    total                                 DECIMAL(14,2)  GENERATED ALWAYS AS
                                           (cantidad_vendida * precio_unitario) STORED,
    fecha_venta                             DATE           NOT NULL,
    id_usuario_registro                        INT            NOT NULL,        -- vendedor
    fecha_creacion                               TIMESTAMP      NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_ventas_cliente FOREIGN KEY (id_cliente)
        REFERENCES clientes(id_cliente),
    CONSTRAINT fk_ventas_cultivo FOREIGN KEY (id_cultivo)
        REFERENCES cultivos(id_cultivo),
    CONSTRAINT fk_ventas_usuario FOREIGN KEY (id_usuario_registro)
        REFERENCES usuarios(id_usuario),
    CONSTRAINT chk_ventas_cantidad_positiva CHECK (cantidad_vendida > 0)   -- RF-06/RNF-06
) ENGINE=InnoDB;

CREATE INDEX idx_ventas_cliente ON ventas(id_cliente);   -- RF-22 búsqueda por cliente
CREATE INDEX idx_ventas_fecha ON ventas(fecha_venta);    -- RF-22 búsqueda por fecha

SET FOREIGN_KEY_CHECKS = 1;


-- =====================================================================
-- TRIGGERS — lógica de negocio (decisión de arquitectura: vive en la BD)
-- =====================================================================
DELIMITER $$

-- RF-04 / RNF-08: mensaje claro al intentar eliminar un cultivo con cosechas
-- (evita el error genérico de llave foránea)
DROP TRIGGER IF EXISTS trg_cultivo_before_delete $$
CREATE TRIGGER trg_cultivo_before_delete
BEFORE DELETE ON cultivos
FOR EACH ROW
BEGIN
    DECLARE total_cosechas INT;

    SELECT COUNT(*) INTO total_cosechas
    FROM cosechas
    WHERE id_cultivo = OLD.id_cultivo;

    IF total_cosechas > 0 THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'No se puede eliminar el cultivo: tiene cosechas registradas.';
    END IF;
END $$

-- RF-07: al registrar una cosecha, sumar automáticamente al inventario
-- del cultivo correspondiente (crea la fila si aún no existe)
DROP TRIGGER IF EXISTS trg_cosecha_after_insert $$
CREATE TRIGGER trg_cosecha_after_insert
AFTER INSERT ON cosechas
FOR EACH ROW
BEGIN
    INSERT INTO inventario (id_cultivo, cantidad_disponible, unidad_medida)
    VALUES (NEW.id_cultivo, NEW.cantidad_cosechada, NEW.unidad_medida)
    ON DUPLICATE KEY UPDATE
        cantidad_disponible = cantidad_disponible + NEW.cantidad_cosechada;
END $$

-- RF-16 / RF-18: antes de registrar una venta, verificar stock suficiente
-- y bloquear la operación con un mensaje claro si no alcanza (RNF-08)
DROP TRIGGER IF EXISTS trg_venta_before_insert $$
CREATE TRIGGER trg_venta_before_insert
BEFORE INSERT ON ventas
FOR EACH ROW
BEGIN
    DECLARE stock_actual DECIMAL(10,2);

    SELECT cantidad_disponible INTO stock_actual
    FROM inventario
    WHERE id_cultivo = NEW.id_cultivo
    FOR UPDATE;

    IF stock_actual IS NULL THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'No existe inventario registrado para este cultivo.';
    ELSEIF stock_actual < NEW.cantidad_vendida THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Stock insuficiente para completar la venta.';
    END IF;
END $$

-- RF-17: al confirmar una venta, descontar automáticamente del inventario
DROP TRIGGER IF EXISTS trg_venta_after_insert $$
CREATE TRIGGER trg_venta_after_insert
AFTER INSERT ON ventas
FOR EACH ROW
BEGIN
    UPDATE inventario
    SET cantidad_disponible = cantidad_disponible - NEW.cantidad_vendida
    WHERE id_cultivo = NEW.id_cultivo;
END $$

DELIMITER ;


-- =====================================================================
-- DATOS DE PRUEBA (seed)
-- =====================================================================
INSERT INTO usuarios (nombre_completo, nombre_usuario, contrasena_hash, rol) VALUES
('Juan Camilo González', 'jgonzalez', '$2y$10$hashEjemploAdmin', 'administrador'),
('Juan Felipe Cardona', 'jcardona', '$2y$10$hashEjemploVendedor', 'vendedor');

INSERT INTO cultivos
(nombre_cultivo, tipo, variedad, lote, cantidad_sembrada, fecha_siembra,
 fecha_estimada_cosecha, area_cultivada, unidad_area, ubicacion, estado, id_usuario_registro)
VALUES
('Tomate', 'Hortaliza', 'Chonto', 'Lote 1', 500, '2026-07-15', '2026-10-15', 200.00, 'm2', 'Lote 1', 'en_crecimiento', 1);

INSERT INTO cosechas
(id_cultivo, fecha_cosecha, cantidad_cosechada, unidad_medida, calidad, lote, observaciones, id_usuario_registro)
VALUES
(1, '2026-10-15', 50.00, 'kg', 'primera', 'Lote 1', 'Sin daños', 1);
-- ↑ El trigger trg_cosecha_after_insert ya deja 50 kg disponibles en inventario

INSERT INTO clientes (nombre_cliente, tipo_identificacion, numero_identificacion, telefono, correo)
VALUES ('María Restrepo', 'CC', '1020304050', '3001234567', 'maria.restrepo@example.com');

-- Venta de ejemplo (requiere que exista stock; el trigger valida y descuenta)
INSERT INTO ventas (id_cliente, id_cultivo, cantidad_vendida, precio_unitario, fecha_venta, id_usuario_registro)
VALUES (1, 1, 20.00, 3500.00, '2026-10-16', 2);

-- Prueba del trigger de eliminación (debe fallar con el mensaje claro):
-- DELETE FROM cultivos WHERE id_cultivo = 1;

-- Verificación rápida
-- SELECT * FROM inventario;   -- debe mostrar 30 kg disponibles (50 - 20)
