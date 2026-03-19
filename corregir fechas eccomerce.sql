UPDATE Ventas_Por_Numero_Serie_Productos
SET IVA = ROUND(Precio_Venta * 15 / 115, 2)
WHERE IVA != ROUND(Precio_Venta * 15 / 115, 2);

UPDATE Ventas
SET Fecha_Compra = GETDATE();

SELECT Id, Precio_Venta, IVA, Precio_Venta - IVA AS BaseImponible
FROM Ventas_Por_Numero_Serie_Productos
WHERE FK_Ventas = 1;

SELECT Id, Estado_SRI, Fecha_Autorizacion, Clave_Acceso
FROM Ventas;


-- ============================================
-- INSERTS ADICIONALES PARA VENTAS (DATOS DE PRUEBA)
-- ============================================
-- Se generan 10 nuevas ventas con sus respectivos detalles,
-- utilizando números de serie de productos disponibles (no usados previamente).
-- Los estados se alternan entre 'Pendiente' y 'Autorizado' para simular diferentes escenarios.
-- Las fechas de compra se establecen con GETDATE() para que sean actuales.
-- Los totales se calculan a partir de los precios de los productos (incluyen IVA 15%).
-- ============================================

-- Nueva venta 23 (ID autoincremental)
INSERT INTO Ventas (Codigo_Factura, FK_Empresa_Cliente, FK_Vendedor, Tipo_Venta, Fecha_Compra, Estado_SRI, Clave_Acceso, Numero_Autorizacion, Fecha_Autorizacion, XML_Path, PDF_Path, Observaciones, Direccion_Entrega, Telefono_Contacto, Total_Compra, Creado_Por, Fecha_Modificacion, Modificado_Por)
VALUES ('001-001-00001023', 1, 11, 'Contado', GETDATE(), 'Pendiente', NULL, NULL, NULL, NULL, NULL, 'Venta de prueba 23', 'Av. Amazonas N12-34, Quito', '0987654321', 349.99, 11, NULL, NULL);

-- Producto asociado (FK_Numero_Serie_Producto = 23, asumiendo disponible)
INSERT INTO Ventas_Por_Numero_Serie_Productos (FK_Ventas, FK_Numero_Serie_Producto, Precio_Venta, Descuento, IVA)
VALUES ((SELECT Id FROM Ventas WHERE Codigo_Factura = '001-001-00001023'), 23, 349.99, 0.00, ROUND(349.99 * 15 / 115, 2));

-- Nueva venta 24
INSERT INTO Ventas (Codigo_Factura, FK_Empresa_Cliente, FK_Vendedor, Tipo_Venta, Fecha_Compra, Estado_SRI, Clave_Acceso, Numero_Autorizacion, Fecha_Autorizacion, XML_Path, PDF_Path, Observaciones, Direccion_Entrega, Telefono_Contacto, Total_Compra, Creado_Por, Fecha_Modificacion, Modificado_Por)
VALUES ('001-001-00001024', 2, 11, 'Contado', GETDATE(), 'Autorizado', '0803202601095073406100110010010000010241234567812', '0803202601099', GETDATE(), '/xml/facturas/001-001-00001024.xml', '/pdf/facturas/001-001-00001024.pdf', 'Venta autorizada 24', 'Av. 9 de Octubre 123, Guayaquil', '0998765432', 599.99, 11, NULL, NULL);

INSERT INTO Ventas_Por_Numero_Serie_Productos (FK_Ventas, FK_Numero_Serie_Producto, Precio_Venta, Descuento, IVA)
VALUES ((SELECT Id FROM Ventas WHERE Codigo_Factura = '001-001-00001024'), 24, 599.99, 0.00, ROUND(599.99 * 15 / 115, 2));

-- Nueva venta 25
INSERT INTO Ventas (Codigo_Factura, FK_Empresa_Cliente, FK_Vendedor, Tipo_Venta, Fecha_Compra, Estado_SRI, Clave_Acceso, Numero_Autorizacion, Fecha_Autorizacion, XML_Path, PDF_Path, Observaciones, Direccion_Entrega, Telefono_Contacto, Total_Compra, Creado_Por, Fecha_Modificacion, Modificado_Por)
VALUES ('001-001-00001025', 10, 11, 'Credito', GETDATE(), 'Pendiente', NULL, NULL, NULL, NULL, NULL, 'Venta a crédito 25', 'Av. Amazonas y Patria, Quito', '0999999999', 1299.99, 11, NULL, NULL);

