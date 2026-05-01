# Plan de proyecto - Backend (Huellario)

## 1. Stack tecnológico

| Tecnología               | Propósito                                           |
| ------------------------ | --------------------------------------------------- |
| .NET 8                   | Runtime / Framework principal                       |
| ASP.NET Core Web API     | Capa de presentación (REST)                         |
| Entity Framework Core 8  | ORM para acceso a datos                             |
| SQL Server / PostgreSQL  | Base de datos relacional                            |
| JWT (JSON Web Token)     | Autenticación stateless                             |
| ASP.NET Core Identity    | Gestión de usuarios y roles                         |
| Mapster                  | Mapeo entre entidades y DTOs                        |
| FluentValidation         | Validación de requests                              |
| MediatR (opcional)       | Patrón mediator / CQRS                              |
| Swagger / Swashbuckle    | Documentación interactiva de API                    |
| Mercado Pago SDK         | Integración de pagos                                |
| Docker                   | Contenedorización                                   |
| xUnit + Moq              | Tests unitarios y de integración                    |

## 2. Arquitectura (Clean Architecture)

```
┌─────────────────────────────────────────────────┐
│                   server (Web API)               │
│  Controllers · Middleware · Program.cs           │
│  Depende de: application, infrastructure         │
├─────────────────────────────────────────────────┤
│                application (ClassLib)             │
│  DTOs · Services · Interfaces · Validators       │
│  Depende de: domain                              │
├─────────────────────────────────────────────────┤
│                 domain (ClassLib)                 │
│  Entities · Enums · Value Objects · Interfaces   │
│  Sin dependencias externas                       │
├─────────────────────────────────────────────────┤
│              infrastructure (ClassLib)            │
│  DbContext · Repositories · External Services    │
│  Depende de: domain                              │
└─────────────────────────────────────────────────┘
```

## 3. Modelo de dominio (entidades y relaciones)

### 3.1 Entidades principales

```
┌─────────────┐     ┌──────────────┐     ┌─────────────┐
│  Categoria   │     │   Marca      │     │   Usuario    │
└──────┬──────┘     └──────┬───────┘     └──────┬───────┘
       │                   │                     │
       │ 1:N               │ 1:N                 │ 1:N
       ▼                   ▼                     ▼
┌─────────────────────────────────────────┐     ┌─────────────┐
│              Producto                    │     │  Direccion   │
│  CategoriaId · MarcaId · Nombre ·       │     └──────┬───────┘
│  Descripcion · Precio · Imagenes ·      │            │
│  EsMarcaPropia · Activo                  │            │ 1:N
└──────────┬──────────────────────────────┘            │
           │ 1:N                                       ▼
           ▼                                   ┌──────────────┐
     ┌────────────┐                           │   Pedido      │
     │  Variante   │◄──────────────────────────┘              │
     │ Talla/Color │   1:N                    │ UsuarioId?    │
     │ Stock ·     │                          │ Fecha · Total │
     │ PrecioExtra?│                          │ Estado ·      │
     └────────────┘                          │ DireccionId   │
                                              └──────┬───────┘
                                                     │ 1:N
                                                     ▼
                                            ┌──────────────────┐
                                            │ LineaPedido      │
                                            │ ProductoId ·     │
                                            │ VarianteId? ·    │
                                            │ Cantidad ·       │
                                            │ PrecioUnitario   │
                                            └──────────────────┘

┌──────────────┐     ┌──────────────────┐
│   Pago        │     │   CarritoItem    │
│ PedidoId ·    │     │ UsuarioId (o     │
│ Monto ·       │     │   sesionId) ·     │
│ Metodo ·      │     │ ProductoId ·     │
│ Estado ·      │     │ VarianteId? ·    │
│ Referencia    │     │ Cantidad         │
└──────────────┘     └──────────────────┘
```

### 3.2 Enumeraciones

| Enum            | Valores                                                    |
| --------------- | ---------------------------------------------------------- |
| EstadoPedido    | Pendiente, Confirmado, EnEnvio, Entregado, Cancelado, Reembolsado |
| MetodoPago      | TarjetaCredito, TarjetaDebito, Transferencia, MercadoPago, Efectivo |
| EstadoPago      | Pendiente, Aprobado, Rechazado, Reembolsado                |
| RolUsuario      | Cliente, Administrador                                     |
| TipoDireccion   | Envio, Facturacion                                         |

