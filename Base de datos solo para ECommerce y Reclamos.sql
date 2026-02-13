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
    Razon_Social VARCHAR(200) NULL,
    Tipo_Identificacion VARCHAR(20) CHECK (Tipo_Identificacion IN ('Cedula', 'Pasaporte')),
    Identificacion VARCHAR(13) UNIQUE NOT NULL,
    RUC VARCHAR(13) UNIQUE NULL,
    Correo VARCHAR(100) UNIQUE NOT NULL,
    Contrasena VARBINARY(256) NOT NULL,
    Celular VARCHAR(15) NOT NULL,
    Convencional VARCHAR(15),
    Pais VARCHAR(50), --Ecuador, USA
    Division_administrativa VARCHAR(50), --Guayas, California
    Ciudad VARCHAR(50), --Guayaquil, Los Angeles
    Codigo_Postal VARCHAR(20), --codigo
    Direccion VARCHAR(500), --direccion domiciliaria
    Rol VARCHAR(50) NOT NULL CHECK (
        Rol IN (
            'Cliente', --Cliente Persona Natural segun el SRI, es aquella persona comun que compra para si mismo o es un emprendedor. 
            --Puede crear su propia cuenta desde cero o partiendo de datos que ya se halla guardado de este al momento de compras pasadas.
            'Revisor', --Puede crear reclamos de productos defectuoso, 
            -- este tambien puede probar los productos nuevos que esta comprando el usuario para ver su funcionamiento. Funciona como atencion al cliente
            'Tecnico', --Revisa los productos defectuosos y aprueba a reprueba individualmente cada producto unicamente de marcas que este este certificado
            'Personal de Entrega', --Es el que entrega los productos de reemplazo al cliente
            'Cliente_Juridico', --Cliente Persona Juridico segun el SRI, es una empresa grande que compra volumenes mayores que los clientes Persona Natural y
            -- como obtiene mayores ganancias se le exige evidiencia valida que demuestre su crecimiento totalmente licito y legal. 
            --Puede crear su propia cuenta desde cero o partiendo de datos que ya se halla guardado de este al momento de compras pasadas.
            'Vendedor', --Es el que vende los productos en cualquiera de nuestros puntos de venta fisico
            'Analista_Datos', -- Puede ver mucha informacion confidencial ya que se encarga de analizar absolutamente todo 
            -- de como le esta yendo a nuestra empresa
            'Encargado_Inventario', --Es el encargado de recibir todos los productos de los distintos proveedor que agregar su informacion, 
            -- stock, ubicacion, etc.
            'Gestor_Productos', --Es el encargado de solicitar los productos que se estan necesitando comprando, ya que muchos clientes lo solicitan 
            -- o hay poco stock de estos
            'Administrador' --controla todo a nivel administrativo, puede crear cuentas de todos los empleados, ver informacion confidencial
        )
    ),
    Fecha_Creacion DATETIME DEFAULT GETDATE() NOT NULL,
    Num_Cuenta_Bancaria VARCHAR(30) NULL DEFAULT NULL, 
    Tipo_Cuenta_Bancaria VARCHAR(20) NULL CHECK(Tipo_Cuenta_Bancaria IN ('Ahorro', 'Corriente')),
    Activo BIT DEFAULT 1 NOT NULL,
    Contribuyente_Especial BIT DEFAULT 0 NOT NULL,
    Obligado_Contabilidad BIT DEFAULT 0 NOT NULL,
    Creado_Por INT REFERENCES Usuarios(Id)
);
GO

-- ============================================
-- TABLAS E-COMMERCE
-- ============================================
CREATE TABLE Categorias(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Nombre VARCHAR(100) NOT NULL,
    Descripcion VARCHAR(500),
    Activo BIT DEFAULT 1,
    FK_Categoria_Padre INT REFERENCES Categorias(Id),
    Fecha_Creacion DATETIME DEFAULT GETDATE()
);
GO

 CREATE TABLE Proveedores(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Nombre VARCHAR(100) NOT NULL,
    Cedula VARCHAR(10) UNIQUE NOT NULL,
    RUC VARCHAR(13) UNIQUE NOT NULL,
    Direccion VARCHAR(500),
    Telefono VARCHAR(15),
    Email VARCHAR(100),
    Contacto_Principal VARCHAR(100),
    Plazo_Entrega_Dias INT DEFAULT 7,
    Activo BIT DEFAULT 1,
    Fecha_Creacion DATETIME DEFAULT GETDATE()
 );

 -- Tabla Métodos de Pago
CREATE TABLE Metodos_Pago(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Tipo VARCHAR(20) UNIQUE NOT NULL CHECK (
        Tipo IN ('Efectivo', 'Debito', 'Credito', 'Payphone', 'Transferencia')
    ),    
    Descripcion VARCHAR(200),
    Activo BIT DEFAULT 1,
    Requiere_Confirmacion BIT DEFAULT 0,
    Comision_Porcentaje DECIMAL(5,2) DEFAULT 0
);
GO

-- Tabla Configuración SRI
CREATE TABLE Configuracion_SRI(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Ambiente VARCHAR(20) DEFAULT 'Pruebas' CHECK (Ambiente IN ('Pruebas', 'Produccion')),
    Token_Acceso VARCHAR(500),
    Fecha_Expiracion_Token DATETIME
);

-- Tabla Configuración Empresa
CREATE TABLE Datos_Empresa(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    RUC_Empresa VARCHAR(13) NOT NULL,
    Nombre_Comercial VARCHAR(300) NOT NULL,
    Razon_Social VARCHAR(300) NOT NULL,
    Direccion_Matriz VARCHAR(500) NOT NULL,
);

-- ============================================
-- TABLAS RECLAMOS
-- ============================================

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
 
-- Tabla Productos (ampliada para e-commerce)
CREATE TABLE Productos(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    SKU VARCHAR(50) UNIQUE NOT NULL,
    Peso_kg DECIMAL(12,4), -- en kg
    Alto_cm DECIMAL(8,2) NOT NULL, --en cm
    Ancho_cm DECIMAL(8,2) NOT NULL, --en cm
    Profundidad_cm DECIMAL(8,2) NOT NULL, --en cm
    Visibilidad VARCHAR(20) DEFAULT 'Publico' 
        CHECK (Visibilidad IN ('Publico', 'Privado', 'Oculta')),
    Codigo VARCHAR(50) UNIQUE NOT NULL,
    FK_Marca INT REFERENCES Marcas(Id) NOT NULL,
    FK_Categoria INT REFERENCES Categorias(Id),
    Modelo VARCHAR(50) NOT NULL,
    Especificacion VARCHAR(MAX) NOT NULL,
    Descripcion VARCHAR(2000),
    Activo BIT DEFAULT 1,
    Imagen_URL VARCHAR(500),
    Fecha_Creacion DATETIME DEFAULT GETDATE(),
    --Normalmente los productos van a tener garantia de 3 dias si es extremamente barato y generico, con costos de poquisimos dolares.
	--pero si tiene 0 es porque ese producto esta descontinuado por la marca y no se ofrece garantia alguna
    Dias_Garantia INT NOT NULL CHECK (Dias_Garantia >= 0),
    Precio DECIMAL(12,2) NOT NULL CHECK (Precio > 0),
    UNIQUE(FK_Marca, Modelo), -- Mismo modelo no puede repetirse en misma marca
    Creado_Por INT REFERENCES Usuarios(Id),
    Modificado_Por INT REFERENCES Usuarios(Id)
);
GO

-- Tabla Inventario Movimientos
CREATE TABLE Inventario_Movimientos(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    FK_Producto INT REFERENCES Productos(Id) NOT NULL,
    FK_Usuario INT REFERENCES Usuarios(Id),
    Tipo_Movimiento VARCHAR(20) NOT NULL CHECK (Tipo_Movimiento IN ('Entrada', 'Salida', 'Ajuste', 'Devolucion')),
    Cantidad INT NOT NULL,
    Cantidad_Anterior INT NOT NULL,
    Cantidad_Nueva INT NOT NULL,
    Motivo VARCHAR(200),
    Referencia VARCHAR(100),
    Fecha_Movimiento DATETIME DEFAULT GETDATE(),
    Costo_Unitario DECIMAL(12,2)
);
GO

CREATE TABLE Inventario_Ubicaciones (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Codigo VARCHAR(20) UNIQUE NOT NULL,
    Nombre VARCHAR(100) NOT NULL,
    Tipo VARCHAR(20) CHECK (Tipo IN ('Bodega', 'Estante', 'Pasillo', 'Caja')),
    FK_Ubicacion_Padre INT REFERENCES Inventario_Ubicaciones(Id),
    Capacidad_Maxima INT,
    Activo BIT DEFAULT 1
);