INSERT INTO Ventas_Por_Numero_Serie_Productos (FK_Ventas, FK_Numero_Serie_Producto, Precio_Venta, Descuento, IVA)
VALUES ((SELECT Id FROM Ventas WHERE Codigo_Factura = '001-001-00001025'), 25, 1299.99, 0.00, ROUND(1299.99 * 15 / 115, 2));

-- Nueva venta 26
INSERT INTO Ventas (Codigo_Factura, FK_Empresa_Cliente, FK_Vendedor, Tipo_Venta, Fecha_Compra, Estado_SRI, Clave_Acceso, Numero_Autorizacion, Fecha_Autorizacion, XML_Path, PDF_Path, Observaciones, Direccion_Entrega, Telefono_Contacto, Total_Compra, Creado_Por, Fecha_Modificacion, Modificado_Por)
VALUES ('001-001-00001026', 1, 11, 'Contado', GETDATE(), 'Autorizado', '0803202601095073406100110010010000010261234567812', '0803202601099', GETDATE(), '/xml/facturas/001-001-00001026.xml', '/pdf/facturas/001-001-00001026.pdf', 'Venta autorizada 26', 'Av. Amazonas N12-34, Quito', '0987654321', 799.99, 11, NULL, NULL);

INSERT INTO Ventas_Por_Numero_Serie_Productos (FK_Ventas, FK_Numero_Serie_Producto, Precio_Venta, Descuento, IVA)
VALUES ((SELECT Id FROM Ventas WHERE Codigo_Factura = '001-001-00001026'), 26, 799.99, 0.00, ROUND(799.99 * 15 / 115, 2));

-- Nueva venta 27
INSERT INTO Ventas (Codigo_Factura, FK_Empresa_Cliente, FK_Vendedor, Tipo_Venta, Fecha_Compra, Estado_SRI, Clave_Acceso, Numero_Autorizacion, Fecha_Autorizacion, XML_Path, PDF_Path, Observaciones, Direccion_Entrega, Telefono_Contacto, Total_Compra, Creado_Por, Fecha_Modificacion, Modificado_Por)
VALUES ('001-001-00001027', 2, 11, 'Contado', GETDATE(), 'Pendiente', NULL, NULL, NULL, NULL, NULL, 'Venta pendiente 27', 'Av. 9 de Octubre 123, Guayaquil', '0998765432', 899.99, 11, NULL, NULL);

INSERT INTO Ventas_Por_Numero_Serie_Productos (FK_Ventas, FK_Numero_Serie_Producto, Precio_Venta, Descuento, IVA)
VALUES ((SELECT Id FROM Ventas WHERE Codigo_Factura = '001-001-00001027'), 27, 899.99, 0.00, ROUND(899.99 * 15 / 115, 2));

-- Nueva venta 28
INSERT INTO Ventas (Codigo_Factura, FK_Empresa_Cliente, FK_Vendedor, Tipo_Venta, Fecha_Compra, Estado_SRI, Clave_Acceso, Numero_Autorizacion, Fecha_Autorizacion, XML_Path, PDF_Path, Observaciones, Direccion_Entrega, Telefono_Contacto, Total_Compra, Creado_Por, Fecha_Modificacion, Modificado_Por)
VALUES ('001-001-00001028', 10, 11, 'Contado', GETDATE(), 'Autorizado', '0803202601095073406100110010010000010281234567812', '0803202601099', GETDATE(), '/xml/facturas/001-001-00001028.xml', '/pdf/facturas/001-001-00001028.pdf', 'Venta autorizada 28', 'Av. Amazonas y Patria, Quito', '0999999999', 1499.99, 11, NULL, NULL);

INSERT INTO Ventas_Por_Numero_Serie_Productos (FK_Ventas, FK_Numero_Serie_Producto, Precio_Venta, Descuento, IVA)
VALUES ((SELECT Id FROM Ventas WHERE Codigo_Factura = '001-001-00001028'), 28, 1499.99, 0.00, ROUND(1499.99 * 15 / 115, 2));