### 3.3 Propiedades clave por entidad

**Producto**
- Id, CategoriaId, MarcaId
- Nombre, Descripcion, Precio (decimal), Imagenes (List<string>)
- EsMarcaPropia (bool), Activo (bool), FechaCreacion, FechaActualizacion

**Variante**
- Id, ProductoId
- Nombre (ej: "Talla S", "Color Rojo"), Stock (int), PrecioExtra (decimal?)

**Pedido**
- Id, UsuarioId (nullable para invitados), DireccionId (nullable para retiro)
- FechaPedido, Total (decimal), Estado (EstadoPedido)
- EsRetiro (bool), Notas (string?)

**LineaPedido**
- Id, PedidoId, ProductoId, VarianteId (nullable)
- Cantidad (int), PrecioUnitario (decimal)

**Pago**
- Id, PedidoId
- Monto (decimal), Metodo (MetodoPago), Estado (EstadoPago)
- ReferenciaExterna (string?) — ej: ID de transacción de Mercado Pago

**Usuario**
- Id (guid), Email, Nombre, Apellido, Telefono
- AspNetIdentityId (para vinculación con Identity)
- FechaRegistro

**Direccion**
- Id, UsuarioId (nullable para invitados)
- Calle, Numero, Ciudad, CodigoPostal, Referencia (string?)
- Tipo (TipoDireccion), EsPredeterminada (bool)

## 4. Esquema de base de datos

### 4.1 Tablas y relaciones

