-- Crear base de datos
IF EXISTS (SELECT 1 FROM sys.databases WHERE name = 'Reclamos')
BEGIN
    USE master;
    
    ALTER DATABASE Reclamos
    SET SINGLE_USER
    WITH ROLLBACK IMMEDIATE;

    DROP DATABASE Reclamos;
END
GO

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name='Reclamos')
    CREATE DATABASE Reclamos;
GO

USE Reclamos;
GO

-- Tabla de Usuarios
CREATE TABLE Usuarios(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Nombres VARCHAR(50) NOT NULL,
    Apellidos VARCHAR(50) NOT NULL,
    Correo VARCHAR(100) UNIQUE NOT NULL,
    Contrasena VARBINARY(256) NOT NULL,
    Celular VARCHAR(15) NOT NULL,
    Convencional VARCHAR(15),
    RUC VARCHAR(13) UNIQUE NOT NULL,
    Rol VARCHAR(20) NOT NULL CHECK(Rol IN ('Cliente', 'Revisor', 'Tecnico', 'Personal de Entrega')),
    Fecha_Creacion DATETIME DEFAULT GETDATE() NOT NULL,
    Num_Cuenta_Bancaria VARCHAR(30) NULL DEFAULT NULL, 
    Tipo_Cuenta_Bancaria VARCHAR(20) NULL CHECK(Tipo_Cuenta_Bancaria IN ('Ahorro', 'Corriente')),
);
GO

CREATE TABLE TokensDeAcceso(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Token VARCHAR(256) NOT NULL,
    FechaCreacion DATETIME DEFAULT GETDATE() NOT NULL,
    FechaExpiracion DATETIME NOT NULL,
    Vigente BIT DEFAULT 1 NOT NULL,
        -- 1 = token activo y usable, 0 = inactivo (usado/expirado/revocado)
    Tipo_Token VARCHAR(20) DEFAULT 'ResetPassword' NULL 
        CHECK(Tipo_Token IN (
            'ResetPassword', -- Restablecer contraseña
            'ChangeEmail', -- Cambiar/corregir email
            'EmailVerification', -- Verificar cuenta (registro)
            'SecurityAlert' -- Alerta de seguridad
        )),
    FK_Usuario INT REFERENCES Usuarios(Id) NOT NULL
);
GO

CREATE TABLE Marcas(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Nombre VARCHAR(50) UNIQUE NOT NULL
);
GO

CREATE TABLE Usuarios_Certificacion_Marcas(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    FK_Marca INT REFERENCES Marcas(Id) NOT NULL,
    FK_Tecnico INT REFERENCES Usuarios(Id) NOT NULL,
    UNIQUE(FK_Marca, FK_Tecnico) -- Un tecnico no puede certificarse dos veces en misma marca
);
GO
 
CREATE TABLE Productos(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    FK_Marca INT REFERENCES Marcas(Id) NOT NULL,
    Modelo VARCHAR(50) NOT NULL,
    Especificacion VARCHAR(1000) NOT NULL,
    --Normalmente los productos van a tener garantia de 3 dias si es extremamente barato y generico, con costos de poquisimos dolares.
	--pero si tiene 0 es porque ese producto esta descontinuado por la marca y no se ofrece garantia alguna
    Dias_Garantia INT NOT NULL CHECK (Dias_Garantia >= 0),
    Precio DECIMAL(10,2) NOT NULL CHECK (Precio > 0),
    UNIQUE(FK_Marca, Modelo) -- Mismo modelo no puede repetirse en misma marca
);
GO

CREATE TABLE Numero_Serie_Productos(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    FK_Producto INT REFERENCES Productos(Id) NOT NULL,
    Numero_Serie VARCHAR(50) UNIQUE NOT NULL,
    Estado_Inventario VARCHAR(50) CHECK (Estado_Inventario IN 
    ('Se_Puede_Vender', 'Vendido', 'Entregado_Como_Reemplazo_Al_Cliente', 'Recibido_Del_Cliente_Por_Defecto_De_Fabrica'))
    -- Si se puede vender entonces puedes venderlo
	-- Si esta vendido, Entregado_Como_Reemplazo_Al_Cliente o Recibido_Del_Cliente_Por_Defecto_De_Fabrica no se lo puede vender
	-- Si esta vendido, Entregado_Como_Reemplazo_Al_Cliente se puede hacer un reclamo
	-- Si esta Recibido_Del_Cliente_Por_Defecto_De_Fabrica ya no se puede ni Vender, ni Reclamar, no se puede hacer nada, ya pasa a ser un producto no disponible que la marca oficial me tiene que reemplazar por otro nuevo
);
GO

CREATE TABLE Marca_Lo_Entrego_Como_Reemplazo(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    FK_Numero_Serie_Productos INT REFERENCES Numero_Serie_Productos(Id) NOT NULL,
);
GO

CREATE TABLE Ventas(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Codigo_Factura VARCHAR(50) UNIQUE NOT NULL,
    FK_Empresa_Cliente INT REFERENCES Usuarios(Id) NOT NULL,
    Fecha_Compra DATETIME DEFAULT GETDATE(),
    Total_Compra DECIMAL(10,2) NOT NULL CHECK (Total_Compra >= 0)
);
GO

CREATE TABLE Ventas_Por_Numero_Serie_Productos(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    FK_Ventas INT REFERENCES Ventas(Id) NOT NULL,
    FK_Numero_Serie_Producto INT REFERENCES Numero_Serie_Productos(Id) NOT NULL UNIQUE,
    Precio_Venta DECIMAL(10,2) NOT NULL CHECK (Precio_Venta >= 0),
);
GO

CREATE TABLE Reclamos(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Codigo_Reclamo VARCHAR(50) UNIQUE NOT NULL,
    FK_Empresa_Cliente INT REFERENCES Usuarios(Id) NOT NULL,
    Fecha_Creacion_Reclamo DATETIME DEFAULT GETDATE(),
);
GO

