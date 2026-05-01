# Huellario

Ecommerce para venta de productos de animales. Stack: **Next.js** (frontend) + **.NET 8** (backend).

## Estructura

```
huellario/
├── client/          # Next.js frontend (puerto 3000)
└── server/          # Solución .NET con Clean Architecture
    ├── domain/           # Entidades, reglas de negocio puras
    ├── application/      # Casos de uso, servicios de aplicación
    ├── infrastructure/   # DB, servicios externos
    └── server/          # Web API (Controllers)
```

## Clean Architecture - Capas .NET

```
server (Web API)          → Interface Adapters
application (ClassLib)    → Application Business Rules
domain (ClassLib)         → Enterprise Business Rules
infrastructure (ClassLib)  → Frameworks & Drivers
```

**Dependencias:** `domain ← application ← server` y `domain ← infrastructure ← server`

## Comandos

### Agregar referencias entre proyectos
```bash
cd server
dotnet add server/server.csproj reference application/application.csproj infrastructure/infrastructure.csproj
dotnet add application/application.csproj reference domain/domain.csproj
dotnet add infrastructure/infrastructure.csproj reference domain/domain.csproj
```

### Ejecutar
```bash
# Backend
cd server && dotnet run --project server/server.csproj

# Frontend
cd client && npm run dev
```