```sql
-- Tabla: Categorias
CREATE TABLE Categorias (
    Id          INT           PRIMARY KEY IDENTITY(1,1),
    Nombre      NVARCHAR(100) NOT NULL,
    Slug        NVARCHAR(100) NOT NULL UNIQUE,
    Descripcion NVARCHAR(500) NULL,
    ImagenUrl   NVARCHAR(500) NULL,
    Activo      BIT           NOT NULL DEFAULT 1
);

-- Tabla: Marcas
CREATE TABLE Marcas (
    Id          INT           PRIMARY KEY IDENTITY(1,1),
    Nombre      NVARCHAR(100) NOT NULL,
    Slug        NVARCHAR(100) NOT NULL UNIQUE,
    LogoUrl     NVARCHAR(500) NULL,
    Activo      BIT           NOT NULL DEFAULT 1
);

-- Tabla: Productos
CREATE TABLE Productos (
    Id              INT            PRIMARY KEY IDENTITY(1,1),
    CategoriaId     INT            NOT NULL,
    MarcaId         INT            NOT NULL,
    Nombre          NVARCHAR(200)  NOT NULL,
    Slug            NVARCHAR(200)  NOT NULL UNIQUE,
    Descripcion     NVARCHAR(MAX)  NULL,
    Precio          DECIMAL(18,2)  NOT NULL,
    EsMarcaPropia   BIT            NOT NULL DEFAULT 0,
    Activo          BIT            NOT NULL DEFAULT 1,
    FechaCreacion   DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
    FechaActualizacion DATETIME2   NULL,

    CONSTRAINT FK_Productos_Categorias FOREIGN KEY (CategoriaId) REFERENCES Categorias(Id),
    CONSTRAINT FK_Productos_Marcas FOREIGN KEY (MarcaId) REFERENCES Marcas(Id)
);

-- Tabla: ProductoImagenes
CREATE TABLE ProductoImagenes (
    Id          INT            PRIMARY KEY IDENTITY(1,1),
    ProductoId  INT            NOT NULL,
    Url         NVARCHAR(500)  NOT NULL,
    Orden       INT            NOT NULL DEFAULT 0,

    CONSTRAINT FK_ProdImagenes_Productos FOREIGN KEY (ProductoId) REFERENCES Productos(Id) ON DELETE CASCADE
);

-- Tabla: Variantes
CREATE TABLE Variantes (
    Id          INT            PRIMARY KEY IDENTITY(1,1),
    ProductoId  INT            NOT NULL,
    Nombre      NVARCHAR(100)  NOT NULL,
    Stock       INT            NOT NULL DEFAULT 0,
    PrecioExtra DECIMAL(18,2)  NULL DEFAULT 0,

    CONSTRAINT FK_Variantes_Productos FOREIGN KEY (ProductoId) REFERENCES Productos(Id) ON DELETE CASCADE
);

-- Tabla: Usuarios (extensión de Identity)
CREATE TABLE Usuarios (
    Id              INT            PRIMARY KEY IDENTITY(1,1),
    IdentityId      NVARCHAR(450)  NOT NULL,
    Nombre          NVARCHAR(100)  NOT NULL,
    Apellido        NVARCHAR(100)  NOT NULL,
    Telefono        NVARCHAR(20)   NULL,
    FechaRegistro   DATETIME2      NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT FK_Usuarios_AspNetUsers FOREIGN KEY (IdentityId) REFERENCES AspNetUsers(Id)
);

-- Tabla: Direcciones
CREATE TABLE Direcciones (
    Id              INT            PRIMARY KEY IDENTITY(1,1),
    UsuarioId       INT            NULL,
    PedidoInvitadoId INT           NULL,  -- para guest checkout
    Calle           NVARCHAR(200)  NOT NULL,
    Numero          NVARCHAR(20)   NOT NULL,
    Complemento     NVARCHAR(200)  NULL,
    Ciudad          NVARCHAR(100)  NOT NULL,
    Provincia       NVARCHAR(100)  NULL,
    CodigoPostal    NVARCHAR(20)   NOT NULL,
    Referencia      NVARCHAR(500)  NULL,
    EsPredeterminada BIT           NOT NULL DEFAULT 0,
    Tipo            INT            NOT NULL DEFAULT 0,  -- 0=envio, 1=facturacion

    CONSTRAINT FK_Direcciones_Usuarios FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id)
);

-- Tabla: Pedidos
CREATE TABLE Pedidos (
    Id              INT            PRIMARY KEY IDENTITY(1,1),
    UsuarioId       INT            NULL,
    DireccionId     INT            NULL,
    FechaPedido     DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
    Total           DECIMAL(18,2)  NOT NULL,
    Estado          INT            NOT NULL DEFAULT 0,  -- EstadoPedido enum
    EsRetiro        BIT            NOT NULL DEFAULT 0,
    Notas           NVARCHAR(MAX)  NULL,
    NumeroSeguimiento NVARCHAR(100) NULL,

    CONSTRAINT FK_Pedidos_Usuarios FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id),
    CONSTRAINT FK_Pedidos_Direcciones FOREIGN KEY (DireccionId) REFERENCES Direcciones(Id)
);

-- Tabla: LineasPedido
CREATE TABLE LineasPedido (
    Id              INT            PRIMARY KEY IDENTITY(1,1),
    PedidoId        INT            NOT NULL,
    ProductoId      INT            NOT NULL,
    VarianteId      INT            NULL,
    Cantidad        INT            NOT NULL,
    PrecioUnitario  DECIMAL(18,2)  NOT NULL,

    CONSTRAINT FK_Lineas_Pedidos FOREIGN KEY (PedidoId) REFERENCES Pedidos(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Lineas_Productos FOREIGN KEY (ProductoId) REFERENCES Productos(Id),
    CONSTRAINT FK_Lineas_Variantes FOREIGN KEY (VarianteId) REFERENCES Variantes(Id)
);

-- Tabla: Pagos
CREATE TABLE Pagos (
    Id                  INT            PRIMARY KEY IDENTITY(1,1),
    PedidoId            INT            NOT NULL,
    Monto               DECIMAL(18,2)  NOT NULL,
    Metodo              INT            NOT NULL,  -- MetodoPago enum
    Estado              INT            NOT NULL DEFAULT 0,  -- EstadoPago enum
    ReferenciaExterna   NVARCHAR(200)  NULL,
    FechaCreacion       DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
    FechaConfirmacion   DATETIME2      NULL,

    CONSTRAINT FK_Pagos_Pedidos FOREIGN KEY (PedidoId) REFERENCES Pedidos(Id)
);

-- Tabla: CarritoItems
CREATE TABLE CarritoItems (
    Id              INT            PRIMARY KEY IDENTITY(1,1),
    UsuarioId       INT            NULL,     -- NULL para invitados
    SesionId        NVARCHAR(200)  NULL,     -- para carrito de invitados
    ProductoId      INT            NOT NULL,
    VarianteId      INT            NULL,
    Cantidad        INT            NOT NULL DEFAULT 1,
    FechaAgregado   DATETIME2      NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT FK_Carrito_Productos FOREIGN KEY (ProductoId) REFERENCES Productos(Id),
    CONSTRAINT FK_Carrito_Variantes FOREIGN KEY (VarianteId) REFERENCES Variantes(Id)
);
```