CREATE TABLE Reclamos_Producto_SN(
    Id INT IDENTITY(1,1) PRIMARY KEY,
	--MUCHO CUIDADO!!! que solo puedes crear un reclamo de aquellos productos Vendidos o Entregados_Como_Reemplazo_Al_Cliente
    FK_Numero_Serie_Productos INT REFERENCES Numero_Serie_Productos(Id) NOT NULL UNIQUE,
    FK_Reclamos INT REFERENCES Reclamos(Id) NOT NULL,
    Fecha_Venta_Cliente_Final DATETIME NOT NULL,
    Fecha_Reclamo_Cliente_Final DATETIME DEFAULT NULL,
    Forma_Compensacion VARCHAR(20) CHECK (Forma_Compensacion IN ('Reembolso', 'Reemplazo')) NOT NULL,
    --Pendiente cuando inicia el proceso, en revision cuando se asigna de Tecnico, 
	--Aprobado o Reprobado cuando el Tecnico da su respuesta
	--compensado cuando se le entrega el Reemplazo o Reembolso a la empresa cliente
    Estado VARCHAR(20) NOT NULL DEFAULT 'Pendiente' CHECK (Estado IN ('Pendiente', 'En Revision', 'Aprobado', 'Rechazado', 'Compensado')),
    FK_Tecnico_Asignado INT REFERENCES Usuarios(Id),
    Fecha_Revision_Tecnico DATETIME,
    Explicacion_Respuesta_Tecnico VARCHAR(1000),
    PDF_Revision_Tecnico VARCHAR(255),
);
GO

CREATE TABLE Reembolso(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Numero_Comprobante_Reembolso VARCHAR(50) NOT NULL,
    Fecha_Reembolso DATETIME NOT NULL DEFAULT GETDATE(),
	--Numero de cuenta en el momento en que se realizo el reembolso, queda como historial para auditoria.
    Num_Cuenta_Bancaria_Reembolso VARCHAR(30) NOT NULL 
);
GO

CREATE TABLE Reembolso_Por_Reclamos(
    Id INT IDENTITY(1,1) PRIMARY KEY,
	--solo se puede reembolsar, reclamos que hallan sido aprobados por el tecnico
    FK_Reclamos_Producto_SN INT REFERENCES Reclamos_Producto_SN(Id) NOT NULL UNIQUE,
    FK_Reembolso INT REFERENCES Reembolso(Id) NOT NULL,
);
GO

-- Tabla modificada directamente con la columna NULLable
CREATE TABLE Comprobante_De_Reemplazo(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    PDF_Comprobante_Entrega_Cliente VARCHAR(255) NOT NULL,
    FK_Personal_Entrega INT REFERENCES Usuarios(Id) NOT NULL,
    --es el personal de entrega que le esta dando los productos al cliente y le esta haciendo firmar.
    Estado VARCHAR(20) DEFAULT 'Pendiente' 
    CHECK (Estado IN ('Pendiente', 'Generado', 'Firmado', 'Completado'))
);
GO

-- Tabla modificada directamente con FK NULLable y ON DELETE SET NULL
CREATE TABLE Comprobante_Producto_Reemplazado(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    FK_Reclamos_Producto_SN INT REFERENCES Reclamos_Producto_SN(Id) NOT NULL UNIQUE,
    FK_Comprobante_De_Reemplazo INT NULL,
	--Producto_De_Reemplazo es el producto que debe ser de la misma marca y mismo modelo que se le dara como reemplazo de un producto defectuoso
	-- Si ya se dio ese producto como reemplazo de Reemplazo ese producto pasa a no estar Disponibilidad_De_Venta osea un Disponibilidad_De_Venta en false.
    FK_Producto_De_Reemplazo INT REFERENCES Numero_Serie_Productos(Id) NOT NULL UNIQUE,
    CONSTRAINT FK_Comprobante_Producto_Reemplazado_Comprobante_De_Reemplazo
    FOREIGN KEY (FK_Comprobante_De_Reemplazo)
    REFERENCES dbo.Comprobante_De_Reemplazo(Id)
    ON DELETE SET NULL
);
GO

-- ============================================
-- INSERTAR DATOS
-- ============================================

-- INSERTAR USUARIOS (con Fecha_Creacion explícita)
INSERT INTO Usuarios (Nombres, Apellidos, Correo, Contrasena, Celular, Convencional, RUC, Rol, Fecha_Creacion, Num_Cuenta_Bancaria, Tipo_Cuenta_Bancaria)
VALUES 
('Juan', 'Pérez', 'juan.perez@gmail.com', HASHBYTES('SHA2_256', 'Juan123*'), '0987654321', '042345678', '0912345678001', 'Cliente', '2023-01-15 09:30:00', '0102030405060708', 'Ahorro'),
('Ana', 'Rodríguez', 'ana.rodriguez@empresa.com', HASHBYTES('SHA2_256', 'Ana123*'), '0998765432', '042111222', '0923456789001', 'Cliente', '2023-02-20 10:15:00', '2222333344445555', 'Ahorro'),
('María', 'Gómez', 'maria.gomez@empresa.com', HASHBYTES('SHA2_256', 'Maria456*'), '0991234567', NULL, '0934567890001', 'Revisor', '2023-03-10 14:20:00', '1718192021222324', 'Corriente'),
('Pedro', 'López', 'pedro.lopez@empresa.com', HASHBYTES('SHA2_256', 'Pedro456*'), '0988887777', NULL, '0945678901001', 'Revisor', '2023-04-05 11:45:00', '3333444455556666', 'Corriente'),
('Carlos', 'Ramírez', 'carlos.ramirez@soporte.com', HASHBYTES('SHA2_256', 'Carlos789*'), '0976543210', '042998877', '0956789012001', 'Tecnico', '2023-05-12 08:30:00', '2526272829303132', 'Ahorro'),
('Ana', 'Torres', 'ana.torres@soporte.com', HASHBYTES('SHA2_256', 'Ana123*'), '0988888888', '042123456', '0967890123001', 'Tecnico', '2023-06-18 13:15:00', '4142434445464748', 'Corriente'),
('Luis', 'Fernández', 'luis.fernandez@soporte.com', HASHBYTES('SHA2_256', 'Luis123*'), '0977777777', NULL, '0978901234001', 'Tecnico', '2023-07-22 16:40:00', '5758596061626364', 'Ahorro'),
('Roberto', 'Mendoza', 'roberto.mendoza@logistica.com', HASHBYTES('SHA2_256', 'Roberto123*'), '0961122334', NULL, '0989012345001', 'Personal de Entrega', '2023-08-30 09:10:00', '3334353637383940', 'Corriente'),
('Sofía', 'Castro', 'sofia.castro@logistica.com', HASHBYTES('SHA2_256', 'Sofia123*'), '0969988776', NULL, '0990123456001', 'Personal de Entrega', '2023-09-14 12:25:00', '7071727374757677', 'Ahorro'),
('Empresa XYZ', 'S.A.', 'contacto@xyz.com', HASHBYTES('SHA2_256', 'Xyz123*'), '0999999999', '022333444', '0911111111001', 'Cliente', '2023-10-05 15:30:00', '8888999900001111', 'Corriente');
GO