-- Nueva venta 29
INSERT INTO Ventas (Codigo_Factura, FK_Empresa_Cliente, FK_Vendedor, Tipo_Venta, Fecha_Compra, Estado_SRI, Clave_Acceso, Numero_Autorizacion, Fecha_Autorizacion, XML_Path, PDF_Path, Observaciones, Direccion_Entrega, Telefono_Contacto, Total_Compra, Creado_Por, Fecha_Modificacion, Modificado_Por)
VALUES ('001-001-00001029', 1, 11, 'Credito', GETDATE(), 'Pendiente', NULL, NULL, NULL, NULL, NULL, 'Venta a crédito 29', 'Av. Amazonas N12-34, Quito', '0987654321', 429.99, 11, NULL, NULL);

INSERT INTO Ventas_Por_Numero_Serie_Productos (FK_Ventas, FK_Numero_Serie_Producto, Precio_Venta, Descuento, IVA)
VALUES ((SELECT Id FROM Ventas WHERE Codigo_Factura = '001-001-00001029'), 29, 429.99, 0.00, ROUND(429.99 * 15 / 115, 2));

-- Nueva venta 30
INSERT INTO Ventas (Codigo_Factura, FK_Empresa_Cliente, FK_Vendedor, Tipo_Venta, Fecha_Compra, Estado_SRI, Clave_Acceso, Numero_Autorizacion, Fecha_Autorizacion, XML_Path, PDF_Path, Observaciones, Direccion_Entrega, Telefono_Contacto, Total_Compra, Creado_Por, Fecha_Modificacion, Modificado_Por)
VALUES ('001-001-00001030', 2, 11, 'Contado', GETDATE(), 'Autorizado', '0803202601095073406100110010010000010301234567812', '0803202601099', GETDATE(), '/xml/facturas/001-001-00001030.xml', '/pdf/facturas/001-001-00001030.pdf', 'Venta autorizada 30', 'Av. 9 de Octubre 123, Guayaquil', '0998765432', 699.99, 11, NULL, NULL);

INSERT INTO Ventas_Por_Numero_Serie_Productos (FK_Ventas, FK_Numero_Serie_Producto, Precio_Venta, Descuento, IVA)
VALUES ((SELECT Id FROM Ventas WHERE Codigo_Factura = '001-001-00001030'), 30, 699.99, 0.00, ROUND(699.99 * 15 / 115, 2));

-- Nueva venta 31
INSERT INTO Ventas (Codigo_Factura, FK_Empresa_Cliente, FK_Vendedor, Tipo_Venta, Fecha_Compra, Estado_SRI, Clave_Acceso, Numero_Autorizacion, Fecha_Autorizacion, XML_Path, PDF_Path, Observaciones, Direccion_Entrega, Telefono_Contacto, Total_Compra, Creado_Por, Fecha_Modificacion, Modificado_Por)
VALUES ('001-001-00001031', 10, 11, 'Contado', GETDATE(), 'Pendiente', NULL, NULL, NULL, NULL, NULL, 'Venta pendiente 31', 'Av. Amazonas y Patria, Quito', '0999999999', 999.99, 11, NULL, NULL);

INSERT INTO Ventas_Por_Numero_Serie_Productos (FK_Ventas, FK_Numero_Serie_Producto, Precio_Venta, Descuento, IVA)
VALUES ((SELECT Id FROM Ventas WHERE Codigo_Factura = '001-001-00001031'), 31, 999.99, 0.00, ROUND(999.99 * 15 / 115, 2));

-- Nueva venta 32
INSERT INTO Ventas (Codigo_Factura, FK_Empresa_Cliente, FK_Vendedor, Tipo_Venta, Fecha_Compra, Estado_SRI, Clave_Acceso, Numero_Autorizacion, Fecha_Autorizacion, XML_Path, PDF_Path, Observaciones, Direccion_Entrega, Telefono_Contacto, Total_Compra, Creado_Por, Fecha_Modificacion, Modificado_Por)
VALUES ('001-001-00001032', 1, 11, 'Contado', GETDATE(), 'Autorizado', '0803202601095073406100110010010000010321234567812', '0803202601099', GETDATE(), '/xml/facturas/001-001-00001032.xml', '/pdf/facturas/001-001-00001032.pdf', 'Venta autorizada 32', 'Av. Amazonas N12-34, Quito', '0987654321', 1199.99, 11, NULL, NULL);