### 4.2 Diagrama ER (resumen)

```
Categorias ──1:N── Productos ──1:N── Variantes
Marcas ──────1:N── Productos
                    │
                    │ 1:N
                    ▼
              LineasPedido ──N:1── Pedidos ──1:1── Pagos
                    │                 │
                    │                 │ 1:N
                    ▼                 ▼
              Variantes         Direcciones ──N:1── Usuarios
              Productos
```

## 5. API endpoints

### 5.1 Autenticación y usuarios

| Método | Ruta                          | Acción                     | Auth     | Rol     |
| ------ | ----------------------------- | -------------------------- | -------- | ------- |
| POST   | /api/auth/register            | Registrar nuevo usuario    | No       | -       |
| POST   | /api/auth/login               | Iniciar sesión (JWT)       | No       | -       |
| POST   | /api/auth/refresh             | Refrescar token            | No       | -       |
| POST   | /api/auth/forgot-password     | Solicitar reset password   | No       | -       |
| POST   | /api/auth/reset-password      | Resetear password          | No       | -       |
| GET    | /api/usuarios/me              | Perfil del usuario actual  | JWT      | Cliente |
| PUT    | /api/usuarios/me              | Editar perfil              | JWT      | Cliente |
| GET    | /api/usuarios                 | Listar usuarios            | JWT      | Admin   |

### 5.2 Productos

| Método | Ruta                                      | Acción                  | Auth     | Rol     |
| ------ | ----------------------------------------- | ----------------------- | -------- | ------- |
| GET    | /api/productos                            | Listar productos (paginado, filtros) | No | -    |
| GET    | /api/productos/{slug}                     | Detalle de producto     | No       | -       |
| POST   | /api/productos                            | Crear producto          | JWT      | Admin   |
| PUT    | /api/productos/{id}                       | Actualizar producto     | JWT      | Admin   |
| DELETE | /api/productos/{id}                       | Eliminar (baja lógica)  | JWT      | Admin   |
| POST   | /api/productos/{id}/imagenes              | Agregar imagen          | JWT      | Admin   |
| DELETE | /api/productos/{id}/imagenes/{imagenId}   | Eliminar imagen         | JWT      | Admin   |

### 5.3 Variantes

| Método | Ruta                                              | Acción              | Auth     | Rol     |
| ------ | ------------------------------------------------- | ------------------- | -------- | ------- |
| GET    | /api/productos/{productoId}/variantes             | Listar variantes    | No       | -       |
| POST   | /api/productos/{productoId}/variantes             | Crear variante      | JWT      | Admin   |
| PUT    | /api/productos/{productoId}/variantes/{id}        | Actualizar variante | JWT      | Admin   |
| DELETE | /api/productos/{productoId}/variantes/{id}        | Eliminar variante   | JWT      | Admin   |
| PATCH  | /api/productos/{productoId}/variantes/{id}/stock  | Actualizar stock    | JWT      | Admin   |

### 5.4 Categorías

| Método | Ruta                     | Acción               | Auth     | Rol     |
| ------ | ------------------------ | -------------------- | -------- | ------- |
| GET    | /api/categorias          | Listar categorías    | No       | -       |
| GET    | /api/categorias/{slug}   | Detalle + productos  | No       | -       |
| POST   | /api/categorias          | Crear categoría      | JWT      | Admin   |
| PUT    | /api/categorias/{id}     | Actualizar categoría | JWT      | Admin   |
| DELETE | /api/categorias/{id}     | Eliminar categoría   | JWT      | Admin   |

### 5.5 Marcas

| Método | Ruta                     | Acción            | Auth     | Rol     |
| ------ | ------------------------ | ----------------- | -------- | ------- |
| GET    | /api/marcas              | Listar marcas     | No       | -       |
| GET    | /api/marcas/{slug}       | Detalle + productos | No     | -       |
| POST   | /api/marcas              | Crear marca       | JWT      | Admin   |
| PUT    | /api/marcas/{id}         | Actualizar marca  | JWT      | Admin   |
| DELETE | /api/marcas/{id}         | Eliminar marca    | JWT      | Admin   |

### 5.6 Carrito