-- INSERTAR TOKENS DE ACCESO (con fechas explícitas)
INSERT INTO TokensDeAcceso (Token, FechaCreacion, FechaExpiracion, Vigente, Tipo_Token, FK_Usuario)
VALUES 
('token_abc123', '2024-01-10 08:30:00', '2024-01-17 08:30:00', 1, 'ResetPassword', 1),
('token_def456', '2024-01-12 14:20:00', '2024-01-19 14:20:00', 1, 'EmailVerification', 2),
('token_ghi789', '2024-01-15 10:15:00', '2024-01-22 10:15:00', 0, 'SecurityAlert', 3),
('token_jkl012', '2024-01-18 16:45:00', '2024-01-25 16:45:00', 1, 'ChangeEmail', 4),
('token_mno345', '2024-01-20 11:30:00', '2024-01-27 11:30:00', 1, 'ResetPassword', 5);
GO

-- INSERTAR MARCAS
INSERT INTO Marcas (Nombre) 
VALUES 
('Samsung'),
('LG'),
('Apple'),
('Xiaomi'),
('Sony'),
('HP'),
('Dell'),
('Lenovo'),
('Huawei'),
('Nokia');
GO

-- INSERTAR PRODUCTOS
INSERT INTO Productos (FK_Marca, Modelo, Especificacion, Dias_Garantia, Precio) 
VALUES 
(1, 'Galaxy S23', 'Smartphone 256GB, 8GB RAM, Snapdragon 8 Gen 2', 365, 999.99),
(1, 'Galaxy S21', 'Smartphone 128GB, 8GB RAM', 365, 699.99),
(1, 'QLED TV 55"', 'Televisor 4K QLED 55 pulgadas, Smart TV', 730, 899.99),
(1, 'Galaxy Tab S9', 'Tablet 11", 256GB, S-Pen incluido', 365, 799.99),
(1, 'Galaxy Watch 6', 'Smartwatch 44mm, LTE, ECG', 365, 349.99),
(1, 'Galaxy S7', 'Smartphone antiguo descontinuado', 0, 99.99),
(2, 'OLED TV 65"', 'Televisor OLED 65 pulgadas, 4K, WebOS', 730, 1499.99),
(2, 'Gram 17"', 'Laptop ultrabook 17", 16GB RAM, 1TB SSD', 365, 1699.99),
(2, 'Monitor 27" 4K', 'Monitor IPS 27", 4K UHD, HDR10', 365, 499.99),
(3, 'iPhone 15 Pro', 'Smartphone 256GB, Titanio, A17 Pro', 365, 1299.99),
(3, 'iPhone 14', 'Smartphone 256GB, A15 Bionic', 365, 999.99),
(3, 'MacBook Air M2', 'Laptop 13", 8GB RAM, 256GB SSD', 365, 1099.99),
(3, 'iPad Air 5', 'Tablet 10.9", M1, 64GB', 365, 599.99),
(3, 'Apple Watch Series 9', 'Smartwatch 45mm, GPS', 365, 429.99),
(4, 'Redmi Note 13 Pro', 'Smartphone 256GB, 12GB RAM, 200MP cámara', 180, 349.99),
(4, 'Mi TV 4K 50"', 'Televisor 4K Android TV, Dolby Vision', 365, 449.99),
(4, 'Xiaomi 13T', 'Smartphone 256GB, MediaTek Dimensity 8200', 180, 599.99),
(5, 'WH-1000XM5', 'Audífonos noise cancelling, 30h batería', 365, 399.99),
(5, 'PlayStation 5', 'Consola de videojuegos, 1TB SSD', 365, 499.99),
(5, 'Bravia XR 55"', 'Televisor OLED 55", Cognitive Processor XR', 730, 1799.99),
(6, 'Spectre x360', 'Laptop convertible 14", OLED, i7, 16GB', 365, 1299.99),
(6, 'LaserJet Pro', 'Impresora láser, WiFi, duplex', 180, 299.99),
(7, 'XPS 13', 'Laptop 13.4", i7, 16GB, 512GB SSD', 365, 1199.99),
(7, 'UltraSharp 27"', 'Monitor 4K, USB-C, color calibrado', 365, 699.99),
(8, 'ThinkPad X1 Carbon', 'Laptop 14", i7, 16GB, 1TB SSD', 365, 1599.99),
(8, 'Yoga 9i', 'Laptop convertible 14", 2.8K OLED', 365, 1399.99),
(9, 'Mate 50 Pro', 'Smartphone 256GB, cámara Leica', 180, 899.99),
(9, 'MatePad Pro', 'Tablet 12.6", HarmonyOS', 180, 799.99),
(10, 'G50', 'Smartphone 128GB, 5G, batería 5000mAh', 365, 299.99);
GO

-- INSERTAR NÚMEROS DE SERIE (primera tanda)
INSERT INTO Numero_Serie_Productos (FK_Producto, Numero_Serie, Estado_Inventario) 
VALUES 
(1, 'SM-S911BZKDEUE', 'Vendido'),
(2, 'SM-G991BZKDEUE', 'Vendido'),
(10, 'MPXJ3LL/A', 'Vendido'),
(11, 'MPXV3LL/A', 'Vendido'),
(12, 'Z15T0008W', 'Vendido'),
(2, 'SM-G991BZKDXYZ', 'Entregado_Como_Reemplazo_Al_Cliente'),
(11, 'MPXV3LL/B', 'Entregado_Como_Reemplazo_Al_Cliente'),
(2, 'SM-G991BZKVENC', 'Vendido'),
(6, 'SM-G930FDESC', 'Vendido'),
(1, 'SM-X716BZKDEUE', 'Se_Puede_Vender');
GO

-- INSERTAR VENTAS (primera tanda con fechas específicas)
INSERT INTO Ventas (Codigo_Factura, FK_Empresa_Cliente, Fecha_Compra, Total_Compra)
VALUES 
('FACT-001-2024', 1, '2024-01-15 10:30:00', 999.99),
('FACT-002-2024', 1, '2023-12-01 14:45:00', 699.99),
('FACT-003-2024', 1, '2023-12-15 11:20:00', 1299.99),
('FACT-004-2024', 1, '2023-09-20 16:10:00', 999.99),
('FACT-005-2024', 1, '2023-07-05 09:15:00', 1099.99),
('FACT-006-2024', 1, '2023-08-10 13:40:00', 699.99),
('FACT-007-2024', 1, '2023-08-25 15:30:00', 999.99),
('FACT-VENC-001', 1, '2023-01-10 10:00:00', 699.99);
GO

-- INSERTAR VENTAS POR NÚMERO DE SERIE (primera tanda)
INSERT INTO Ventas_Por_Numero_Serie_Productos (FK_Ventas, FK_Numero_Serie_Producto, Precio_Venta)
VALUES 
(1, 1, 999.99),
(2, 2, 699.99),
(3, 3, 1299.99),
(4, 4, 999.99),
(5, 5, 1099.99),
(6, 6, 699.99),
(7, 7, 999.99),
(8, 8, 699.99);
GO

