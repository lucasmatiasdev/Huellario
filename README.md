<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 1200 200" width="400" height="70">
  <text x="50%" y="55%" dominant-baseline="middle" text-anchor="middle" font-family="system-ui, sans-serif" font-size="72" font-weight="800" fill="#ea580c">Huellario</text>
  <text x="50%" y="80%" dominant-baseline="middle" text-anchor="middle" font-family="system-ui, sans-serif" font-size="20" fill="#78716c" letter-spacing="6">ECOMERCE DE ACCESORIOS CANINOS</text>
</svg>

<div align="center">

[![Next.js](https://img.shields.io/badge/Next.js-16-black?style=for-the-badge&logo=next.js)](https://nextjs.org/)
[![.NET](https://img.shields.io/badge/.NET-8-512BD4?style=for-the-badge&logo=dotnet)](https://dotnet.microsoft.com/)
[![TypeScript](https://img.shields.io/badge/TypeScript-5-3178C6?style=for-the-badge&logo=typescript)](https://www.typescriptlang.org/)
[![Tailwind CSS](https://img.shields.io/badge/Tailwind-4-06B6D4?style=for-the-badge&logo=tailwindcss)](https://tailwindcss.com/)

</div>

---

## Sobre Huellario

Ecommerce B2C especializado en **accesorios para perros** — collares, camas, juguetes, ropa, comederos y transporte. Venta directa al consumidor final, con productos de **marca propia y de terceros**, alcance local, envío a domicilio y retiro en punto de retiro.

## Stack

| Capa      | Tecnología                                                    |
| --------- | ------------------------------------------------------------- |
| Frontend  | Next.js 16, React 19, TypeScript, Tailwind CSS v4, ShadcnUI   |
| Backend   | .NET 8, ASP.NET Core Web API, Entity Framework Core           |
| DB        | SQL Server / PostgreSQL                                       |
| Auth      | JWT + ASP.NET Core Identity                                   |
| Pagos     | Mercado Pago API                                              |
| Estado    | Zustand (frontend)                                            |
| Validación| Zod + FluentValidation                                        |

## Estructura del proyecto

```
huellario/
├── client/           # Frontend Next.js
│   ├── app/          # App Router (páginas)
│   ├── components/   # UI y componentes (ShadcnUI)
│   ├── stores/       # Zustand stores
│   ├── schemas/      # Esquemas Zod
│   └── lib/          # Utilidades y API client
│
├── server/           # Backend .NET (Clean Architecture)
│   ├── domain/       # Entidades, enums, reglas de negocio
│   ├── application/  # DTOs, servicios, validadores
│   ├── infrastructure/ # DbContext, repositorios, pagos
│   └── server/       # Web API (controllers)
│
├── PRD.md            # Documento de producto
└── README.md
```

## Funcionalidades principales

- Catálogo con filtros y búsqueda
- Carrito de compras persistente (registrados e invitados)
- Registro, login y compra como invitado
- Checkout con múltiples métodos de pago
- Panel de administración (productos, pedidos, stock, usuarios)
- Control de inventario por variante (talla, color)

## Roles

| Rol           | Acceso                                       |
| ------------- | -------------------------------------------- |
| Invitado      | Catálogo, carrito, checkout sin registro     |
| Cliente       | Perfil, historial de pedidos, direcciones    |
| Administrador | Gestión completa de productos, pedidos, usuarios |

## Cómo empezar

### Backend

```bash
cd server
dotnet restore
dotnet run --project server/server.csproj
```

API disponible en `http://localhost:5029` — Swagger en `/swagger`.

### Frontend

```bash
cd client
npm install
npm run dev
```

App disponible en `http://localhost:3000`.

## Documentación

- [PRD — Documento de producto](PRD.md)
- [Plan Backend](server/PLAN.md) — entidades, endpoints, DB schema
- [Plan Frontend](client/PLAN.md) — rutas, stores, componentes

## Licencia

Proyecto privado — todos los derechos reservados.