| Método | Ruta                          | Acción                     | Auth        |
| ------ | ----------------------------- | -------------------------- | ----------- |
| GET    | /api/carrito                  | Obtener carrito actual     | JWT/Sesión  |
| POST   | /api/carrito/items            | Agregar item al carrito    | JWT/Sesión  |
| PUT    | /api/carrito/items/{id}       | Actualizar cantidad        | JWT/Sesión  |
| DELETE | /api/carrito/items/{id}       | Quitar item del carrito    | JWT/Sesión  |
| DELETE | /api/carrito                  | Vaciar carrito             | JWT/Sesión  |

### 5.7 Pedidos

| Método | Ruta                          | Acción                     | Auth     | Rol     |
| ------ | ----------------------------- | -------------------------- | -------- | ------- |
| POST   | /api/pedidos                  | Crear pedido (checkout)    | JWT/Sesión | -    |
| GET    | /api/pedidos                  | Listar pedidos (propios)   | JWT      | Cliente |
| GET    | /api/pedidos/{id}             | Detalle de pedido          | JWT      | Cliente |
| GET    | /api/admin/pedidos            | Listar todos los pedidos   | JWT      | Admin   |
| GET    | /api/admin/pedidos/{id}       | Detalle de pedido (admin)  | JWT      | Admin   |
| PUT    | /api/admin/pedidos/{id}/estado| Cambiar estado del pedido  | JWT      | Admin   |
| POST   | /api/pedidos/{id}/cancelar    | Cancelar pedido            | JWT      | Cliente |

### 5.8 Pagos

| Método | Ruta                          | Acción                         | Auth     |
| ------ | ----------------------------- | ------------------------------ | -------- |
| POST   | /api/pagos/mercado-pago       | Crear preferencia de pago MP   | JWT/Sesión |
| POST   | /api/pagos/mercado-pago/webhook | Webhook de confirmación MP   | No       |
| POST   | /api/pagos/transferencia      | Reportar transferencia realizada | JWT/Sesión |
| PUT    | /api/admin/pagos/{id}/confirmar | Confirmar pago manualmente   | JWT      | Admin   |

### 5.9 Direcciones

| Método | Ruta                                 | Acción                   | Auth | Rol     |
| ------ | ------------------------------------ | ------------------------ | ---- | ------- |
| GET    | /api/usuarios/me/direcciones         | Listar direcciones       | JWT  | Cliente |
| POST   | /api/usuarios/me/direcciones         | Agregar dirección        | JWT  | Cliente |
| PUT    | /api/usuarios/me/direcciones/{id}    | Editar dirección         | JWT  | Cliente |
| DELETE | /api/usuarios/me/direcciones/{id}    | Eliminar dirección       | JWT  | Cliente |
| PATCH  | /api/usuarios/me/direcciones/{id}/predeterminada | Marcar como predeterminada | JWT | Cliente |

## 6. Autenticación y autorización

- **Mecanismo:** JWT (JSON Web Token) con tokens de acceso y refresh.
- **Flujo:**
  1. El cliente envía credenciales (email + password) a `/api/auth/login`.
  2. El servidor valida contra ASP.NET Core Identity y devuelve `access_token` (15 min) + `refresh_token` (7 días).
  3. El cliente incluye el access token en el header `Authorization: Bearer <token>`.
  4. Para endpoints de invitado, se usa un `SesionId` generado en el frontend y enviado como header `X-Sesion-Id`.
- **Roles:** `Cliente` y `Administrador`. Se asignan mediante `[Authorize(Roles = "Administrador")]`.

## 7. Reglas de negocio (validaciones backend)

1. Un producto sin stock ni variantes no puede tener stock > 0. Si tiene variantes, el stock se maneja por variante.
2. Al confirmar un pedido, se descuenta el stock de cada línea. Si no hay stock suficiente, el pedido se rechaza.
3. Un pedido en estado `Pendiente` se cancela automáticamente si no se confirma el pago en 24h (job programado).
4. Los pagos con `Transferencia` o `Efectivo` requieren confirmación manual del admin.
5. Los pagos con `MercadoPago` o `Tarjeta` se procesan vía API externa y se confirman automáticamente.
6. El total del pedido se calcula en backend al momento de crear el pedido (nunca se confía en el precio enviado desde el frontend).
7. Un usuario no puede tener más de 5 direcciones guardadas.

## 8. Estructura de carpetas (código)