-- INSERTAR CERTIFICACIONES
INSERT INTO Usuarios_Certificacion_Marcas (FK_Marca, FK_Tecnico)
VALUES 
(1, 5),
(3, 5),
(1, 6),
(3, 6),
(9, 6),
(2, 7),
(5, 7);
GO

-- INSERTAR RECLAMOS (primera tanda con fechas específicas)
INSERT INTO Reclamos (Codigo_Reclamo, FK_Empresa_Cliente, Fecha_Creacion_Reclamo)
VALUES 
('REC-20240115-ABC123', 1, '2024-01-15 14:20:00'),
('REC-20240120-DEF456', 2, '2024-01-20 10:45:00');
GO

-- INSERTAR RECLAMOS_PRODUCTO_SN (con fechas completas)
INSERT INTO Reclamos_Producto_SN (FK_Numero_Serie_Productos, FK_Reclamos, Fecha_Venta_Cliente_Final, Fecha_Reclamo_Cliente_Final, Forma_Compensacion, Estado, FK_Tecnico_Asignado, Fecha_Revision_Tecnico, Explicacion_Respuesta_Tecnico, PDF_Revision_Tecnico)
VALUES 
(1, 1, '2024-01-10 11:30:00', '2024-01-15 14:30:00', 'Reembolso', 'Aprobado', 5, '2024-01-18 09:15:00', 'Producto con falla confirmada. Se aprueba el reclamo para reembolso.', '/reclamos/tecnico/123456_20240125_informe.pdf'),
(2, 2, '2023-12-05 15:20:00', '2024-01-20 11:00:00', 'Reemplazo', 'Rechazado', 6, '2024-01-22 16:40:00', 'Producto sin fallas. Daño por mal uso del cliente.', '/reclamos/tecnico/789012_20240124_informe.pdf');
GO

-- INSERTAR MÁS NÚMEROS DE SERIE (segunda tanda)
INSERT INTO Numero_Serie_Productos (FK_Producto, Numero_Serie, Estado_Inventario) 
VALUES 
(1, 'SM-S911BZKDEU1', 'Vendido'),
(1, 'SM-S911BZKDEU2', 'Vendido'),
(2, 'SM-G991BZKDEU1', 'Vendido'),
(3, 'SN-QLED55-001', 'Vendido'),
(10, 'MPXJ3LL/B', 'Vendido'),
(11, 'MPXV3LL/C', 'Vendido'),
(12, 'Z15T0009W', 'Vendido'),
(13, 'MPL23LL/A', 'Vendido'),
(25, 'HW-MATE50-001', 'Vendido'),
(26, 'HW-MATEPAD-001', 'Vendido'),
(7, 'LG-OLED65-001', 'Vendido'),
(8, 'LG-GRAM17-001', 'Vendido'),
(16, 'SNY-WH1000-001', 'Vendido'),
(17, 'SNY-PS5-001', 'Vendido');
GO

-- INSERTAR MÁS VENTAS (segunda tanda con fechas específicas)
INSERT INTO Ventas (Codigo_Factura, FK_Empresa_Cliente, Fecha_Compra, Total_Compra)
VALUES 
('FACT-009-2024', 1, '2024-01-10 09:30:00', 999.99),
('FACT-010-2024', 1, '2024-01-15 14:20:00', 999.99),
('FACT-011-2024', 1, '2024-01-17 11:45:00', 699.99),
('FACT-012-2024', 1, '2024-01-05 16:10:00', 899.99),
('FACT-013-2024', 1, '2024-01-13 10:15:00', 1299.99),
('FACT-014-2024', 1, '2024-01-20 13:40:00', 999.99),
('FACT-015-2024', 1, '2024-01-07 15:30:00', 1099.99),
('FACT-016-2024', 1, '2024-01-22 08:45:00', 599.99),
('FACT-017-2024', 1, '2023-12-30 12:20:00', 899.99),
('FACT-018-2024', 1, '2024-01-03 17:35:00', 799.99),
('FACT-019-2024', 1, '2023-12-25 10:50:00', 1499.99),
('FACT-020-2024', 1, '2023-12-27 14:15:00', 1699.99),
('FACT-021-2024', 1, '2024-01-18 09:25:00', 399.99),
('FACT-022-2024', 1, '2024-01-11 11:55:00', 499.99);
GO

-- INSERTAR VENTAS POR NÚMERO DE SERIE (segunda tanda)
INSERT INTO Ventas_Por_Numero_Serie_Productos (FK_Ventas, FK_Numero_Serie_Producto, Precio_Venta)
VALUES 
(9, 9, 999.99),
(10, 10, 999.99),
(11, 11, 699.99),
(12, 12, 899.99),
(13, 13, 1299.99),
(14, 14, 999.99),
(15, 15, 1099.99),
(16, 16, 599.99),
(17, 17, 899.99),
(18, 18, 799.99),
(19, 19, 1499.99),
(20, 20, 1699.99),
(21, 21, 399.99),
(22, 22, 499.99);
GO

-- INSERTAR MÁS RECLAMOS (con fechas específicas)
INSERT INTO Reclamos (Codigo_Reclamo, FK_Empresa_Cliente, Fecha_Creacion_Reclamo)
VALUES 
('REC-20240125-GHI789', 1, '2024-01-25 15:30:00'),
('REC-20240126-JKL012', 1, '2024-01-26 10:45:00'),
('REC-20240127-MNO345', 1, '2024-01-27 14:20:00'),
('REC-20240128-PQR678', 1, '2024-01-28 09:15:00'),
('REC-20240129-STU901', 1, '2024-01-29 16:40:00'),
('REC-20240130-VWX234', 1, '2024-01-30 11:25:00'),
('REC-20240131-YZA567', 2, '2024-01-31 13:50:00'),
('REC-20240201-BCD890', 2, '2024-02-01 08:35:00'),
('REC-20240202-EFG123', 3, '2024-02-02 12:10:00');
GO

-- INSERTAR MÁS RECLAMOS_PRODUCTO_SN (con todas las fechas)
INSERT INTO Reclamos_Producto_SN (FK_Numero_Serie_Productos, FK_Reclamos, Fecha_Venta_Cliente_Final, Fecha_Reclamo_Cliente_Final, Forma_Compensacion, Estado, FK_Tecnico_Asignado, Fecha_Revision_Tecnico, Explicacion_Respuesta_Tecnico, PDF_Revision_Tecnico)
SELECT 9, 3, '2024-01-10 09:30:00', '2024-01-25 16:00:00', 'Reembolso', 'En Revision', 5, '2024-01-26 14:30:00', 'En proceso de revisión técnica', '/reclamos/tecnico/001_revision.pdf'
WHERE NOT EXISTS (SELECT 1 FROM Reclamos_Producto_SN WHERE FK_Numero_Serie_Productos = 9);