INSERT INTO Ventas_Por_Numero_Serie_Productos (FK_Ventas, FK_Numero_Serie_Producto, Precio_Venta, Descuento, IVA)
VALUES ((SELECT Id FROM Ventas WHERE Codigo_Factura = '001-001-00001032'), 32, 1199.99, 0.00, ROUND(1199.99 * 15 / 115, 2));

-- ============================================
-- FIN DE INSERCIONES ADICIONALES
-- ============================================

-- ============================================
-- INSERTS ADICIONALES PARA VENTAS (LOTE 3)
-- ============================================
-- Se generan 10 nuevas ventas (IDs 33 a 42) con sus respectivos detalles.
-- Se utilizan números de serie hipotéticos 33 a 42; si no existen, se debe ajustar.
-- Estados: alternados entre 'Pendiente' y 'Autorizado'.
-- Fecha de compra: GETDATE() para que sean actuales.
-- Totales variados (IVA incluido 15%).
-- ============================================

-- Venta 33
INSERT INTO Ventas (Codigo_Factura, FK_Empresa_Cliente, FK_Vendedor, Tipo_Venta, Fecha_Compra, Estado_SRI, Clave_Acceso, Numero_Autorizacion, Fecha_Autorizacion, XML_Path, PDF_Path, Observaciones, Direccion_Entrega, Telefono_Contacto, Total_Compra, Creado_Por, Fecha_Modificacion, Modificado_Por)
VALUES ('001-001-00001033', 1, 11, 'Contado', GETDATE(), 'Pendiente', NULL, NULL, NULL, NULL, NULL, 'Venta de prueba 33', 'Av. Amazonas N12-34, Quito', '0987654321', 299.99, 11, NULL, NULL);

INSERT INTO Ventas_Por_Numero_Serie_Productos (FK_Ventas, FK_Numero_Serie_Producto, Precio_Venta, Descuento, IVA)
VALUES ((SELECT Id FROM Ventas WHERE Codigo_Factura = '001-001-00001033'), 33, 299.99, 0.00, ROUND(299.99 * 15 / 115, 2));

-- Venta 34
INSERT INTO Ventas (Codigo_Factura, FK_Empresa_Cliente, FK_Vendedor, Tipo_Venta, Fecha_Compra, Estado_SRI, Clave_Acceso, Numero_Autorizacion, Fecha_Autorizacion, XML_Path, PDF_Path, Observaciones, Direccion_Entrega, Telefono_Contacto, Total_Compra, Creado_Por, Fecha_Modificacion, Modificado_Por)
VALUES ('001-001-00001034', 2, 11, 'Contado', GETDATE(), 'Autorizado', '0803202601095073406100110010010000010341234567812', '0803202601099', GETDATE(), '/xml/facturas/001-001-00001034.xml', '/pdf/facturas/001-001-00001034.pdf', 'Venta autorizada 34', 'Av. 9 de Octubre 123, Guayaquil', '0998765432', 459.99, 11, NULL, NULL);

INSERT INTO Ventas_Por_Numero_Serie_Productos (FK_Ventas, FK_Numero_Serie_Producto, Precio_Venta, Descuento, IVA)
VALUES ((SELECT Id FROM Ventas WHERE Codigo_Factura = '001-001-00001034'), 34, 459.99, 0.00, ROUND(459.99 * 15 / 115, 2));

-- Venta 35
INSERT INTO Ventas (Codigo_Factura, FK_Empresa_Cliente, FK_Vendedor, Tipo_Venta, Fecha_Compra, Estado_SRI, Clave_Acceso, Numero_Autorizacion, Fecha_Autorizacion, XML_Path, PDF_Path, Observaciones, Direccion_Entrega, Telefono_Contacto, Total_Compra, Creado_Por, Fecha_Modificacion, Modificado_Por)
VALUES ('001-001-00001035', 10, 11, 'Credito', GETDATE(), 'Pendiente', NULL, NULL, NULL, NULL, NULL, 'Venta a crédito 35', 'Av. Amazonas y Patria, Quito', '0999999999', 789.99, 11, NULL, NULL);

INSERT INTO Ventas_Por_Numero_Serie_Productos (FK_Ventas, FK_Numero_Serie_Producto, Precio_Venta, Descuento, IVA)
VALUES ((SELECT Id FROM Ventas WHERE Codigo_Factura = '001-001-00001035'), 35, 789.99, 0.00, ROUND(789.99 * 15 / 115, 2));