```
server/
├── domain/
│   ├── Entities/
│   │   ├── Producto.cs
│   │   ├── Variante.cs
│   │   ├── Categoria.cs
│   │   ├── Marca.cs
│   │   ├── Pedido.cs
│   │   ├── LineaPedido.cs
│   │   ├── Pago.cs
│   │   ├── Usuario.cs
│   │   ├── Direccion.cs
│   │   └── CarritoItem.cs
│   ├── Enums/
│   │   ├── EstadoPedido.cs
│   │   ├── MetodoPago.cs
│   │   ├── EstadoPago.cs
│   │   ├── RolUsuario.cs
│   │   └── TipoDireccion.cs
│   ├── ValueObjects/
│   │   └── Precio.cs
│   └── Interfaces/
│       ├── IProductoRepository.cs
│       ├── IPedidoRepository.cs
│       └── IUnitOfWork.cs
│
├── application/
│   ├── DTOs/
│   │   ├── Producto/
│   │   ├── Pedido/
│   │   ├── Usuario/
│   │   └── Carrito/
│   ├── Services/
│   │   ├── ProductoService.cs
│   │   ├── PedidoService.cs
│   │   ├── CarritoService.cs
│   │   ├── AuthService.cs
│   │   └── PagoService.cs
│   ├── Interfaces/
│   │   ├── IProductoService.cs
│   │   ├── IPedidoService.cs
│   │   └── IPagoService.cs
│   ├── Mappings/
│   │   └── MappingProfile.cs
│   └── Validators/
│       ├── CreateProductoValidator.cs
│       └── CreatePedidoValidator.cs
│
├── infrastructure/
│   ├── Data/
│   │   ├── HuellarioDbContext.cs
│   │   └── Configurations/
│   │       ├── ProductoConfiguration.cs
│   │       ├── PedidoConfiguration.cs
│   │       └── ...
│   ├── Repositories/
│   │   ├── ProductoRepository.cs
│   │   └── PedidoRepository.cs
│   ├── Services/
│   │   ├── MercadoPagoService.cs
│   │   └── EmailService.cs
│   └── Identity/
│       └── HuellarioIdentityUser.cs
│
└── server/
    ├── Controllers/
    │   ├── AuthController.cs
    │   ├── ProductosController.cs
    │   ├── CategoriasController.cs
    │   ├── MarcasController.cs
    │   ├── CarritoController.cs
    │   ├── PedidosController.cs
    │   ├── PagosController.cs
    │   ├── UsuariosController.cs
    │   └── Admin/
    │       ├── PedidosController.cs
    │       └── UsuariosController.cs
    ├── Middleware/
    │   └── ExceptionMiddleware.cs
    └── Program.cs
```

## 9. Paquetes NuGet requeridos

| Proyecto       | Paquete                                       |
| -------------- | --------------------------------------------- |
| infrastructure | Microsoft.EntityFrameworkCore                  |
| infrastructure | Microsoft.EntityFrameworkCore.SqlServer        |
| infrastructure | Microsoft.AspNetCore.Identity.EntityFrameworkCore |
| server         | Microsoft.AspNetCore.Authentication.JwtBearer |
| server         | Microsoft.AspNetCore.Identity.UI              |
| application    | Mapster                                              |
| application    | FluentValidation.AspNetCore                   |
| application    | MediatR (opcional)                            |
| server         | Swashbuckle.AspNetCore (ya agregado)          |
| infrastructure | MercadoPagoSDK (o SDK de Mercado Pago)        |

## 10. Decisiones técnicas

- **Autenticación:** Se usa ASP.NET Core Identity como proveedor de usuarios, con tablas extendidas (`Usuarios` con FK a `AspNetUsers`).
- **Manejo de invitados:** Los carritos de invitados se identifican por `SesionId` (GUID generado en frontend, enviado como header). Al registrarse, se migra el carrito de invitado al usuario.
- **Paginación:** Todos los endpoints de listado soportan paginación (`?page=1&pageSize=20`) y devuelven metadata (total, página actual, total páginas).
- **Manejo de imágenes:** Las imágenes se almacenan como URLs (pueden ser en disco local, S3, o CDN). Se guardan en tabla separada `ProductoImagenes` para permitir múltiples imágenes por producto con orden.
- **Auditoría:** Las entidades `Producto` y `Pedido` tienen timestamps de creación/actualización.
- **Baja lógica:** Los productos se desactivan (`Activo = false`), no se eliminan físicamente, para preservar integridad referencial con pedidos históricos.