INSERT INTO Reclamos_Producto_SN (FK_Numero_Serie_Productos, FK_Reclamos, Fecha_Venta_Cliente_Final, Fecha_Reclamo_Cliente_Final, Forma_Compensacion, Estado, FK_Tecnico_Asignado, Fecha_Revision_Tecnico, Explicacion_Respuesta_Tecnico, PDF_Revision_Tecnico)
SELECT 13, 4, '2024-01-13 10:15:00', '2024-01-26 11:30:00', 'Reemplazo', 'Pendiente', 5, NULL, NULL, NULL
WHERE NOT EXISTS (SELECT 1 FROM Reclamos_Producto_SN WHERE FK_Numero_Serie_Productos = 13);

INSERT INTO Reclamos_Producto_SN (FK_Numero_Serie_Productos, FK_Reclamos, Fecha_Venta_Cliente_Final, Fecha_Reclamo_Cliente_Final, Forma_Compensacion, Estado, FK_Tecnico_Asignado, Fecha_Revision_Tecnico, Explicacion_Respuesta_Tecnico, PDF_Revision_Tecnico)
SELECT 14, 5, '2024-01-20 13:40:00', '2024-01-27 15:20:00', 'Reembolso', 'Pendiente', 5, NULL, NULL, NULL
WHERE NOT EXISTS (SELECT 1 FROM Reclamos_Producto_SN WHERE FK_Numero_Serie_Productos = 14);

INSERT INTO Reclamos_Producto_SN (FK_Numero_Serie_Productos, FK_Reclamos, Fecha_Venta_Cliente_Final, Fecha_Reclamo_Cliente_Final, Forma_Compensacion, Estado, FK_Tecnico_Asignado, Fecha_Revision_Tecnico, Explicacion_Respuesta_Tecnico, PDF_Revision_Tecnico)
SELECT 15, 6, '2024-01-07 15:30:00', '2024-01-28 10:45:00', 'Reemplazo', 'Pendiente', 5, NULL, NULL, NULL
WHERE NOT EXISTS (SELECT 1 FROM Reclamos_Producto_SN WHERE FK_Numero_Serie_Productos = 15);

INSERT INTO Reclamos_Producto_SN (FK_Numero_Serie_Productos, FK_Reclamos, Fecha_Venta_Cliente_Final, Fecha_Reclamo_Cliente_Final, Forma_Compensacion, Estado, FK_Tecnico_Asignado, Fecha_Revision_Tecnico, Explicacion_Respuesta_Tecnico, PDF_Revision_Tecnico)
SELECT 10, 7, '2024-01-15 14:20:00', '2024-01-29 14:30:00', 'Reemplazo', 'En Revision', 6, '2024-01-30 09:15:00', 'Revisión en curso', '/reclamos/tecnico/002_revision.pdf'
WHERE NOT EXISTS (SELECT 1 FROM Reclamos_Producto_SN WHERE FK_Numero_Serie_Productos = 10);

INSERT INTO Reclamos_Producto_SN (FK_Numero_Serie_Productos, FK_Reclamos, Fecha_Venta_Cliente_Final, Fecha_Reclamo_Cliente_Final, Forma_Compensacion, Estado, FK_Tecnico_Asignado, Fecha_Revision_Tecnico, Explicacion_Respuesta_Tecnico, PDF_Revision_Tecnico)
SELECT 16, 8, '2024-01-22 08:45:00', '2024-01-30 16:20:00', 'Reembolso', 'Pendiente', 6, NULL, NULL, NULL
WHERE NOT EXISTS (SELECT 1 FROM Reclamos_Producto_SN WHERE FK_Numero_Serie_Productos = 16);

INSERT INTO Reclamos_Producto_SN (FK_Numero_Serie_Productos, FK_Reclamos, Fecha_Venta_Cliente_Final, Fecha_Reclamo_Cliente_Final, Forma_Compensacion, Estado, FK_Tecnico_Asignado, Fecha_Revision_Tecnico, Explicacion_Respuesta_Tecnico, PDF_Revision_Tecnico)
SELECT 17, 9, '2023-12-30 12:20:00', '2024-01-31 11:10:00', 'Reemplazo', 'Pendiente', 6, NULL, NULL, NULL
WHERE NOT EXISTS (SELECT 1 FROM Reclamos_Producto_SN WHERE FK_Numero_Serie_Productos = 17);

INSERT INTO Reclamos_Producto_SN (FK_Numero_Serie_Productos, FK_Reclamos, Fecha_Venta_Cliente_Final, Fecha_Reclamo_Cliente_Final, Forma_Compensacion, Estado, FK_Tecnico_Asignado, Fecha_Revision_Tecnico, Explicacion_Respuesta_Tecnico, PDF_Revision_Tecnico)
SELECT 18, 10, '2024-01-03 17:35:00', '2024-02-01 09:45:00', 'Reembolso', 'Pendiente', 6, NULL, NULL, NULL
WHERE NOT EXISTS (SELECT 1 FROM Reclamos_Producto_SN WHERE FK_Numero_Serie_Productos = 18);

INSERT INTO Reclamos_Producto_SN (FK_Numero_Serie_Productos, FK_Reclamos, Fecha_Venta_Cliente_Final, Fecha_Reclamo_Cliente_Final, Forma_Compensacion, Estado, FK_Tecnico_Asignado, Fecha_Revision_Tecnico, Explicacion_Respuesta_Tecnico, PDF_Revision_Tecnico)
SELECT 19, 11, '2023-12-25 10:50:00', '2024-02-02 13:25:00', 'Reemplazo', 'En Revision', 7, '2024-02-03 10:40:00', 'Análisis técnico iniciado', '/reclamos/tecnico/003_revision.pdf'
WHERE NOT EXISTS (SELECT 1 FROM Reclamos_Producto_SN WHERE FK_Numero_Serie_Productos = 19);

INSERT INTO Reclamos_Producto_SN (FK_Numero_Serie_Productos, FK_Reclamos, Fecha_Venta_Cliente_Final, Fecha_Reclamo_Cliente_Final, Forma_Compensacion, Estado, FK_Tecnico_Asignado, Fecha_Revision_Tecnico, Explicacion_Respuesta_Tecnico, PDF_Revision_Tecnico)
SELECT 20, 3, '2023-12-27 14:15:00', '2024-01-25 16:30:00', 'Reembolso', 'Pendiente', 7, NULL, NULL, NULL
WHERE NOT EXISTS (SELECT 1 FROM Reclamos_Producto_SN WHERE FK_Numero_Serie_Productos = 20);