CREATE TABLE Numero_Serie_Productos(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    FK_Producto INT REFERENCES Productos(Id) NOT NULL,
    Numero_Serie VARCHAR(50) UNIQUE NOT NULL,
    Estado_Inventario VARCHAR(50) NOT NULL CHECK (Estado_Inventario IN 
    ('Se_Puede_Vender', 'Vendido', 'Entregado_Como_Reemplazo_Al_Cliente', 'Recibido_Del_Cliente_Por_Defecto_De_Fabrica')),
    -- Si se puede vender entonces puedes venderlo
	-- Si esta vendido, Entregado_Como_Reemplazo_Al_Cliente o Recibido_Del_Cliente_Por_Defecto_De_Fabrica no se lo puede vender
	-- Si esta vendido, Entregado_Como_Reemplazo_Al_Cliente se puede hacer un reclamo
	-- Si esta Recibido_Del_Cliente_Por_Defecto_De_Fabrica ya no se puede ni Vender, ni Reclamar, no se puede hacer nada, ya pasa a ser un producto no disponible que la marca oficial me tiene que reemplazar por otro nuevo
    Fecha_Ingreso DATETIME DEFAULT GETDATE(),
    FK_Proveedor INT REFERENCES Proveedores(Id) NOT NULL,
    FK_Ubicacion INT REFERENCES Inventario_Ubicaciones(Id)
);
GO

CREATE TABLE Marca_Lo_Entrego_Como_Reemplazo(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    FK_Numero_Serie_Productos INT REFERENCES Numero_Serie_Productos(Id) NOT NULL
);
GO

CREATE TABLE Ventas(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Codigo_Factura VARCHAR(50) UNIQUE NOT NULL,
    FK_Empresa_Cliente INT REFERENCES Usuarios(Id) NOT NULL, --Es cualquier cliente ya sea natural o juridico
    FK_Vendedor INT REFERENCES Usuarios(Id),
    Tipo_Venta VARCHAR(20) DEFAULT 'Contado' CHECK (Tipo_Venta IN ('Contado', 'Credito')),
    Fecha_Compra DATETIME DEFAULT GETDATE(),
    Estado_SRI VARCHAR(20) DEFAULT 'Pendiente' CHECK (Estado_SRI IN ('Pendiente', 'Autorizado', 'Rechazado', 'Anulado')),
    Clave_Acceso VARCHAR(100),
    Numero_Autorizacion VARCHAR(100),
    Fecha_Autorizacion DATETIME,
    XML_Path VARCHAR(500),
    PDF_Path VARCHAR(500),
    Observaciones VARCHAR(1000),
    Direccion_Entrega VARCHAR(500),
    Telefono_Contacto VARCHAR(15),
    Total_Compra DECIMAL(12,2) NOT NULL CHECK (Total_Compra > 0),
    Creado_Por INT REFERENCES Usuarios(Id),
    Fecha_Modificacion DATETIME,
    Modificado_Por INT REFERENCES Usuarios(Id)
);
GO

-- Tabla Pagos
CREATE TABLE Pagos(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    FK_Venta INT REFERENCES Ventas(Id) NOT NULL,
    FK_Metodo_Pago INT REFERENCES Metodos_Pago(Id) NOT NULL,
    Estado VARCHAR(20) DEFAULT 'Pendiente' CHECK (Estado IN ('Pendiente', 'Procesado', 'Completo', 'Fallido', 'Reembolsado')),
    Monto DECIMAL(12,2) NOT NULL,
    Referencia VARCHAR(100),
    Fecha_Pago DATETIME DEFAULT GETDATE(),
    Datos_Transaccion VARCHAR(1000),
    Terminal_PuntoVenta VARCHAR(50),
    Cuotas INT DEFAULT 1,
    Monto_Cuota DECIMAL(12,2)
);
GO

CREATE TABLE Ventas_Por_Numero_Serie_Productos(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    FK_Ventas INT REFERENCES Ventas(Id) NOT NULL,
    FK_Numero_Serie_Producto INT REFERENCES Numero_Serie_Productos(Id) NOT NULL UNIQUE,
    Precio_Venta DECIMAL(12,2) NOT NULL CHECK (Precio_Venta >= 0),
    Descuento DECIMAL(12,2) DEFAULT 0,
    IVA DECIMAL(12,2) NOT NULL
);
GO

CREATE TABLE Reclamos(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Codigo_Reclamo VARCHAR(50) UNIQUE NOT NULL,
    FK_Empresa_Cliente INT REFERENCES Usuarios(Id) NOT NULL,
    Fecha_Creacion_Reclamo DATETIME DEFAULT GETDATE()
);
GO

CREATE TABLE Reclamos_Producto_SN(
    Id INT IDENTITY(1,1) PRIMARY KEY,
	--MUCHO CUIDADO!!! que solo puedes crear un reclamo de aquellos productos Vendidos o Entregados_Como_Reemplazo_Al_Cliente
    FK_Numero_Serie_Productos INT REFERENCES Numero_Serie_Productos(Id) NOT NULL UNIQUE,
    FK_Reclamos INT REFERENCES Reclamos(Id) NOT NULL,
    Fecha_Venta_Cliente_Final DATETIME NOT NULL,
    Fecha_Reclamo_Cliente_Final DATETIME,
    Forma_Compensacion VARCHAR(20) CHECK (Forma_Compensacion IN ('Reembolso', 'Reemplazo')) NOT NULL,
    --Pendiente cuando inicia el proceso, en revision cuando se asigna de Tecnico, 
	--Aprobado o Reprobado cuando el Tecnico da su respuesta
	--compensado cuando se le entrega el Reemplazo o Reembolso a la empresa cliente
    Estado VARCHAR(20) NOT NULL DEFAULT 'Pendiente' CHECK (Estado IN ('Pendiente', 'En Revision', 'Aprobado', 'Rechazado', 'Compensado')),
    FK_Tecnico_Asignado INT REFERENCES Usuarios(Id),
    Fecha_Revision_Tecnico DATETIME,
    Explicacion_Respuesta_Tecnico VARCHAR(1000),
    PDF_Revision_Tecnico VARCHAR(255)
);
GO

CREATE TABLE Reembolso(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Numero_Comprobante_Reembolso VARCHAR(50) NOT NULL,
    Fecha_Reembolso DATETIME NOT NULL DEFAULT GETDATE(),
	--Numero de cuenta en el momento en que se realizo el reembolso, queda como historial para auditoria.
    Num_Cuenta_Bancaria_Reembolso VARCHAR(30) NOT NULL,
    FK_Metodo_Pago INT REFERENCES Metodos_Pago(Id),
    Estado VARCHAR(20) DEFAULT 'Procesando' 
        CHECK (Estado IN ('Procesando', 'Completado', 'Fallido', 'Reversado')),
    Referencia_Bancaria VARCHAR(100),
    Comprobante_Pago VARCHAR(500),
    FK_Usuario_Autorizo INT REFERENCES Usuarios(Id),
    Fecha_Autorizacion DATETIME
);
GO

