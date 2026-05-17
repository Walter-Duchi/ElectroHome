# ElectroHome Guayaquil

Sistema de gestión empresarial integral para una tienda de electrodomésticos, desarrollado con arquitectura limpia en .NET 10 y React + TypeScript. Cubre el ciclo completo del negocio: e-commerce, gestión de inventario, sistema de reclamos, facturación electrónica con el SRI y pagos en línea mediante Payphone.

---

## Tabla de contenidos

- [Descripción general](#descripción-general)
- [Tecnologías utilizadas](#tecnologías-utilizadas)
- [Arquitectura del proyecto](#arquitectura-del-proyecto)
- [Módulos del sistema](#módulos-del-sistema)
- [Requisitos previos](#requisitos-previos)
- [Instalación y ejecución local](#instalación-y-ejecución-local)
- [Variables de entorno](#variables-de-entorno)
- [Roles de usuario](#roles-de-usuario)
- [Estructura del proyecto](#estructura-del-proyecto)
- [Documentación de la API](#documentación-de-la-api)
- [Preguntas frecuentes](#preguntas-frecuentes)
- [Autor](#autor)

---

## Descripción general

ElectroHome es una plataforma empresarial que reemplaza procesos manuales de una tienda de electrodomésticos por flujos digitales automatizados. El sistema gestiona desde la publicación de productos en la tienda virtual hasta la resolución de reclamos posventa, pasando por el control de inventario por número de serie, la emisión de comprobantes electrónicos autorizados por el SRI y el cobro en línea.

El proyecto fue construido aplicando principios de arquitectura limpia (Clean Architecture), separando responsabilidades en capas independientes (API, Application, Domain, Infrastructure) para garantizar mantenibilidad, testabilidad y escalabilidad.

---

## Tecnologías utilizadas

### Backend
| Tecnología | Versión | Propósito |
|---|---|---|
| ASP.NET Core | .NET 10 | Framework principal de la API REST |
| Entity Framework Core | 10.x | ORM para acceso a base de datos |
| SQL Server | - | Base de datos relacional |
| JWT Bearer | 8.x | Autenticación y autorización stateless |
| Swashbuckle / Swagger | 10.x | Documentación interactiva de la API |
| QuestPDF | Community | Generación de comprobantes PDF |
| DinkToPdf | 1.0.8 | Conversión HTML a PDF |
| Yamgooo.SRI.Sign | - | Firma electrónica de comprobantes |
| WCF (SOAP) | - | Integración con los web services del SRI (recepción y autorización) |
| Payphone SDK | - | Pasarela de pago en línea |

### Frontend
| Tecnología | Versión | Propósito |
|---|---|---|
| React | 19.x | Biblioteca principal de UI |
| TypeScript | - | Tipado estático |
| Vite | - | Bundler y servidor de desarrollo |
| React Router DOM | - | Enrutamiento del lado del cliente |
| Material UI (MUI) | - | Sistema de componentes visuales |

---

## Arquitectura del proyecto

El backend sigue una arquitectura limpia en cuatro capas con dependencias en una sola dirección:

```
┌─────────────────────────────────────┐
│              API Layer              │  Minimal API endpoints, autenticación, CORS
├─────────────────────────────────────┤
│          Application Layer          │  DTOs, contratos de servicio
├─────────────────────────────────────┤
│           Domain Layer              │  Entidades del negocio, reglas de dominio
├─────────────────────────────────────┤
│        Infrastructure Layer         │  EF Core, servicios externos (SRI, Payphone, Email)
└─────────────────────────────────────┘
```

```
Frontend (React + TypeScript)
        │
        └── HTTP/REST ──────────────▶ ASP.NET Core Minimal API
                                             │
                                       SQL Server (EF Core)
                                             │
                              ┌──────────────┴──────────────┐
                         SRI (SOAP/WCF)              Payphone API
                    (Facturación electrónica)      (Pagos en línea)
```

---

## Módulos del sistema

### E-commerce
Tienda virtual con catálogo de productos, filtros por categoría y marca, carrito de compras, proceso de checkout y pago en línea mediante Payphone.

### Gestión de inventario
Control de stock por número de serie, registro de movimientos de entrada y salida, gestión de ubicaciones físicas en bodega y administración de proveedores.

### Sistema de reclamos
Flujo completo de posventa: el cliente crea un reclamo sobre un producto comprado, un técnico realiza la revisión, se determina si corresponde reemplazo o devolución, y el personal de entrega gestiona el retiro y la entrega del producto de reemplazo con comprobante digital.

### Facturación electrónica (SRI)
Emisión de comprobantes electrónicos (facturas) firmados digitalmente y autorizados por el Servicio de Rentas Internas del Ecuador, usando los web services SOAP de recepción y autorización.

### Panel de administración
Gestión completa de usuarios con roles diferenciados, configuración de datos de empresa, parámetros del SRI y ajustes generales del sistema.

### Dashboard analítico
Visualización de métricas de ventas, inventario y reclamos para la toma de decisiones gerenciales.

---

## Requisitos previos

### Backend
- [.NET SDK 10](https://dotnet.microsoft.com/download) o superior
- SQL Server 2019 o superior (o SQL Server Express)
- Visual Studio 2022 o Visual Studio Code con extensión C#

### Frontend
- [Node.js](https://nodejs.org/) v18 o superior
- npm v9 o superior

### Verificar instalaciones
```bash
dotnet --version
node --version
npm --version
```

---

## Instalación y ejecución local

### 1. Clonar el repositorio

```bash
git clone https://github.com/Walter-Duchi/ElectroHome.git
cd ElectroHome
```

### 2. Configurar la base de datos

Crear la base de datos en SQL Server y agregar la cadena de conexión y los secretos de configuración. Se recomienda usar `dotnet user-secrets` para el entorno de desarrollo:

```bash
cd API
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=ElectroHomeDB;Trusted_Connection=True;TrustServerCertificate=True"
dotnet user-secrets set "JwtSettings:Key" "tu_clave_secreta_de_al_menos_32_caracteres"
dotnet user-secrets set "JwtSettings:Issuer" "ElectroHomeAPI"
dotnet user-secrets set "JwtSettings:Audience" "ElectroHomeClient"
```

### 3. Aplicar las migraciones de Entity Framework

```bash
cd API
dotnet ef database update
```

### 4. Instalar dependencias del frontend

```bash
cd frontend
npm install
```

### 5. Iniciar ambos servicios con el script de desarrollo

Desde la raíz del proyecto, ejecutar el script de PowerShell incluido. Este script verifica los puertos, compila el backend y levanta ambos servicios en ventanas separadas:

```powershell
.\start-dev.ps1
```

O iniciarlos manualmente en terminales separadas:

```bash
# Terminal 1 — Backend
cd API
dotnet watch run

# Terminal 2 — Frontend
cd frontend
npm run dev
```

### URLs de acceso

| Servicio | URL |
|---|---|
| Frontend | http://localhost:3000 |
| Backend API | https://localhost:5298 |
| Swagger UI | https://localhost:5298/swagger |
| Debug de rutas | https://localhost:5298/debug/routes |

---

## Variables de entorno

### Backend (`API/appsettings.json` + User Secrets)

| Clave | Descripción |
|---|---|
| `ConnectionStrings:DefaultConnection` | Cadena de conexión a SQL Server |
| `JwtSettings:Key` | Clave secreta para firmar tokens JWT (mínimo 32 caracteres) |
| `JwtSettings:Issuer` | Emisor del token JWT |
| `JwtSettings:Audience` | Audiencia del token JWT |
| `SriSign:*` | Parámetros de configuración para la firma electrónica del SRI |
| `Payphone:*` | Credenciales de la pasarela de pago Payphone |
| `EmailSettings:*` | Configuración del servidor SMTP para envío de correos |

En entornos de desarrollo, los valores sensibles se gestionan con `dotnet user-secrets` y nunca se commitean al repositorio.

---

## Roles de usuario

El sistema implementa autorización basada en roles. Cada rol tiene acceso a un conjunto específico de funcionalidades:

| Rol | Acceso |
|---|---|
| `Administrador` | Panel completo: usuarios, configuración de empresa, SRI, todos los módulos |
| `Gestor_Productos` | Alta, edición y gestión del catálogo de productos y categorías |
| `Encargado_Inventario` | Control de stock, números de serie, movimientos y proveedores |
| `Revisor` | Revisión técnica de productos reclamados |
| `Tecnico` | Diagnóstico y resolución de reclamos asignados |
| `Personal de Entrega` | Gestión de entregas y generación de comprobantes de reemplazo |
| `Analista_Datos` | Acceso al dashboard con métricas y reportes |
| `Cliente` | Tienda virtual, carrito, historial de compras y reclamos propios |

---

## Estructura del proyecto

```
ElectroHome/
├── API/                              # Capa de presentación — Minimal API endpoints
│   ├── Certificados/                 # Certificados para firma electrónica
│   ├── Documents/                    # Documentos generados (comprobantes PDF)
│   ├── Program.cs                    # Punto de entrada y configuración de servicios
│   └── API.csproj
│
├── Application/                      # Capa de aplicación — DTOs y contratos
│   └── DTOs/
│       ├── Admin/                    # DTOs de configuración de empresa
│       ├── Auth/                     # DTOs de autenticación y registro
│       ├── Ecommerce/                # DTOs de carrito, productos y categorías
│       ├── Inventario/               # DTOs de movimientos, series y proveedores
│       ├── Productos/                # DTOs de gestión del catálogo
│       ├── Reclamos/                 # DTOs del flujo completo de reclamos
│       │   ├── Cliente/
│       │   ├── Entrega/
│       │   ├── Reclamo/
│       │   ├── Tecnico/
│       │   └── User/
│       └── User/                     # DTOs de perfil de usuario
│
├── Domain/                           # Capa de dominio — entidades del negocio
│
├── Infrastructure/                   # Capa de infraestructura — implementaciones
│   ├── Data/                         # DbContext de Entity Framework
│   ├── Facturacion/                  # Servicio de facturación electrónica SRI
│   │   ├── Helpers/                  # Generación de clave de acceso
│   │   ├── Models/                   # Modelo de factura
│   │   └── Services/                 # Firma, formatter y envío al SRI
│   ├── Models/                       # Entidades mapeadas a tablas de BD
│   ├── Payphone/                     # Integración con pasarela de pago
│   ├── Reclamos/
│   │   ├── Interfaces/               # Contratos de los servicios
│   │   └── Services/                 # Implementación de la lógica de negocio
│   ├── Connected Services/           # Referencias a web services SOAP del SRI
│   │   ├── SriAutorizacion/
│   │   └── SriRecepcion/
│   └── WcfInspectors/                # Interceptores para depuración de SOAP
│
├── frontend/                         # Capa de presentación web
│   ├── components/                   # Componentes por módulo
│   │   ├── Admin/
│   │   ├── Analista/
│   │   ├── Auth/
│   │   ├── Cliente/
│   │   ├── Ecommerce/
│   │   ├── Entrega/
│   │   ├── Factura/
│   │   ├── Inventario/
│   │   ├── Payphone/
│   │   ├── Productos/
│   │   ├── Reclamo/
│   │   └── Tecnico/
│   ├── services/                     # Capa de acceso a la API REST
│   ├── src/
│   │   ├── context/                  # Context API (tema visual)
│   │   ├── hooks/                    # Custom hooks
│   │   ├── theme/                    # Configuración de tema MUI
│   │   ├── types/                    # Tipos TypeScript por dominio
│   │   └── App.tsx                   # Componente raíz y enrutamiento
│   └── vite.config.ts
│
├── start-dev.ps1                     # Script de inicio para desarrollo
└── ElectroHomeGuayaquil.slnx         # Solución de Visual Studio
```

---

## Documentación de la API

En entorno de desarrollo, Swagger UI está disponible en `https://localhost:5298/swagger`. Expone todos los endpoints organizados por dominio con sus parámetros, esquemas de request/response y soporte para autenticación JWT directamente desde la interfaz.

Los principales grupos de endpoints son:

- `/api/auth/*` — Autenticación, registro y recuperación de contraseña
- `/api/admin/*` — Gestión de usuarios y configuración empresarial
- `/api/productos/*` — CRUD del catálogo de productos
- `/api/inventario/*` — Movimientos, números de serie y ubicaciones
- `/api/ecommerce/*` — Tienda virtual y carrito de compras
- `/api/reclamos/*` — Flujo completo de posventa
- `/api/entrega/*` — Gestión de entregas y comprobantes
- `/api/facturacion/*` — Emisión y consulta de comprobantes electrónicos
- `/api/payphone/*` — Inicialización y confirmación de pagos
- `/api/analista/*` — Métricas y datos del dashboard

---

## Preguntas frecuentes

**¿Cómo se maneja la autenticación?**
El sistema usa JWT Bearer. Al iniciar sesión se emite un token firmado con una clave secreta. Ese token se incluye en el header `Authorization: Bearer <token>` de cada petición protegida. El frontend lo almacena en memoria mediante Context API y lo adjunta automáticamente desde la capa de servicios.

**¿Qué versión de .NET se requiere?**
El proyecto usa .NET 10. Es importante tener instalado el SDK correcto. Puedes tenerlo junto a versiones anteriores sin conflictos; .NET permite múltiples versiones lado a lado.

**¿Cómo funciona la facturación electrónica con el SRI?**
Se genera el XML de la factura según el formato definido por el SRI, se firma digitalmente usando el certificado `.p12` almacenado en `API/Certificados/`, y se envía al web service SOAP del SRI en dos pasos: recepción y autorización. La integración usa WCF con un inspector de mensajes personalizado para capturar las respuestas XML del SRI.

**¿Qué pasa si el SRI no responde?**
El sistema registra el error y guarda el estado del comprobante como pendiente. Se puede reintentar el envío desde el panel de administración una vez que los servicios del SRI estén disponibles.

**¿Cómo se controla el inventario por número de serie?**
Cada unidad física de un producto se registra con su número de serie único. Los movimientos de entrada y salida quedan auditados en la tabla `InventarioMovimiento`. Cuando se asocia un reclamo o una venta, el sistema vincula el número de serie específico de la unidad transaccionada.

**¿Cómo funciona el flujo de reclamos?**
El cliente presenta el reclamo indicando el producto y el motivo. El sistema valida que el cliente y el producto existan en el historial de compras. Un técnico recibe el producto, realiza la revisión y registra el diagnóstico. Según el resultado, se puede aprobar un reemplazo o un reembolso. El personal de entrega gestiona la entrega del producto de reemplazo y genera el comprobante digital.

**¿Es posible usar una base de datos distinta a SQL Server?**
El proyecto está configurado para SQL Server mediante EF Core. Migrar a PostgreSQL o MySQL requeriría cambiar el proveedor en `Infrastructure.csproj` y ajustar las migraciones, pero la arquitectura lo permite sin modificar las capas de Application o Domain.

**¿Cómo se generan los comprobantes PDF?**
Se usa QuestPDF (licencia Community) para la generación de documentos estructurados como los comprobantes de entrega. DinkToPdf se utiliza para convertir plantillas HTML a PDF cuando se requiere mayor control visual sobre el diseño del comprobante.

---

## Autor

**Walter Alejandro Duchi Rivera**

Desarrollador Full Stack con experiencia en React, .NET y arquitecturas orientadas a eventos con WebSockets.

- GitHub: [@WalterDuchi](https://github.com/Walter-Duchi)
- LinkedIn: [linkedin.com/in/walter-duchi](https://www.linkedin.com/in/walter-duchi/)
- Portafolio Profesional: [Walter Duchi](https://portafolio-theta-ten-87.vercel.app/)
- Correo: [waltduchi@gmail.com](mailto:waltduchi@gmail.com)
- WhatsApp: [+593 993 516 268](https://wa.me/593993516268)

---

*Proyecto desarrollado como parte del portafolio profesional.*