INSERT INTO Reclamos_Producto_SN (FK_Numero_Serie_Productos, FK_Reclamos, Fecha_Venta_Cliente_Final, Fecha_Reclamo_Cliente_Final, Forma_Compensacion, Estado, FK_Tecnico_Asignado, Fecha_Revision_Tecnico, Explicacion_Respuesta_Tecnico, PDF_Revision_Tecnico)
SELECT 21, 4, '2024-01-18 09:25:00', '2024-01-26 12:15:00', 'Reemplazo', 'Pendiente', 7, NULL, NULL, NULL
WHERE NOT EXISTS (SELECT 1 FROM Reclamos_Producto_SN WHERE FK_Numero_Serie_Productos = 21);

INSERT INTO Reclamos_Producto_SN (FK_Numero_Serie_Productos, FK_Reclamos, Fecha_Venta_Cliente_Final, Fecha_Reclamo_Cliente_Final, Forma_Compensacion, Estado, FK_Tecnico_Asignado, Fecha_Revision_Tecnico, Explicacion_Respuesta_Tecnico, PDF_Revision_Tecnico)
SELECT 22, 5, '2024-01-11 11:55:00', '2024-01-27 17:40:00', 'Reembolso', 'Pendiente', 7, NULL, NULL, NULL
WHERE NOT EXISTS (SELECT 1 FROM Reclamos_Producto_SN WHERE FK_Numero_Serie_Productos = 22);
GO

-- INSERTAR PRODUCTOS PARA REEMPLAZO
INSERT INTO Numero_Serie_Productos (FK_Producto, Numero_Serie, Estado_Inventario) 
VALUES 
(1, 'SM-S911BZKD-REP01', 'Se_Puede_Vender'),
(1, 'SM-S911BZKD-REP02', 'Se_Puede_Vender'),
(1, 'SM-S911BZKD-REP03', 'Se_Puede_Vender'),
(2, 'SM-G991BZKD-REP01', 'Se_Puede_Vender'),
(2, 'SM-G991BZKD-REP02', 'Se_Puede_Vender'),
(10, 'MPXJ3LL-REP01', 'Se_Puede_Vender'),
(10, 'MPXJ3LL-REP02', 'Se_Puede_Vender'),
(11, 'MPXV3LL-REP01', 'Se_Puede_Vender'),
(11, 'MPXV3LL-REP02', 'Se_Puede_Vender'),
(12, 'Z15T0009-REP01', 'Se_Puede_Vender'),
(12, 'Z15T0009-REP02', 'Se_Puede_Vender');
GO

-- INSERTAR MARCA LO ENTREGÓ COMO REEMPLAZO
INSERT INTO Marca_Lo_Entrego_Como_Reemplazo (FK_Numero_Serie_Productos)
VALUES 
(6),
(7);
GO

-- INSERTAR RECLAMOS PARA ENTREGA (con fechas específicas)
INSERT INTO Reclamos (Codigo_Reclamo, FK_Empresa_Cliente, Fecha_Creacion_Reclamo)
VALUES 
('REC-ENTREGA-001', 1, '2024-01-20 14:30:00'),
('REC-ENTREGA-002', 2, '2024-01-22 10:45:00'),
('REC-ENTREGA-003', 3, '2024-01-24 16:20:00');
GO

-- INSERTAR RECLAMOS_PRODUCTO_SN PARA ENTREGA (con todas las fechas)
INSERT INTO Reclamos_Producto_SN (FK_Numero_Serie_Productos, FK_Reclamos, Fecha_Venta_Cliente_Final, Fecha_Reclamo_Cliente_Final, Forma_Compensacion, Estado, FK_Tecnico_Asignado, Fecha_Revision_Tecnico, Explicacion_Respuesta_Tecnico, PDF_Revision_Tecnico)
SELECT 1, (SELECT Id FROM Reclamos WHERE Codigo_Reclamo = 'REC-ENTREGA-001'), '2024-01-10 11:30:00', '2024-01-20 15:00:00', 'Reemplazo', 'Aprobado', 5, '2024-01-21 09:15:00', 'Producto con falla confirmada. Se aprueba reemplazo.', '/reclamos/tecnico/entrega_001.pdf'
WHERE NOT EXISTS (SELECT 1 FROM Reclamos_Producto_SN WHERE FK_Numero_Serie_Productos = 1);

INSERT INTO Reclamos_Producto_SN (FK_Numero_Serie_Productos, FK_Reclamos, Fecha_Venta_Cliente_Final, Fecha_Reclamo_Cliente_Final, Forma_Compensacion, Estado, FK_Tecnico_Asignado, Fecha_Revision_Tecnico, Explicacion_Respuesta_Tecnico, PDF_Revision_Tecnico)
SELECT 2, (SELECT Id FROM Reclamos WHERE Codigo_Reclamo = 'REC-ENTREGA-001'), '2023-12-05 15:20:00', '2024-01-20 16:30:00', 'Reemplazo', 'Aprobado', 5, '2024-01-21 10:30:00', 'Producto con falla confirmada. Se aprueba reemplazo.', '/reclamos/tecnico/entrega_002.pdf'
WHERE NOT EXISTS (SELECT 1 FROM Reclamos_Producto_SN WHERE FK_Numero_Serie_Productos = 2);

INSERT INTO Reclamos_Producto_SN (FK_Numero_Serie_Productos, FK_Reclamos, Fecha_Venta_Cliente_Final, Fecha_Reclamo_Cliente_Final, Forma_Compensacion, Estado, FK_Tecnico_Asignado, Fecha_Revision_Tecnico, Explicacion_Respuesta_Tecnico, PDF_Revision_Tecnico)
SELECT 3, (SELECT Id FROM Reclamos WHERE Codigo_Reclamo = 'REC-ENTREGA-001'), '2023-12-15 11:20:00', '2024-01-20 14:45:00', 'Reemplazo', 'Aprobado', 5, '2024-01-21 11:45:00', 'Producto con falla confirmada. Se aprueba reemplazo.', '/reclamos/tecnico/entrega_003.pdf'
WHERE NOT EXISTS (SELECT 1 FROM Reclamos_Producto_SN WHERE FK_Numero_Serie_Productos = 3);