-- Venta 36
INSERT INTO Ventas (Codigo_Factura, FK_Empresa_Cliente, FK_Vendedor, Tipo_Venta, Fecha_Compra, Estado_SRI, Clave_Acceso, Numero_Autorizacion, Fecha_Autorizacion, XML_Path, PDF_Path, Observaciones, Direccion_Entrega, Telefono_Contacto, Total_Compra, Creado_Por, Fecha_Modificacion, Modificado_Por)
VALUES ('001-001-00001036', 1, 11, 'Contado', GETDATE(), 'Autorizado', '0803202601095073406100110010010000010361234567812', '0803202601099', GETDATE(), '/xml/facturas/001-001-00001036.xml', '/pdf/facturas/001-001-00001036.pdf', 'Venta autorizada 36', 'Av. Amazonas N12-34, Quito', '0987654321', 999.99, 11, NULL, NULL);

INSERT INTO Ventas_Por_Numero_Serie_Productos (FK_Ventas, FK_Numero_Serie_Producto, Precio_Venta, Descuento, IVA)
VALUES ((SELECT Id FROM Ventas WHERE Codigo_Factura = '001-001-00001036'), 36, 999.99, 0.00, ROUND(999.99 * 15 / 115, 2));

-- Venta 37
INSERT INTO Ventas (Codigo_Factura, FK_Empresa_Cliente, FK_Vendedor, Tipo_Venta, Fecha_Compra, Estado_SRI, Clave_Acceso, Numero_Autorizacion, Fecha_Autorizacion, XML_Path, PDF_Path, Observaciones, Direccion_Entrega, Telefono_Contacto, Total_Compra, Creado_Por, Fecha_Modificacion, Modificado_Por)
VALUES ('001-001-00001037', 2, 11, 'Contado', GETDATE(), 'Pendiente', NULL, NULL, NULL, NULL, NULL, 'Venta pendiente 37', 'Av. 9 de Octubre 123, Guayaquil', '0998765432', 549.99, 11, NULL, NULL);

INSERT INTO Ventas_Por_Numero_Serie_Productos (FK_Ventas, FK_Numero_Serie_Producto, Precio_Venta, Descuento, IVA)
VALUES ((SELECT Id FROM Ventas WHERE Codigo_Factura = '001-001-00001037'), 37, 549.99, 0.00, ROUND(549.99 * 15 / 115, 2));

-- Venta 38
INSERT INTO Ventas (Codigo_Factura, FK_Empresa_Cliente, FK_Vendedor, Tipo_Venta, Fecha_Compra, Estado_SRI, Clave_Acceso, Numero_Autorizacion, Fecha_Autorizacion, XML_Path, PDF_Path, Observaciones, Direccion_Entrega, Telefono_Contacto, Total_Compra, Creado_Por, Fecha_Modificacion, Modificado_Por)
VALUES ('001-001-00001038', 10, 11, 'Contado', GETDATE(), 'Autorizado', '0803202601095073406100110010010000010381234567812', '0803202601099', GETDATE(), '/xml/facturas/001-001-00001038.xml', '/pdf/facturas/001-001-00001038.pdf', 'Venta autorizada 38', 'Av. Amazonas y Patria, Quito', '0999999999', 1199.99, 11, NULL, NULL);

INSERT INTO Ventas_Por_Numero_Serie_Productos (FK_Ventas, FK_Numero_Serie_Producto, Precio_Venta, Descuento, IVA)
VALUES ((SELECT Id FROM Ventas WHERE Codigo_Factura = '001-001-00001038'), 38, 1199.99, 0.00, ROUND(1199.99 * 15 / 115, 2));

-- Venta 39
INSERT INTO Ventas (Codigo_Factura, FK_Empresa_Cliente, FK_Vendedor, Tipo_Venta, Fecha_Compra, Estado_SRI, Clave_Acceso, Numero_Autorizacion, Fecha_Autorizacion, XML_Path, PDF_Path, Observaciones, Direccion_Entrega, Telefono_Contacto, Total_Compra, Creado_Por, Fecha_Modificacion, Modificado_Por)
VALUES ('001-001-00001039', 1, 11, 'Credito', GETDATE(), 'Pendiente', NULL, NULL, NULL, NULL, NULL, 'Venta a crédito 39', 'Av. Amazonas N12-34, Quito', '0987654321', 679.99, 11, NULL, NULL);