CREATE TABLE Reembolso_Por_Reclamos(
    Id INT IDENTITY(1,1) PRIMARY KEY,
	--solo se puede reembolsar, reclamos que hallan sido aprobados por el tecnico
    FK_Reclamos_Producto_SN INT REFERENCES Reclamos_Producto_SN(Id) NOT NULL UNIQUE,
    FK_Reembolso INT REFERENCES Reembolso(Id) NOT NULL
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

-- 2. Tabla para imágenes adicionales de productos
CREATE TABLE Producto_Imagenes (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    FK_Producto INT REFERENCES Productos(Id) NOT NULL,
    URL_Imagen VARCHAR(500) NOT NULL,
    Es_Principal BIT DEFAULT 0
);

-- Carrito de compras (si es e-commerce online)
CREATE TABLE Carrito_Compras (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    FK_Cliente INT REFERENCES Usuarios(Id) NOT NULL,
    FK_Producto INT REFERENCES Productos(Id) NOT NULL,
    Cantidad INT NOT NULL DEFAULT 1,
    Fecha_Agregado DATETIME DEFAULT GETDATE(),
    UNIQUE(FK_Cliente, FK_Producto)
);


-- ============================================
-- INSERTAR DATOS COMPLETOS (CORREGIDO)
-- ============================================

-- 2. Categorias
INSERT INTO Categorias (Nombre, Descripcion, Activo, FK_Categoria_Padre, Fecha_Creacion)
VALUES 
('Electrónica', 'Productos electrónicos de consumo', 1, NULL, '2023-01-01'),
('Smartphones', 'Teléfonos inteligentes', 1, 1, '2023-01-01'),
('Tablets', 'Tabletas electrónicas', 1, 1, '2023-01-01'),
('Laptops', 'Computadoras portátiles', 1, 1, '2023-01-01'),
('Televisores', 'Pantallas y televisores', 1, 1, '2023-01-01'),
('Audio', 'Equipos de audio', 1, 1, '2023-01-01'),
('Electrodomésticos', 'Electrodomésticos del hogar', 1, NULL, '2023-01-01'),
('Accesorios', 'Accesorios electrónicos', 1, 1, '2023-01-01'),
('Gaming', 'Consolas y videojuegos', 1, 1, '2023-01-01'),
('Smartwatches', 'Relojes inteligentes', 1, 1, '2023-01-01');
GO

-- 4. Proveedores
INSERT INTO Proveedores (Nombre, Cedula, RUC, Direccion, Telefono, Email, Contacto_Principal, Plazo_Entrega_Dias, Activo, Fecha_Creacion)
VALUES 
('Importadora TechEC', '0912345678', '0912345678001', 'Av. Amazonas N12-34, Quito', '022345678', 'contacto@techec.com', 'Juan López', 5, 1, '2023-01-15'),
('Distribuidora Electrónica SA', '0923456789', '0923456789001', 'Av. 9 de Octubre 123, Guayaquil', '042111222', 'ventas@distribuidora.com', 'María González', 7, 1, '2023-02-10'),
('Apple Ecuador', '0934567890', '0934567890001', 'Mall del Sol Local 45, Guayaquil', '042333444', 'contact@apple.ec', 'Carlos Ramírez', 3, 1, '2023-03-05'),
('Samsung Ecuador', '0945678901', '0945678901001', 'CCI Local 12, Quito', '022555666', 'ventas@samsung.ec', 'Ana Torres', 4, 1, '2023-03-20'),
('Importadora Xiaomi', '0956789012', '0956789012001', 'Av. Shyris N45-67, Quito', '022777888', 'info@xiaomi-ec.com', 'Luis Fernández', 6, 1, '2023-04-15');
GO

-- 5. Metodos_Pago
INSERT INTO Metodos_Pago (Tipo, Descripcion, Activo, Requiere_Confirmacion, Comision_Porcentaje)
VALUES 
('Efectivo', 'Pago en efectivo en local', 1, 0, 0.00),
('Debito', 'Tarjeta de débito', 1, 0, 1.50),
('Credito', 'Crédito interno de la tienda', 1, 1, 0.00),
('Payphone', 'Pago mediante Payphone', 1, 1, 2.00),
('Transferencia', 'Transferencia bancaria', 1, 1, 0.50);
GO

-- 6. Configuracion_SRI
INSERT INTO Configuracion_SRI (
    Ambiente,
    Token_Acceso,
    Fecha_Expiracion_Token
)
VALUES (
    'Pruebas',
    'TOKEN_DE_PRUEBA_ABC123XYZ',
    DATEADD(DAY, 30, GETDATE())
);
GO

INSERT INTO Configuracion_Empresa (
    RUC_Empresa,
    Nombre_Comercial,
    Razon_Social,
    Direccion_Matriz
)
VALUES (
    '0999999999001',
    'Electro Home',
    'Electro Home GuayaquilS.A.',
    'Av. Principal 123, Guayaquil, Ecuador'
);

-- 8. Marcas
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

-- 9. Usuarios (CORREGIDO: Tipo_Identificacion solo puede ser 'Cedula' o 'Pasaporte')
INSERT INTO Usuarios (
    Nombres, Apellidos, Razon_Social, Tipo_Identificacion, Identificacion, 
    RUC, Correo, Contrasena, Celular, Convencional, Pais, Division_administrativa, 
    Ciudad, Codigo_Postal, Direccion, Rol, Fecha_Creacion, Num_Cuenta_Bancaria, 
    Tipo_Cuenta_Bancaria, Activo, Contribuyente_Especial, Obligado_Contabilidad, Creado_Por
) VALUES 
-- Cliente Persona Natural
('Juan', 'Pérez', 'Individuo', 'Cedula', '0912345678', '0912345678001', 
 'juan.perez@gmail.com', HASHBYTES('SHA2_256', 'Juan123*'), '0987654321', 
 '042345678', 'Ecuador', 'Pichincha', 'Quito', '170135', 
 'Av. Amazonas N12-34 y Patria', 'Cliente', '2023-01-15 09:30:00', 
 '0102030405060708', 'Ahorro', 1, 0, 0, NULL),

('Ana', 'Rodríguez', 'Individuo', 'Cedula', '0923456789', '0923456789001', 
 'ana.rodriguez@empresa.com', HASHBYTES('SHA2_256', 'Ana123*'), '0998765432', 
 '042111222', 'Ecuador', 'Guayas', 'Guayaquil', '090150', 
 'Av. 9 de Octubre 123 y Pedro Carbo', 'Cliente', '2023-02-20 10:15:00', 
 '2222333344445555', 'Ahorro', 1, 0, 0, NULL),

-- Revisores
('María', 'Gómez', 'Individuo', 'Cedula', '0934567890', '0934567890001', 
 'maria.gomez@empresa.com', HASHBYTES('SHA2_256', 'Maria456*'), '0991234567', 
 '042333444', 'Ecuador', 'Pichincha', 'Quito', '170135', 
 'Av. Shyris N45-67 y Naciones Unidas', 'Revisor', '2023-03-10 14:20:00', 
 '1718192021222324', 'Corriente', 1, 0, 1, NULL),

('Pedro', 'López', 'Individuo', 'Cedula', '0945678901', '0945678901001', 
 'pedro.lopez@empresa.com', HASHBYTES('SHA2_256', 'Pedro456*'), '0988887777', 
 '042555666', 'Ecuador', 'Pichincha', 'Quito', '170135', 
 'Av. 6 de Diciembre N23-45 y Colón', 'Revisor', '2023-04-05 11:45:00', 
 '3333444455556666', 'Corriente', 1, 0, 1, NULL),

-- Técnicos
('Carlos', 'Ramírez', 'Individuo', 'Cedula', '0956789012', '0956789012001', 
 'carlos.ramirez@soporte.com', HASHBYTES('SHA2_256', 'Carlos789*'), '0976543210', 
 '042998877', 'Ecuador', 'Pichincha', 'Quito', '170135', 
 'Av. de la Prensa N56-78 y 10 de Agosto', 'Tecnico', '2023-05-12 08:30:00', 
 '2526272829303132', 'Ahorro', 1, 0, 0, NULL),

('Ana', 'Torres', 'Individuo', 'Cedula', '0967890123', '0967890123001', 
 'ana.torres@soporte.com', HASHBYTES('SHA2_256', 'Ana123*'), '0988888888', 
 '042123456', 'Ecuador', 'Guayas', 'Guayaquil', '090150', 
 'Calle 10 de Agosto N34-56 y Boyacá', 'Tecnico', '2023-06-18 13:15:00', 
 '4142434445464748', 'Corriente', 1, 0, 0, NULL),

('Luis', 'Fernández', 'Individuo', 'Cedula', '0978901234', '0978901234001', 
 'luis.fernandez@soporte.com', HASHBYTES('SHA2_256', 'Luis123*'), '0977777777', 
 '042777888', 'Ecuador', 'Pichincha', 'Quito', '170135', 
 'Av. del Bombero N67-89 y Mariscal Sucre', 'Tecnico', '2023-07-22 16:40:00', 
 '5758596061626364', 'Ahorro', 1, 0, 0, NULL),

-- Personal de Entrega
('Roberto', 'Mendoza', 'Individuo', 'Cedula', '0989012345', '0989012345001', 
 'roberto.mendoza@logistica.com', HASHBYTES('SHA2_256', 'Roberto123*'), '0961122334', 
 '042999000', 'Ecuador', 'Guayas', 'Guayaquil', '090150', 
 'Calle 5 de Junio N12-34 y Esmeraldas', 'Personal de Entrega', '2023-08-30 09:10:00', 
 '3334353637383940', 'Corriente', 1, 0, 0, NULL),

('Sofía', 'Castro', 'Individuo', 'Cedula', '0990123456', '0990123456001', 
 'sofia.castro@logistica.com', HASHBYTES('SHA2_256', 'Sofia123*'), '0969988776', 
 '042111000', 'Ecuador', 'Guayas', 'Guayaquil', '090150', 
 'Av. Francisco de Orellana y Juan Tanca Marengo', 'Personal de Entrega', '2023-09-14 12:25:00', 
 '7071727374757677', 'Ahorro', 1, 0, 0, NULL),

-- Cliente Jurídico
('Empresa', 'XYZ S.A.', 'EMPRESA XYZ SOCIEDAD ANONIMA', 'Cedula', '0911111111001', '0911111111001', 
 'contacto@xyz.com', HASHBYTES('SHA2_256', 'Xyz123*'), '0999999999', 
 '022333444', 'Ecuador', 'Pichincha', 'Quito', '170135', 
 'Av. Amazonas y Patria, Edificio Corporativo', 'Cliente_Juridico', '2023-10-05 15:30:00', 
 '8888999900001111', 'Corriente', 1, 1, 1, NULL),

-- Vendedor
('Miguel', 'Álvarez', 'Individuo', 'Cedula', '0991122334', '0991122334001', 
 'miguel.alvarez@techstore.ec', HASHBYTES('SHA2_256', 'Miguel123*'), '0981122334', 
 '022444555', 'Ecuador', 'Pichincha', 'Quito', '170135', 
 'Av. Amazonas N100-200 y República', 'Vendedor', '2023-11-10 08:15:00', 
 '1122334455667788', 'Corriente', 1, 0, 0, NULL),

-- Analista de Datos
('Laura', 'Morales', 'Individuo', 'Cedula', '0992233445', '0992233445001', 
 'laura.morales@techstore.ec', HASHBYTES('SHA2_256', 'Laura123*'), '0992233445', 
 '022666777', 'Ecuador', 'Pichincha', 'Quito', '170135', 
 'Calle Foch N45-67 y 6 de Diciembre', 'Analista_Datos', '2023-11-15 10:30:00', 
 '2233445566778899', 'Ahorro', 1, 0, 0, NULL),

-- Encargados de Inventario
('Diego', 'Vargas', 'Individuo', 'Cedula', '0993344556', '0993344556001', 
 'diego.vargas@techstore.ec', HASHBYTES('SHA2_256', 'Diego123*'), '0973344556', 
 '022555666', 'Ecuador', 'Pichincha', 'Quito', '170135', 
 'Av. 6 de Diciembre N300 y Granados', 'Encargado_Inventario', '2023-11-20 14:45:00', 
 '3344556677889900', 'Corriente', 1, 0, 0, NULL),

('Gabriela', 'Rojas', 'Individuo', 'Cedula', '0994455667', '0994455667001', 
 'gabriela.rojas@techstore.ec', HASHBYTES('SHA2_256', 'Gabriela123*'), '0984455667', 
 '022888999', 'Ecuador', 'Pichincha', 'Quito', '170135', 
 'Av. Naciones Unidas y Amazonas', 'Encargado_Inventario', '2023-11-25 09:20:00', 
 '4455667788990011', 'Ahorro', 1, 0, 0, NULL),

-- Gestor de Productos
('Andrés', 'Silva', 'Individuo', 'Cedula', '0995566778', '0995566778001', 
 'andres.silva@techstore.ec', HASHBYTES('SHA2_256', 'Andres123*'), '0995566778', 
 '022777888', 'Ecuador', 'Pichincha', 'Quito', '170135', 
 'Av. González Suárez y Orellana', 'Gestor_Productos', '2023-12-01 11:10:00', 
 '5566778899001122', 'Corriente', 1, 0, 0, NULL),

-- Administrador
('Administrador', 'Sistema', 'Individuo', 'Cedula', '0996677889', '0996677889001', 
 'admin@techstore.ec', HASHBYTES('SHA2_256', 'Admin123*'), '0996677889', 
 '022888999', 'Ecuador', 'Pichincha', 'Quito', '170135', 
 'Av. Amazonas N12-34 y Patria, Piso 10', 'Administrador', '2023-01-01 00:00:00', 
 '6677889900112233', 'Corriente', 1, 0, 1, NULL);
GO
-- 10. Inventario_Ubicaciones
INSERT INTO Inventario_Ubicaciones (Codigo, Nombre, Tipo, FK_Ubicacion_Padre, Capacidad_Maxima, Activo)
VALUES 
('BOD-QUITO', 'Bodega Quito', 'Bodega', NULL, 10000, 1),
('BOD-GYE', 'Bodega Guayaquil', 'Bodega', NULL, 15000, 1),
('EST-01-Q', 'Estante 01 Quito', 'Estante', 1, 500, 1),
('EST-02-Q', 'Estante 02 Quito', 'Estante', 1, 500, 1),
('EST-03-Q', 'Estante 03 Quito', 'Estante', 1, 500, 1),
('EST-01-G', 'Estante 01 Guayaquil', 'Estante', 2, 600, 1),
('EST-02-G', 'Estante 02 Guayaquil', 'Estante', 2, 600, 1),
('PAS-01-A', 'Pasillo A', 'Pasillo', 3, 100, 1),
('PAS-02-A', 'Pasillo B', 'Pasillo', 3, 100, 1),
('CAJ-01', 'Caja 01', 'Caja', 8, 50, 1);
GO

-- 11. Productos (modificado para usar NULL en Creado_Por y Modificado_Por temporalmente)
INSERT INTO Productos (
SKU, Peso_kg, Alto_cm, Ancho_cm, Profundidad_cm,
Visibilidad, Codigo, FK_Marca, FK_Categoria,
Modelo, Especificacion, Descripcion, Activo,
Imagen_URL, Fecha_Creacion,
Dias_Garantia, Precio, Creado_Por, Modificado_Por
)
VALUES 
('SM-GS23-256', 0.168, 14.6, 7.0, 0.76, 'Publico', 'PRD001', 1, 2, 'Galaxy S23', 'Smartphone 256GB, 8GB RAM, Snapdragon 8 Gen 2', 'El Galaxy S23 ofrece un rendimiento excepcional con su procesador Snapdragon 8 Gen 2 y cámara de alta resolución.', 1, '/img/products/galaxy_s23.jpg', '2023-01-15', 365, 999.99, 15, 15),

('SM-GS21-128', 0.169, 14.7, 7.1, 0.78, 'Publico', 'PRD002', 1, 2, 'Galaxy S21', 'Smartphone 128GB, 8GB RAM', 'Galaxy S21 con pantalla Dynamic AMOLED 2X y sistema de cámaras profesional.', 1, '/img/products/galaxy_s21.jpg', '2023-01-15', 365, 699.99, 15, 15),

('SM-QLED55', 18.5, 71.0, 122.0, 5.5, 'Publico', 'PRD003', 1, 5, 'QLED TV 55"', 'Televisor 4K QLED 55 pulgadas, Smart TV', 'Televisor QLED 4K con tecnología Quantum Dot para colores más vivos y realistas.', 1, '/img/products/qled_tv.jpg', '2023-01-15', 730, 899.99, 15, 15),

('SM-GTABS9', 0.498, 25.4, 16.5, 0.59, 'Publico', 'PRD004', 1, 3, 'Galaxy Tab S9', 'Tablet 11", 256GB, S-Pen incluido', 'Tablet premium con S-Pen incluido, perfecta para creativos y profesionales.', 1, '/img/products/galaxy_tab.jpg', '2023-01-15', 365, 799.99, 15, 15),

('SM-GWATCH6', 0.033, 4.4, 4.4, 1.0, 'Publico', 'PRD005', 1, 10, 'Galaxy Watch 6', 'Smartwatch 44mm, LTE, ECG', 'Smartwatch con monitor de ECG, seguimiento de sueño y conectividad LTE.', 1, '/img/products/galaxy_watch.jpg', '2023-01-15', 365, 349.99, 15, 15),

('SM-GS7-OLD', 0.152, 14.2, 6.9, 0.79, 'Privado', 'PRD006', 1, 2, 'Galaxy S7', 'Smartphone antiguo descontinuado', 'Modelo descontinuado, disponible solo para reposición de garantías.', 0, '/img/products/galaxy_s7.jpg', '2023-01-15', 0, 99.99, 15, 15),

('LG-OLED65', 25.8, 81.5, 144.0, 4.6, 'Publico', 'PRD007', 2, 5, 'OLED TV 65"', 'Televisor OLED 65 pulgadas, 4K, WebOS', 'Televisor OLED con colores perfectos y sistema operativo webOS.', 1, '/img/products/lg_oled.jpg', '2023-01-15', 730, 1499.99, 15, 15),

('LG-GRAM17', 1.35, 1.68, 38.0, 26.0, 'Publico', 'PRD008', 2, 4, 'Gram 17"', 'Laptop ultrabook 17", 16GB RAM, 1TB SSD', 'Laptop ultraligera con pantalla de 17 pulgadas y gran capacidad de almacenamiento.', 1, '/img/products/lg_gram.jpg', '2023-01-15', 365, 1699.99, 15, 15),

('LG-MON27', 5.2, 37.6, 61.2, 21.5, 'Publico', 'PRD009', 2, 1, 'Monitor 27" 4K', 'Monitor IPS 27", 4K UHD, HDR10', 'Monitor 4K UHD con tecnología IPS para ángulos de visión amplios.', 1, '/img/products/lg_monitor.jpg', '2023-01-15', 365, 499.99, 15, 15),

('AP-IP15P', 0.187, 14.7, 7.1, 0.82, 'Publico', 'PRD010', 3, 2, 'iPhone 15 Pro', 'Smartphone 256GB, Titanio, A17 Pro', 'iPhone 15 Pro con diseño en titanio y el poderoso chip A17 Pro.', 1, '/img/products/iphone15.jpg', '2023-01-15', 365, 1299.99, 15, 15),

('AP-IP14', 0.174, 14.6, 7.1, 0.78, 'Publico', 'PRD011', 3, 2, 'iPhone 14', 'Smartphone 256GB, A15 Bionic', 'iPhone 14 con chip A15 Bionic y sistema de cámaras avanzado.', 1, '/img/products/iphone14.jpg', '2023-01-15', 365, 999.99, 15, 15),

('AP-MBAIR', 1.24, 0.41, 30.4, 21.5, 'Publico', 'PRD012', 3, 4, 'MacBook Air M2', 'Laptop 13", 8GB RAM, 256GB SSD', 'MacBook Air con chip M2, diseño delgado y batería de larga duración.', 1, '/img/products/macbook_air.jpg', '2023-01-15', 365, 1099.99, 15, 15),

('AP-IPAIR5', 0.461, 24.8, 17.8, 0.61, 'Publico', 'PRD013', 3, 3, 'iPad Air 5', 'Tablet 10.9", M1, 64GB', 'iPad Air con chip M1 y compatibilidad con Apple Pencil 2.', 1, '/img/products/ipad_air.jpg', '2023-01-15', 365, 599.99, 15, 15),

('AP-WATCH9', 0.038, 4.5, 3.8, 1.07, 'Publico', 'PRD014', 3, 10, 'Apple Watch Series 9', 'Smartwatch 45mm, GPS', 'Apple Watch Series 9 con GPS y monitor de salud avanzado.', 1, '/img/products/apple_watch.jpg', '2023-01-15', 365, 429.99, 15, 15),

('XM-RN13P', 0.185, 16.1, 7.6, 0.81, 'Publico', 'PRD015', 4, 2, 'Redmi Note 13 Pro', 'Smartphone 256GB, 12GB RAM, 200MP cámara', 'Redmi Note 13 Pro con cámara de 200MP y carga rápida de 120W.', 1, '/img/products/redmi_note.jpg', '2023-01-15', 180, 349.99, 15, 15),

('XM-MITV50', 12.8, 64.5, 111.5, 8.2, 'Publico', 'PRD016', 4, 5, 'Mi TV 4K 50"', 'Televisor 4K Android TV, Dolby Vision', 'Televisor Android TV 4K con soporte Dolby Vision y Atmos.', 1, '/img/products/mi_tv.jpg', '2023-01-15', 365, 449.99, 15, 15),

('XM-13T', 0.202, 16.2, 7.5, 0.85, 'Publico', 'PRD017', 4, 2, 'Xiaomi 13T', 'Smartphone 256GB, MediaTek Dimensity 8200', 'Xiaomi 13T con pantalla AMOLED y cámara Leica.', 1, '/img/products/xiaomi_13t.jpg', '2023-01-15', 180, 599.99, 15, 15),

('SN-WHXM5', 0.25, 8.3, 20.4, 24.3, 'Publico', 'PRD018', 5, 6, 'WH-1000XM5', 'Audífonos noise cancelling, 30h batería', 'Audífonos con cancelación de ruido líder en la industria.', 1, '/img/products/sony_headphones.jpg', '2023-01-15', 365, 399.99, 15, 15),

('SN-PS5', 4.5, 39.0, 26.0, 10.4, 'Publico', 'PRD019', 5, 9, 'PlayStation 5', 'Consola de videojuegos, 1TB SSD', 'Consola PlayStation 5 con SSD ultrarrápido y control DualSense.', 1, '/img/products/ps5.jpg', '2023-01-15', 365, 499.99, 15, 15),

('SN-BRAVIA55', 18.2, 71.5, 122.5, 4.8, 'Publico', 'PRD020', 5, 5, 'Bravia XR 55"', 'Televisor OLED 55", Cognitive Processor XR', 'Televisor Bravia OLED con procesador cognitivo XR.', 1, '/img/products/bravia.jpg', '2023-01-15', 730, 1799.99, 15, 15),

('HP-SPECTRE', 1.34, 1.79, 31.9, 22.0, 'Publico', 'PRD021', 6, 4, 'Spectre x360', 'Laptop convertible 14", OLED, i7, 16GB', 'Laptop convertible con pantalla OLED táctil y procesador i7.', 1, '/img/products/spectre.jpg', '2023-01-15', 365, 1299.99, 15, 15),

('HP-LASERJET', 8.7, 24.3, 39.6, 34.0, 'Publico', 'PRD022', 6, 1, 'LaserJet Pro', 'Impresora láser, WiFi, duplex', 'Impresora láser WiFi con impresión a doble cara automática.', 1, '/img/products/laserjet.jpg', '2023-01-15', 180, 299.99, 15, 15),

('DL-XPS13', 1.27, 1.54, 29.5, 19.9, 'Publico', 'PRD023', 7, 4, 'XPS 13', 'Laptop 13.4", i7, 16GB, 512GB SSD', 'Laptop XPS 13 con pantalla InfinityEdge y diseño premium.', 1, '/img/products/xps13.jpg', '2023-01-15', 365, 1199.99, 15, 15),

('DL-ULTRA27', 7.1, 37.8, 61.5, 22.0, 'Publico', 'PRD024', 7, 1, 'UltraSharp 27"', 'Monitor 4K, USB-C, color calibrado', 'Monitor 4K UltraSharp con calibración de color profesional.', 1, '/img/products/ultrasharp.jpg', '2023-01-15', 365, 699.99, 15, 15),

('LN-THINKX1', 1.09, 1.49, 32.3, 21.7, 'Publico', 'PRD025', 8, 4, 'ThinkPad X1 Carbon', 'Laptop 14", i7, 16GB, 1TB SSD', 'ThinkPad X1 Carbon con teclado ergonómico y seguridad avanzada.', 1, '/img/products/thinkpad.jpg', '2023-01-15', 365, 1599.99, 15, 15),

('LN-YOGA9I', 1.4, 1.67, 31.2, 22.4, 'Publico', 'PRD026', 8, 4, 'Yoga 9i', 'Laptop convertible 14", 2.8K OLED', 'Laptop convertible Yoga 9i con pantalla OLED 2.8K.', 1, '/img/products/yoga.jpg', '2023-01-15', 365, 1399.99, 15, 15),

('HW-MATE50P', 0.209, 16.2, 7.5, 0.91, 'Publico', 'PRD027', 9, 2, 'Mate 50 Pro', 'Smartphone 256GB, cámara Leica', 'Huawei Mate 50 Pro con sistema de cámaras Leica y HarmonyOS.', 1, '/img/products/mate50.jpg', '2023-01-15', 180, 899.99, 15, 15),

('HW-MATEPAD', 0.609, 28.6, 18.4, 0.65, 'Publico', 'PRD028', 9, 3, 'MatePad Pro', 'Tablet 12.6", HarmonyOS', 'MatePad Pro con pantalla OLED y soporte para lápiz M-Pencil.', 1, '/img/products/matepad.jpg', '2023-01-15', 180, 799.99, 15, 15),

('NK-G50', 0.189, 16.5, 7.6, 0.89, 'Publico', 'PRD029', 10, 2, 'G50', 'Smartphone 128GB, 5G, batería 5000mAh', 'Nokia G50 con 5G, batería de larga duración y Android puro.', 1, '/img/products/nokia_g50.jpg', '2023-01-15', 365, 299.99, 15, 15);
GO

-- Actualizar Productos para tener referencias válidas (Creado_Por = 15 existe)
UPDATE Productos SET Creado_Por = 15, Modificado_Por = 15 WHERE Creado_Por IS NULL;
GO

-- 12. Numero_Serie_Productos
INSERT INTO Numero_Serie_Productos (FK_Producto, Numero_Serie, Estado_Inventario, Fecha_Ingreso, FK_Proveedor, FK_Ubicacion)
VALUES 
(1, 'SM-S911BZKDEUE', 'Vendido', '2023-12-01', 4, 3),
(2, 'SM-G991BZKDEUE', 'Vendido', '2023-11-15', 4, 3),
(10, 'MPXJ3LL/A', 'Vendido', '2023-12-10', 3, 3),
(11, 'MPXV3LL/A', 'Vendido', '2023-11-20', 3, 3),
(12, 'Z15T0008W', 'Vendido', '2023-12-05', 3, 3),
(2, 'SM-G991BZKDXYZ', 'Entregado_Como_Reemplazo_Al_Cliente', '2023-11-10', 4, 3),
(11, 'MPXV3LL/B', 'Entregado_Como_Reemplazo_Al_Cliente', '2023-11-25', 3, 3),
(2, 'SM-G991BZKVENC', 'Vendido', '2023-11-30', 4, 3),
(6, 'SM-G930FDESC', 'Vendido', '2023-10-15', 4, 3),
(1, 'SM-X716BZKDEUE', 'Se_Puede_Vender', '2023-12-20', 4, 3),
(1, 'SM-S911BZKDEU1', 'Se_Puede_Vender', '2024-01-05', 4, 3),
(1, 'SM-S911BZKDEU2', 'Se_Puede_Vender', '2024-01-05', 4, 3),
(2, 'SM-G991BZKDEU1', 'Se_Puede_Vender', '2024-01-05', 4, 3),
(3, 'SN-QLED55-001', 'Se_Puede_Vender', '2024-01-05', 4, 4),
(10, 'MPXJ3LL/B', 'Se_Puede_Vender', '2024-01-05', 3, 3),
(11, 'MPXV3LL/C', 'Se_Puede_Vender', '2024-01-05', 3, 3),
(12, 'Z15T0009W', 'Se_Puede_Vender', '2024-01-05', 3, 3),
(13, 'MPL23LL/A', 'Se_Puede_Vender', '2024-01-05', 3, 3),
(25, 'HW-MATE50-001', 'Se_Puede_Vender', '2024-01-05', 1, 3),
(26, 'HW-MATEPAD-001', 'Se_Puede_Vender', '2024-01-05', 1, 4),
(7, 'LG-OLED65-001', 'Se_Puede_Vender', '2024-01-05', 2, 4),
(8, 'LG-GRAM17-001', 'Se_Puede_Vender', '2024-01-05', 2, 4),
(16, 'SNY-WH1000-001', 'Se_Puede_Vender', '2024-01-05', 1, 3),
(17, 'SNY-PS5-001', 'Se_Puede_Vender', '2024-01-05', 1, 4),
(1, 'SM-S911BZKD-REP01', 'Se_Puede_Vender', '2024-01-10', 4, 3),
(1, 'SM-S911BZKD-REP02', 'Se_Puede_Vender', '2024-01-10', 4, 3),
(1, 'SM-S911BZKD-REP03', 'Se_Puede_Vender', '2024-01-10', 4, 3),
(2, 'SM-G991BZKD-REP01', 'Se_Puede_Vender', '2024-01-10', 4, 3),
(2, 'SM-G991BZKD-REP02', 'Se_Puede_Vender', '2024-01-10', 4, 3),
(10, 'MPXJ3LL-REP01', 'Se_Puede_Vender', '2024-01-10', 3, 3),
(10, 'MPXJ3LL-REP02', 'Se_Puede_Vender', '2024-01-10', 3, 3),
(11, 'MPXV3LL-REP01', 'Se_Puede_Vender', '2024-01-10', 3, 3),
(11, 'MPXV3LL-REP02', 'Se_Puede_Vender', '2024-01-10', 3, 3),
(12, 'Z15T0009-REP01', 'Se_Puede_Vender', '2024-01-10', 3, 3),
(12, 'Z15T0009-REP02', 'Se_Puede_Vender', '2024-01-10', 3, 3);
GO

-- 13. TokensDeAcceso
INSERT INTO TokensDeAcceso (Token, FechaCreacion, FechaExpiracion, Vigente, Tipo_Token, FK_Usuario)
VALUES 
('token_abc123', '2024-01-10 08:30:00', '2024-01-17 08:30:00', 1, 'ResetPassword', 1),
('token_def456', '2024-01-12 14:20:00', '2024-01-19 14:20:00', 1, 'EmailVerification', 2),
('token_ghi789', '2024-01-15 10:15:00', '2024-01-22 10:15:00', 0, 'SecurityAlert', 3),
('token_jkl012', '2024-01-18 16:45:00', '2024-01-25 16:45:00', 1, 'ChangeEmail', 4),
('token_mno345', '2024-01-20 11:30:00', '2024-01-27 11:30:00', 1, 'ResetPassword', 5);
GO

-- 14. Usuarios_Certificacion_Marcas
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

-- 15. Ventas
INSERT INTO Ventas (Codigo_Factura, FK_Empresa_Cliente, FK_Vendedor, Tipo_Venta, Fecha_Compra, Estado_SRI, Clave_Acceso, Numero_Autorizacion, Fecha_Autorizacion, XML_Path, PDF_Path, Observaciones, Direccion_Entrega, Telefono_Contacto, Total_Compra, Creado_Por, Fecha_Modificacion, Modificado_Por)
VALUES 
('001-001-00001000', 1, 11, 'Contado', '2024-01-15 10:30:00', 'Autorizado', '2701202401099123456789001000110010000000101234567812', '2701202401099', '2024-01-15 10:35:00', '/xml/facturas/001-001-00001000.xml', '/pdf/facturas/001-001-00001000.pdf', 'Venta normal', 'Av. Amazonas N12-34, Quito', '0987654321', 999.99, 11, NULL, NULL),
('001-001-00001001', 1, 11, 'Contado', '2023-12-01 14:45:00', 'Autorizado', '0112202301099123456789001000110010000000111234567812', '0112202301099', '2023-12-01 14:50:00', '/xml/facturas/001-001-00001001.xml', '/pdf/facturas/001-001-00001001.pdf', 'Venta de temporada', 'Av. Amazonas N12-34, Quito', '0987654321', 699.99, 11, NULL, NULL),
('001-001-00001002', 1, 11, 'Contado', '2023-12-15 11:20:00', 'Autorizado', '1512202301099123456789001000110010000000121234567812', '1512202301099', '2023-12-15 11:25:00', '/xml/facturas/001-001-00001002.xml', '/pdf/facturas/001-001-00001002.pdf', 'Cliente frecuente', 'Av. Amazonas N12-34, Quito', '0987654321', 1299.99, 11, NULL, NULL),
('001-001-00001003', 1, 11, 'Credito', '2023-09-20 16:10:00', 'Autorizado', '2009202301099123456789001000110010000000131234567812', '2009202301099', '2023-09-20 16:15:00', '/xml/facturas/001-001-00001003.xml', '/pdf/facturas/001-001-00001003.pdf', 'Venta a crédito 30 días', 'Av. Amazonas N12-34, Quito', '0987654321', 999.99, 11, NULL, NULL),
('001-001-00001004', 1, 11, 'Contado', '2023-07-05 09:15:00', 'Autorizado', '0507202301099123456789001000110010000000141234567812', '0507202301099', '2023-07-05 09:20:00', '/xml/facturas/001-001-00001004.xml', '/pdf/facturas/001-001-00001004.pdf', 'Promoción especial', 'Av. Amazonas N12-34, Quito', '0987654321', 1099.99, 11, NULL, NULL),
('001-001-00001005', 1, 11, 'Contado', '2023-08-10 13:40:00', 'Autorizado', '1008202301099123456789001000110010000000151234567812', '1008202301099', '2023-08-10 13:45:00', '/xml/facturas/001-001-00001005.xml', '/pdf/facturas/001-001-00001005.pdf', 'Venta normal', 'Av. Amazonas N12-34, Quito', '0987654321', 699.99, 11, NULL, NULL),
('001-001-00001006', 1, 11, 'Contado', '2023-08-25 15:30:00', 'Autorizado', '2508202301099123456789001000110010000000161234567812', '2508202301099', '2023-08-25 15:35:00', '/xml/facturas/001-001-00001006.xml', '/pdf/facturas/001-001-00001006.pdf', 'Venta normal', 'Av. Amazonas N12-34, Quito', '0987654321', 999.99, 11, NULL, NULL),
('001-001-00001007', 1, 11, 'Contado', '2023-01-10 10:00:00', 'Autorizado', '1001202301099123456789001000110010000000171234567812', '1001202301099', '2023-01-10 10:05:00', '/xml/facturas/001-001-00001007.xml', '/pdf/facturas/001-001-00001007.pdf', 'Venta antigua', 'Av. Amazonas N12-34, Quito', '0987654321', 699.99, 11, NULL, NULL),
('001-001-00001008', 1, 11, 'Contado', '2024-01-10 09:30:00', 'Autorizado', '1001202401099123456789001000110010000000181234567812', '1001202401099', '2024-01-10 09:35:00', '/xml/facturas/001-001-00001008.xml', '/pdf/facturas/001-001-00001008.pdf', 'Venta normal', 'Av. Amazonas N12-34, Quito', '0987654321', 999.99, 11, NULL, NULL),
('001-001-00001009', 1, 11, 'Contado', '2024-01-15 14:20:00', 'Autorizado', '1501202401099123456789001000110010000000191234567812', '1501202401099', '2024-01-15 14:25:00', '/xml/facturas/001-001-00001009.xml', '/pdf/facturas/001-001-00001009.pdf', 'Venta normal', 'Av. Amazonas N12-34, Quito', '0987654321', 999.99, 11, NULL, NULL),
('001-001-00001010', 1, 11, 'Contado', '2024-01-17 11:45:00', 'Autorizado', '1701202401099123456789001000110010000000201234567812', '1701202401099', '2024-01-17 11:50:00', '/xml/facturas/001-001-00001010.xml', '/pdf/facturas/001-001-00001010.pdf', 'Venta normal', 'Av. Amazonas N12-34, Quito', '0987654321', 699.99, 11, NULL, NULL),
('001-001-00001011', 1, 11, 'Contado', '2024-01-05 16:10:00', 'Autorizado', '0501202401099123456789001000110010000000211234567812', '0501202401099', '2024-01-05 16:15:00', '/xml/facturas/001-001-00001011.xml', '/pdf/facturas/001-001-00001011.pdf', 'Venta normal', 'Av. Amazonas N12-34, Quito', '0987654321', 899.99, 11, NULL, NULL),
('001-001-00001012', 1, 11, 'Contado', '2024-01-13 10:15:00', 'Autorizado', '1301202401099123456789001000110010000000221234567812', '1301202401099', '2024-01-13 10:20:00', '/xml/facturas/001-001-00001012.xml', '/pdf/facturas/001-001-00001012.pdf', 'Venta normal', 'Av. Amazonas N12-34, Quito', '0987654321', 1299.99, 11, NULL, NULL),
('001-001-00001013', 1, 11, 'Contado', '2024-01-20 13:40:00', 'Autorizado', '2001202401099123456789001000110010000000231234567812', '2001202401099', '2024-01-20 13:45:00', '/xml/facturas/001-001-00001013.xml', '/pdf/facturas/001-001-00001013.pdf', 'Venta normal', 'Av. Amazonas N12-34, Quito', '0987654321', 999.99, 11, NULL, NULL),
('001-001-00001014', 1, 11, 'Contado', '2024-01-07 15:30:00', 'Autorizado', '0701202401099123456789001000110010000000241234567812', '0701202401099', '2024-01-07 15:35:00', '/xml/facturas/001-001-00001014.xml', '/pdf/facturas/001-001-00001014.pdf', 'Venta normal', 'Av. Amazonas N12-34, Quito', '0987654321', 1099.99, 11, NULL, NULL),
('001-001-00001015', 1, 11, 'Contado', '2024-01-22 08:45:00', 'Autorizado', '2201202401099123456789001000110010000000251234567812', '2201202401099', '2024-01-22 08:50:00', '/xml/facturas/001-001-00001015.xml', '/pdf/facturas/001-001-00001015.pdf', 'Venta normal', 'Av. Amazonas N12-34, Quito', '0987654321', 599.99, 11, NULL, NULL),
('001-001-00001016', 1, 11, 'Contado', '2023-12-30 12:20:00', 'Autorizado', '3012202301099123456789001000110010000000261234567812', '3012202301099', '2023-12-30 12:25:00', '/xml/facturas/001-001-00001016.xml', '/pdf/facturas/001-001-00001016.pdf', 'Venta fin de año', 'Av. Amazonas N12-34, Quito', '0987654321', 899.99, 11, NULL, NULL),
('001-001-00001017', 1, 11, 'Contado', '2024-01-03 17:35:00', 'Autorizado', '0301202401099123456789001000110010000000271234567812', '0301202401099', '2024-01-03 17:40:00', '/xml/facturas/001-001-00001017.xml', '/pdf/facturas/001-001-00001017.pdf', 'Venta normal', 'Av. Amazonas N12-34, Quito', '0987654321', 799.99, 11, NULL, NULL),
('001-001-00001018', 1, 11, 'Contado', '2023-12-25 10:50:00', 'Autorizado', '2512202301099123456789001000110010000000281234567812', '2512202301099', '2023-12-25 10:55:00', '/xml/facturas/001-001-00001018.xml', '/pdf/facturas/001-001-00001018.pdf', 'Venta navideña', 'Av. Amazonas N12-34, Quito', '0987654321', 1499.99, 11, NULL, NULL),
('001-001-00001019', 1, 11, 'Contado', '2023-12-27 14:15:00', 'Autorizado', '2712202301099123456789001000110010000000291234567812', '2712202301099', '2023-12-27 14:20:00', '/xml/facturas/001-001-00001019.xml', '/pdf/facturas/001-001-00001019.pdf', 'Venta normal', 'Av. Amazonas N12-34, Quito', '0987654321', 1699.99, 11, NULL, NULL),
('001-001-00001020', 1, 11, 'Contado', '2024-01-18 09:25:00', 'Autorizado', '1801202401099123456789001000110010000000301234567812', '1801202401099', '2024-01-18 09:30:00', '/xml/facturas/001-001-00001020.xml', '/pdf/facturas/001-001-00001020.pdf', 'Venta normal', 'Av. Amazonas N12-34, Quito', '0987654321', 399.99, 11, NULL, NULL),
('001-001-00001021', 1, 11, 'Contado', '2024-01-11 11:55:00', 'Autorizado', '1101202401099123456789001000110010000000311234567812', '1101202401099', '2024-01-11 12:00:00', '/xml/facturas/001-001-00001021.xml', '/pdf/facturas/001-001-00001021.pdf', 'Venta normal', 'Av. Amazonas N12-34, Quito', '0987654321', 499.99, 11, NULL, NULL);
GO

-- 16. Ventas_Por_Numero_Serie_Productos
INSERT INTO Ventas_Por_Numero_Serie_Productos (FK_Ventas, FK_Numero_Serie_Producto, Precio_Venta, Descuento, IVA)
VALUES 
(1, 1, 999.99, 0.00, 119.99),
(2, 2, 699.99, 0.00, 83.99),
(3, 3, 1299.99, 0.00, 155.99),
(4, 4, 999.99, 50.00, 113.99),
(5, 5, 1099.99, 0.00, 131.99),
(6, 6, 699.99, 0.00, 83.99),
(7, 7, 999.99, 100.00, 107.99),
(8, 8, 699.99, 0.00, 83.99),
(9, 9, 999.99, 0.00, 119.99),
(10, 10, 999.99, 0.00, 119.99),
(11, 11, 699.99, 0.00, 83.99),
(12, 12, 899.99, 0.00, 107.99),
(13, 13, 1299.99, 0.00, 155.99),
(14, 14, 999.99, 0.00, 119.99),
(15, 15, 1099.99, 0.00, 131.99),
(16, 16, 599.99, 0.00, 71.99),
(17, 17, 899.99, 0.00, 107.99),
(18, 18, 799.99, 0.00, 95.99),
(19, 19, 1499.99, 0.00, 179.99),
(20, 20, 1699.99, 0.00, 203.99),
(21, 21, 399.99, 0.00, 47.99),
(22, 22, 499.99, 0.00, 59.99);
GO

-- 17. Reclamos
INSERT INTO Reclamos (Codigo_Reclamo, FK_Empresa_Cliente, Fecha_Creacion_Reclamo)
VALUES 
('REC-20240115-ABC123', 1, '2024-01-15 14:20:00'),
('REC-20240120-DEF456', 2, '2024-01-20 10:45:00'),
('REC-20240125-GHI789', 1, '2024-01-25 15:30:00'),
('REC-20240126-JKL012', 1, '2024-01-26 10:45:00'),
('REC-20240127-MNO345', 1, '2024-01-27 14:20:00'),
('REC-20240128-PQR678', 1, '2024-01-28 09:15:00'),
('REC-20240129-STU901', 1, '2024-01-29 16:40:00'),
('REC-20240130-VWX234', 1, '2024-01-30 11:25:00'),
('REC-20240131-YZA567', 2, '2024-01-31 13:50:00'),
('REC-20240201-BCD890', 2, '2024-02-01 08:35:00'),
('REC-20240202-EFG123', 3, '2024-02-02 12:10:00'),
('REC-ENTREGA-001', 1, '2024-01-20 14:30:00'),
('REC-ENTREGA-002', 2, '2024-01-22 10:45:00'),
('REC-ENTREGA-003', 3, '2024-01-24 16:20:00'),
('REC-CLIENTE-001', 1, '2024-01-23 11:30:00'),
('REC-CLIENTE-002', 1, '2024-01-20 14:45:00'),
('REC-CLIENTE-003', 1, '2024-01-15 09:20:00'),
('REC-CLIENTE-004', 1, '2024-01-10 16:10:00'),
('REC-CLIENTE-005', 1, '2024-01-05 13:25:00');
GO

-- 18. Reclamos_Producto_SN
INSERT INTO Reclamos_Producto_SN (FK_Numero_Serie_Productos, FK_Reclamos, Fecha_Venta_Cliente_Final, Fecha_Reclamo_Cliente_Final, Forma_Compensacion, Estado, FK_Tecnico_Asignado, Fecha_Revision_Tecnico, Explicacion_Respuesta_Tecnico, PDF_Revision_Tecnico)
VALUES 
(1, 1, '2024-01-10 11:30:00', '2024-01-15 14:30:00', 'Reembolso', 'Aprobado', 5, '2024-01-18 09:15:00', 'Producto con falla confirmada. Se aprueba el reclamo para reembolso.', '/reclamos/tecnico/123456_20240125_informe.pdf'),
(2, 2, '2023-12-05 15:20:00', '2024-01-20 11:00:00', 'Reemplazo', 'Rechazado', 6, '2024-01-22 16:40:00', 'Producto sin fallas. Daño por mal uso del cliente.', '/reclamos/tecnico/789012_20240124_informe.pdf'),
(9, 3, '2024-01-10 09:30:00', '2024-01-25 16:00:00', 'Reembolso', 'En Revision', 5, '2024-01-26 14:30:00', 'En proceso de revisión técnica', '/reclamos/tecnico/001_revision.pdf'),
(13, 4, '2024-01-13 10:15:00', '2024-01-26 11:30:00', 'Reemplazo', 'Pendiente', 5, NULL, NULL, NULL),
(14, 5, '2024-01-20 13:40:00', '2024-01-27 15:20:00', 'Reembolso', 'Pendiente', 5, NULL, NULL, NULL),
(15, 6, '2024-01-07 15:30:00', '2024-01-28 10:45:00', 'Reemplazo', 'Pendiente', 5, NULL, NULL, NULL),
(10, 7, '2024-01-15 14:20:00', '2024-01-29 14:30:00', 'Reemplazo', 'En Revision', 6, '2024-01-30 09:15:00', 'Revisión en curso', '/reclamos/tecnico/002_revision.pdf'),
(16, 8, '2024-01-22 08:45:00', '2024-01-30 16:20:00', 'Reembolso', 'Pendiente', 6, NULL, NULL, NULL),
(17, 9, '2023-12-30 12:20:00', '2024-01-31 11:10:00', 'Reemplazo', 'Pendiente', 6, NULL, NULL, NULL),
(18, 10, '2024-01-03 17:35:00', '2024-02-01 09:45:00', 'Reembolso', 'Pendiente', 6, NULL, NULL, NULL),
(19, 11, '2023-12-25 10:50:00', '2024-02-02 13:25:00', 'Reemplazo', 'En Revision', 7, '2024-02-03 10:40:00', 'Análisis técnico iniciado', '/reclamos/tecnico/003_revision.pdf'),
(20, 3, '2023-12-27 14:15:00', '2024-01-25 16:30:00', 'Reembolso', 'Pendiente', 7, NULL, NULL, NULL),
(21, 4, '2024-01-18 09:25:00', '2024-01-26 12:15:00', 'Reemplazo', 'Pendiente', 7, NULL, NULL, NULL),
(22, 5, '2024-01-11 11:55:00', '2024-01-27 17:40:00', 'Reembolso', 'Pendiente', 7, NULL, NULL, NULL),
(23, 12, '2024-01-10 11:30:00', '2024-01-20 15:00:00', 'Reemplazo', 'Aprobado', 5, '2024-01-21 09:15:00', 'Producto con falla confirmada. Se aprueba reemplazo.', '/reclamos/tecnico/entrega_001.pdf'),
(24, 13, '2023-12-05 15:20:00', '2024-01-22 11:15:00', 'Reemplazo', 'Aprobado', 6, '2024-01-23 14:20:00', 'Producto defectuoso. Aprobado para reemplazo.', '/reclamos/tecnico/entrega_004.pdf');
GO

-- 19. Marca_Lo_Entrego_Como_Reemplazo
INSERT INTO Marca_Lo_Entrego_Como_Reemplazo (FK_Numero_Serie_Productos)
VALUES 
(6),
(7);
GO

-- 20. Pagos
INSERT INTO Pagos (FK_Venta, FK_Metodo_Pago, Estado, Monto, Referencia, Fecha_Pago, Datos_Transaccion, Terminal_PuntoVenta, Cuotas, Monto_Cuota)
VALUES 
(1, 2, 'Completo', 999.99, 'TRX-001-2024', '2024-01-15 10:31:00', '{"terminal": "POS001", "auth_code": "123456"}', 'POS001', 1, 999.99),
(2, 1, 'Completo', 699.99, 'EFECTIVO-001', '2023-12-01 14:46:00', '{"cajero": "Miguel Álvarez"}', NULL, 1, 699.99),
(3, 2, 'Completo', 1299.99, 'TRX-002-2023', '2023-12-15 11:21:00', '{"terminal": "POS001", "auth_code": "123457"}', 'POS001', 1, 1299.99),
(4, 3, 'Completo', 999.99, 'CREDITO-001', '2023-09-20 16:11:00', '{"credito_id": "CR001", "plazo": "30"}', NULL, 1, 999.99),
(5, 4, 'Completo', 1099.99, 'PAYPHONE-001', '2023-07-05 09:16:00', '{"transaction_id": "PP123456", "phone": "0987654321"}', NULL, 1, 1099.99),
(6, 5, 'Completo', 699.99, 'TRANSF-001', '2023-08-10 13:41:00', '{"bank": "Banco Pichincha", "reference": "TRANSF123"}', NULL, 1, 699.99),
(7, 2, 'Completo', 999.99, 'TRX-003-2023', '2023-08-25 15:31:00', '{"terminal": "POS001", "auth_code": "123458"}', 'POS001', 1, 999.99),
(8, 1, 'Completo', 699.99, 'EFECTIVO-002', '2023-01-10 10:01:00', '{"cajero": "Miguel Álvarez"}', NULL, 1, 699.99);
GO

-- 21. Reembolso
INSERT INTO Reembolso (Numero_Comprobante_Reembolso, Fecha_Reembolso, Num_Cuenta_Bancaria_Reembolso, FK_Metodo_Pago, Estado, Referencia_Bancaria, Comprobante_Pago, FK_Usuario_Autorizo, Fecha_Autorizacion)
VALUES 
('REMB-001-2024', '2024-01-17 10:30:00', '0102030405060708', 5, 'Completado', 'TRANSF-REMB-001', '/comprobantes/reembolso/001.pdf', 16, '2024-01-17 10:00:00'),
('REMB-002-2024', '2024-01-25 14:15:00', '2222333344445555', 5, 'Procesando', 'TRANSF-REMB-002', NULL, 16, '2024-01-25 13:30:00'),
('REMB-003-2024', '2024-01-30 11:20:00', '1718192021222324', 2, 'Completado', 'TRX-REMB-003', '/comprobantes/reembolso/003.pdf', 16, '2024-01-30 11:00:00');
GO

-- 22. Reembolso_Por_Reclamos
INSERT INTO Reembolso_Por_Reclamos (FK_Reclamos_Producto_SN, FK_Reembolso)
VALUES 
(1, 1),
(3, 2);
GO

-- 23. Comprobante_De_Reemplazo
INSERT INTO Comprobante_De_Reemplazo (PDF_Comprobante_Entrega_Cliente, FK_Personal_Entrega, Estado)
VALUES 
('/entrega/comprobante_cliente_001.pdf', 8, 'Completado'),
('/entrega/comprobante_cliente_002.pdf', 9, 'Firmado'),
('/entrega/comprobante_cliente_003.pdf', 8, 'Generado'),
('/entrega/comprobante_cliente_004.pdf', 9, 'Pendiente');
GO

-- 24. Comprobante_Producto_Reemplazado
INSERT INTO Comprobante_Producto_Reemplazado (FK_Reclamos_Producto_SN, FK_Comprobante_De_Reemplazo, FK_Producto_De_Reemplazo)
VALUES 
(2, 1, 25),
(4, 2, 26),
(6, 3, 27),
(9, 4, 28);
GO

-- 25. Inventario_Movimientos
INSERT INTO Inventario_Movimientos (FK_Producto, FK_Usuario, Tipo_Movimiento, Cantidad, Cantidad_Anterior, Cantidad_Nueva, Motivo, Referencia, Fecha_Movimiento, Costo_Unitario)
VALUES 
(1, 13, 'Entrada', 10, 0, 10, 'Compra a proveedor', 'OC-001-2024', '2024-01-05 09:00:00', 850.00),
(2, 13, 'Entrada', 8, 0, 8, 'Compra a proveedor', 'OC-002-2024', '2024-01-05 09:15:00', 550.00),
(10, 13, 'Entrada', 5, 0, 5, 'Compra a proveedor', 'OC-003-2024', '2024-01-05 10:00:00', 1100.00),
(1, 13, 'Salida', 1, 10, 9, 'Venta a cliente', 'VENTA-001', '2024-01-15 10:30:00', 850.00),
(2, 13, 'Salida', 1, 8, 7, 'Venta a cliente', 'VENTA-002', '2023-12-01 14:45:00', 550.00),
(10, 13, 'Salida', 1, 5, 4, 'Venta a cliente', 'VENTA-003', '2023-12-15 11:20:00', 1100.00),
(1, 13, 'Ajuste', 1, 9, 8, 'Ajuste por inventario físico', 'AJUSTE-001', '2024-01-20 16:00:00', 850.00),
(2, 13, 'Devolucion', 1, 7, 8, 'Devolución de cliente', 'DEV-001', '2024-01-25 14:00:00', 550.00);
GO

-- 27. Producto_Imagenes
INSERT INTO Producto_Imagenes (FK_Producto, URL_Imagen, Es_Principal)
VALUES 
(1, '/img/products/galaxy_s23_1.jpg', 1),
(1, '/img/products/galaxy_s23_2.jpg', 0),
(1, '/img/products/galaxy_s23_3.jpg', 0),
(2, '/img/products/galaxy_s21_1.jpg', 1),
(2, '/img/products/galaxy_s21_2.jpg', 0),
(10, '/img/products/iphone15_1.jpg', 1),
(10, '/img/products/iphone15_2.jpg', 0),
(10, '/img/products/iphone15_3.jpg', 0),
(12, '/img/products/macbook_air_1.jpg', 1),
(12, '/img/products/macbook_air_2.jpg', 0);
GO

-- 29. Carrito_Compras
INSERT INTO Carrito_Compras (FK_Cliente, FK_Producto, Cantidad, Fecha_Agregado)
VALUES 
(1, 1, 1, '2024-01-28 10:30:00'),
(1, 18, 1, '2024-01-28 10:35:00'),
(2, 10, 1, '2024-01-29 14:20:00'),
(2, 14, 1, '2024-01-29 14:25:00'),
(3, 12, 1, '2024-01-30 11:15:00'),
(3, 19, 1, '2024-01-30 11:20:00');
GO