INSERT INTO Reclamos_Producto_SN (FK_Numero_Serie_Productos, FK_Reclamos, Fecha_Venta_Cliente_Final, Fecha_Reclamo_Cliente_Final, Forma_Compensacion, Estado, FK_Tecnico_Asignado, Fecha_Revision_Tecnico, Explicacion_Respuesta_Tecnico, PDF_Revision_Tecnico)
SELECT 4, (SELECT Id FROM Reclamos WHERE Codigo_Reclamo = 'REC-ENTREGA-002'), '2023-09-20 16:10:00', '2024-01-22 11:15:00', 'Reemplazo', 'Aprobado', 6, '2024-01-23 14:20:00', 'Producto defectuoso. Aprobado para reemplazo.', '/reclamos/tecnico/entrega_004.pdf'
WHERE NOT EXISTS (SELECT 1 FROM Reclamos_Producto_SN WHERE FK_Numero_Serie_Productos = 4);

INSERT INTO Reclamos_Producto_SN (FK_Numero_Serie_Productos, FK_Reclamos, Fecha_Venta_Cliente_Final, Fecha_Reclamo_Cliente_Final, Forma_Compensacion, Estado, FK_Tecnico_Asignado, Fecha_Revision_Tecnico, Explicacion_Respuesta_Tecnico, PDF_Revision_Tecnico)
SELECT 5, (SELECT Id FROM Reclamos WHERE Codigo_Reclamo = 'REC-ENTREGA-002'), '2023-07-05 09:15:00', '2024-01-22 10:30:00', 'Reemplazo', 'Aprobado', 6, '2024-01-23 15:35:00', 'Producto defectuoso. Aprobado para reemplazo.', '/reclamos/tecnico/entrega_005.pdf'
WHERE NOT EXISTS (SELECT 1 FROM Reclamos_Producto_SN WHERE FK_Numero_Serie_Productos = 5);

INSERT INTO Reclamos_Producto_SN (FK_Numero_Serie_Productos, FK_Reclamos, Fecha_Venta_Cliente_Final, Fecha_Reclamo_Cliente_Final, Forma_Compensacion, Estado, FK_Tecnico_Asignado, Fecha_Revision_Tecnico, Explicacion_Respuesta_Tecnico, PDF_Revision_Tecnico)
SELECT 6, (SELECT Id FROM Reclamos WHERE Codigo_Reclamo = 'REC-ENTREGA-003'), '2023-08-10 13:40:00', '2024-01-24 17:10:00', 'Reemplazo', 'Aprobado', 7, '2024-01-25 08:45:00', 'Falla de fábrica. Se autoriza reemplazo.', '/reclamos/tecnico/entrega_006.pdf'
WHERE NOT EXISTS (SELECT 1 FROM Reclamos_Producto_SN WHERE FK_Numero_Serie_Productos = 6);

INSERT INTO Reclamos_Producto_SN (FK_Numero_Serie_Productos, FK_Reclamos, Fecha_Venta_Cliente_Final, Fecha_Reclamo_Cliente_Final, Forma_Compensacion, Estado, FK_Tecnico_Asignado, Fecha_Revision_Tecnico, Explicacion_Respuesta_Tecnico, PDF_Revision_Tecnico)
SELECT 7, (SELECT Id FROM Reclamos WHERE Codigo_Reclamo = 'REC-ENTREGA-003'), '2023-08-25 15:30:00', '2024-01-24 16:45:00', 'Reemplazo', 'Aprobado', 7, '2024-01-25 09:30:00', 'Falla de fábrica. Se autoriza reemplazo.', '/reclamos/tecnico/entrega_007.pdf'
WHERE NOT EXISTS (SELECT 1 FROM Reclamos_Producto_SN WHERE FK_Numero_Serie_Productos = 7);

INSERT INTO Reclamos_Producto_SN (FK_Numero_Serie_Productos, FK_Reclamos, Fecha_Venta_Cliente_Final, Fecha_Reclamo_Cliente_Final, Forma_Compensacion, Estado, FK_Tecnico_Asignado, Fecha_Revision_Tecnico, Explicacion_Respuesta_Tecnico, PDF_Revision_Tecnico)
SELECT 8, (SELECT Id FROM Reclamos WHERE Codigo_Reclamo = 'REC-ENTREGA-001'), '2023-01-10 10:00:00', '2024-01-20 14:15:00', 'Reemplazo', 'En Revision', 5, '2024-01-25 10:00:00', 'Revisión técnica en proceso', '/reclamos/tecnico/entrega_008.pdf'
WHERE NOT EXISTS (SELECT 1 FROM Reclamos_Producto_SN WHERE FK_Numero_Serie_Productos = 8);
GO

-- INSERTAR MÁS RECLAMOS PARA CLIENTE (con fechas específicas)
INSERT INTO Reclamos (Codigo_Reclamo, FK_Empresa_Cliente, Fecha_Creacion_Reclamo)
VALUES 
('REC-CLIENTE-001', 1, '2024-01-23 11:30:00'),
('REC-CLIENTE-002', 1, '2024-01-20 14:45:00'),
('REC-CLIENTE-003', 1, '2024-01-15 09:20:00'),
('REC-CLIENTE-004', 1, '2024-01-10 16:10:00'),
('REC-CLIENTE-005', 1, '2024-01-05 13:25:00');
GO

-- INSERTAR RECLAMOS_PRODUCTO_SN PARA CLIENTE (con todas las fechas)
INSERT INTO Reclamos_Producto_SN (FK_Numero_Serie_Productos, FK_Reclamos, Fecha_Venta_Cliente_Final, Fecha_Reclamo_Cliente_Final, Forma_Compensacion, Estado, FK_Tecnico_Asignado, Fecha_Revision_Tecnico, Explicacion_Respuesta_Tecnico, PDF_Revision_Tecnico)
SELECT 9, (SELECT Id FROM Reclamos WHERE Codigo_Reclamo = 'REC-CLIENTE-001'), '2024-01-10 09:30:00', '2024-01-23 12:00:00', 'Reembolso', 'Pendiente', 5, NULL, NULL, NULL
WHERE NOT EXISTS (SELECT 1 FROM Reclamos_Producto_SN WHERE FK_Numero_Serie_Productos = 9);

INSERT INTO Reclamos_Producto_SN (FK_Numero_Serie_Productos, FK_Reclamos, Fecha_Venta_Cliente_Final, Fecha_Reclamo_Cliente_Final, Forma_Compensacion, Estado, FK_Tecnico_Asignado, Fecha_Revision_Tecnico, Explicacion_Respuesta_Tecnico, PDF_Revision_Tecnico)
SELECT 11, (SELECT Id FROM Reclamos WHERE Codigo_Reclamo = 'REC-CLIENTE-001'), '2024-01-17 11:45:00', '2024-01-23 12:30:00', 'Reemplazo', 'Pendiente', 5, NULL, NULL, NULL
WHERE NOT EXISTS (SELECT 1 FROM Reclamos_Producto_SN WHERE FK_Numero_Serie_Productos = 11);