INSERT INTO Ventas_Por_Numero_Serie_Productos (FK_Ventas, FK_Numero_Serie_Producto, Precio_Venta, Descuento, IVA)
VALUES ((SELECT Id FROM Ventas WHERE Codigo_Factura = '001-001-00001039'), 39, 679.99, 0.00, ROUND(679.99 * 15 / 115, 2));

-- Venta 40
INSERT INTO Ventas (Codigo_Factura, FK_Empresa_Cliente, FK_Vendedor, Tipo_Venta, Fecha_Compra, Estado_SRI, Clave_Acceso, Numero_Autorizacion, Fecha_Autorizacion, XML_Path, PDF_Path, Observaciones, Direccion_Entrega, Telefono_Contacto, Total_Compra, Creado_Por, Fecha_Modificacion, Modificado_Por)
VALUES ('001-001-00001040', 2, 11, 'Contado', GETDATE(), 'Autorizado', '0803202601095073406100110010010000010401234567812', '0803202601099', GETDATE(), '/xml/facturas/001-001-00001040.xml', '/pdf/facturas/001-001-00001040.pdf', 'Venta autorizada 40', 'Av. 9 de Octubre 123, Guayaquil', '0998765432', 1299.99, 11, NULL, NULL);

INSERT INTO Ventas_Por_Numero_Serie_Productos (FK_Ventas, FK_Numero_Serie_Producto, Precio_Venta, Descuento, IVA)
VALUES ((SELECT Id FROM Ventas WHERE Codigo_Factura = '001-001-00001040'), 40, 1299.99, 0.00, ROUND(1299.99 * 15 / 115, 2));

-- Venta 41
INSERT INTO Ventas (Codigo_Factura, FK_Empresa_Cliente, FK_Vendedor, Tipo_Venta, Fecha_Compra, Estado_SRI, Clave_Acceso, Numero_Autorizacion, Fecha_Autorizacion, XML_Path, PDF_Path, Observaciones, Direccion_Entrega, Telefono_Contacto, Total_Compra, Creado_Por, Fecha_Modificacion, Modificado_Por)
VALUES ('001-001-00001041', 10, 11, 'Contado', GETDATE(), 'Pendiente', NULL, NULL, NULL, NULL, NULL, 'Venta pendiente 41', 'Av. Amazonas y Patria, Quito', '0999999999', 849.99, 11, NULL, NULL);

INSERT INTO Ventas_Por_Numero_Serie_Productos (FK_Ventas, FK_Numero_Serie_Producto, Precio_Venta, Descuento, IVA)
VALUES ((SELECT Id FROM Ventas WHERE Codigo_Factura = '001-001-00001041'), 41, 849.99, 0.00, ROUND(849.99 * 15 / 115, 2));

-- Venta 42
INSERT INTO Ventas (Codigo_Factura, FK_Empresa_Cliente, FK_Vendedor, Tipo_Venta, Fecha_Compra, Estado_SRI, Clave_Acceso, Numero_Autorizacion, Fecha_Autorizacion, XML_Path, PDF_Path, Observaciones, Direccion_Entrega, Telefono_Contacto, Total_Compra, Creado_Por, Fecha_Modificacion, Modificado_Por)
VALUES ('001-001-00001042', 1, 11, 'Contado', GETDATE(), 'Autorizado', '0803202601095073406100110010010000010421234567812', '0803202601099', GETDATE(), '/xml/facturas/001-001-00001042.xml', '/pdf/facturas/001-001-00001042.pdf', 'Venta autorizada 42', 'Av. Amazonas N12-34, Quito', '0987654321', 399.99, 11, NULL, NULL);

INSERT INTO Ventas_Por_Numero_Serie_Productos (FK_Ventas, FK_Numero_Serie_Producto, Precio_Venta, Descuento, IVA)
VALUES ((SELECT Id FROM Ventas WHERE Codigo_Factura = '001-001-00001042'), 42, 399.99, 0.00, ROUND(399.99 * 15 / 115, 2));

-- ============================================
-- FIN DE INSERCIONES LOTE 3
-- ============================================