INSERT INTO Reclamos_Producto_SN (FK_Numero_Serie_Productos, FK_Reclamos, Fecha_Venta_Cliente_Final, Fecha_Reclamo_Cliente_Final, Forma_Compensacion, Estado, FK_Tecnico_Asignado, Fecha_Revision_Tecnico, Explicacion_Respuesta_Tecnico, PDF_Revision_Tecnico)
SELECT 13, (SELECT Id FROM Reclamos WHERE Codigo_Reclamo = 'REC-CLIENTE-002'), '2024-01-13 10:15:00', '2024-01-20 15:45:00', 'Reemplazo', 'En Revision', 6, '2024-01-25 14:30:00', 'Producto en proceso de revisión técnica', '/reclamos/tecnico/cliente_001_revision.pdf'
WHERE NOT EXISTS (SELECT 1 FROM Reclamos_Producto_SN WHERE FK_Numero_Serie_Productos = 13);

INSERT INTO Reclamos_Producto_SN (FK_Numero_Serie_Productos, FK_Reclamos, Fecha_Venta_Cliente_Final, Fecha_Reclamo_Cliente_Final, Forma_Compensacion, Estado, FK_Tecnico_Asignado, Fecha_Revision_Tecnico, Explicacion_Respuesta_Tecnico, PDF_Revision_Tecnico)
SELECT 14, (SELECT Id FROM Reclamos WHERE Codigo_Reclamo = 'REC-CLIENTE-002'), '2024-01-20 13:40:00', '2024-01-20 16:20:00', 'Reemplazo', 'En Revision', 6, '2024-01-25 15:15:00', 'Producto en proceso de revisión técnica', '/reclamos/tecnico/cliente_001_revision.pdf'
WHERE NOT EXISTS (SELECT 1 FROM Reclamos_Producto_SN WHERE FK_Numero_Serie_Productos = 14);

INSERT INTO Reclamos_Producto_SN (FK_Numero_Serie_Productos, FK_Reclamos, Fecha_Venta_Cliente_Final, Fecha_Reclamo_Cliente_Final, Forma_Compensacion, Estado, FK_Tecnico_Asignado, Fecha_Revision_Tecnico, Explicacion_Respuesta_Tecnico, PDF_Revision_Tecnico)
SELECT 15, (SELECT Id FROM Reclamos WHERE Codigo_Reclamo = 'REC-CLIENTE-003'), '2024-01-07 15:30:00', '2024-01-15 10:30:00', 'Reembolso', 'Compensado', 5, '2024-01-16 11:45:00', 'Producto con falla confirmada. Se aprueba reembolso total.', '/reclamos/tecnico/cliente_002_aprobado.pdf'
WHERE NOT EXISTS (SELECT 1 FROM Reclamos_Producto_SN WHERE FK_Numero_Serie_Productos = 15);

INSERT INTO Reclamos_Producto_SN (FK_Numero_Serie_Productos, FK_Reclamos, Fecha_Venta_Cliente_Final, Fecha_Reclamo_Cliente_Final, Forma_Compensacion, Estado, FK_Tecnico_Asignado, Fecha_Revision_Tecnico, Explicacion_Respuesta_Tecnico, PDF_Revision_Tecnico)
SELECT 16, (SELECT Id FROM Reclamos WHERE Codigo_Reclamo = 'REC-CLIENTE-004'), '2024-01-22 08:45:00', '2024-01-10 17:10:00', 'Reemplazo', 'Compensado', 6, '2024-01-12 09:25:00', 'Producto defectuoso. Se aprueba reemplazo.', '/reclamos/tecnico/cliente_003_reemplazo.pdf'
WHERE NOT EXISTS (SELECT 1 FROM Reclamos_Producto_SN WHERE FK_Numero_Serie_Productos = 16);

INSERT INTO Reclamos_Producto_SN (FK_Numero_Serie_Productos, FK_Reclamos, Fecha_Venta_Cliente_Final, Fecha_Reclamo_Cliente_Final, Forma_Compensacion, Estado, FK_Tecnico_Asignado, Fecha_Revision_Tecnico, Explicacion_Respuesta_Tecnico, PDF_Revision_Tecnico)
SELECT 8, (SELECT Id FROM Reclamos WHERE Codigo_Reclamo = 'REC-CLIENTE-005'), '2023-01-10 10:00:00', '2024-01-05 14:15:00', 'Reembolso', 'Rechazado', 5, '2024-01-08 16:40:00', 'Producto sin fallas. Daño por mal uso del cliente.', '/reclamos/tecnico/cliente_004_rechazado.pdf'
WHERE NOT EXISTS (SELECT 1 FROM Reclamos_Producto_SN WHERE FK_Numero_Serie_Productos = 8);
GO

-- INSERTAR REEMBOLSO (con fecha específica)
INSERT INTO Reembolso (Numero_Comprobante_Reembolso, Fecha_Reembolso, Num_Cuenta_Bancaria_Reembolso)
VALUES ('REMB-001-2024', '2024-01-17 10:30:00', '0102030405060708');
GO

-- INSERTAR REEMBOLSO_POR_RECLAMOS
INSERT INTO Reembolso_Por_Reclamos (FK_Reclamos_Producto_SN, FK_Reembolso)
SELECT RPS.Id, (SELECT Id FROM Reembolso WHERE Numero_Comprobante_Reembolso = 'REMB-001-2024')
FROM Reclamos_Producto_SN RPS
INNER JOIN Reclamos R ON RPS.FK_Reclamos = R.Id
WHERE R.Codigo_Reclamo = 'REC-CLIENTE-003'
AND RPS.FK_Numero_Serie_Productos = 15;
GO

-- INSERTAR COMPROBANTE_DE_REEMPLAZO (con fecha implícita a través de GETDATE())
INSERT INTO Comprobante_De_Reemplazo (PDF_Comprobante_Entrega_Cliente, FK_Personal_Entrega, Estado)
VALUES ('/entrega/comprobante_cliente_001.pdf', 8, 'Completado');
GO

-- INSERTAR COMPROBANTE_PRODUCTO_REEMPLAZADO
INSERT INTO Comprobante_Producto_Reemplazado (FK_Reclamos_Producto_SN, FK_Comprobante_De_Reemplazo, FK_Producto_De_Reemplazo)
SELECT RPS.Id, (SELECT Id FROM Comprobante_De_Reemplazo WHERE PDF_Comprobante_Entrega_Cliente = '/entrega/comprobante_cliente_001.pdf'), 23
FROM Reclamos_Producto_SN RPS
INNER JOIN Reclamos R ON RPS.FK_Reclamos = R.Id
WHERE R.Codigo_Reclamo = 'REC-CLIENTE-004'
AND RPS.FK_Numero_Serie_Productos = 16;
GO