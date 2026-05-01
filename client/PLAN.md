# Plan de proyecto - Frontend (Huellario)

## 1. Stack tecnológico

| Tecnología           | Propósito                                                |
| -------------------- | -------------------------------------------------------- |
| Next.js 16           | Framework React con App Router, SSR, SSG, ISR            |
| React 19             | Librería de UI                                           |
| TypeScript           | Tipado estático                                          |
| Tailwind CSS v4      | Estilos utilitarios                                      |
| ShadcnUI             | Componentes de UI accesibles y personalizables           |
| Zustand              | Manejo de estado global (carrito, auth, UI)              |
| Zod                  | Validación de esquemas (forms, API responses)            |
| React Hook Form      | Manejo de formularios (integrado con Zod)                |
| NextAuth.js          | Manejo de sesión y autenticación (opcional vs JWT manual)|

### Dependencias a instalar

```
npm install zustand zod react-hook-form @hookform/resolvers
npx shadcn@latest init
npx shadcn@latest add button card input label select dialog dropdown-menu
npx shadcn@latest add table badge toast sheet separator avatar
npx shadcn@latest add skeleton tabs form textarea checkbox radio-group
npx shadcn@latest add command popover scroll-area carousel
```

## 2. Estructura de carpetas

```
client/
├── app/
│   ├── layout.tsx                  # Layout raíz (Navbar + Footer)
│   ├── page.tsx                    # Home
│   ├── loading.tsx                 # Loading global
│   ├── error.tsx                   # Error boundary global
│   ├── not-found.tsx               # 404
│   ├── globals.css                 # Estilos globales + Tailwind
│   │
│   ├── (public)/                   # Rutas públicas (sin layout admin)
│   │   ├── layout.tsx              # Layout público (Navbar, Footer)
│   │   ├── productos/
│   │   │   ├── page.tsx            # Catálogo (GET /api/productos)
│   │   │   └── [slug]/
│   │   │       └── page.tsx        # Detalle (GET /api/productos/{slug})
│   │   ├── categorias/
│   │   │   └── [slug]/
│   │   │       └── page.tsx        # Productos por categoría
│   │   ├── carrito/
│   │   │   └── page.tsx            # Carrito
│   │   ├── checkout/
│   │   │   └── page.tsx            # Checkout (POST /api/pedidos)
│   │   ├── pedido/
│   │   │   └── [id]/
│   │   │       └── page.tsx        # Confirmación post-compra
│   │   ├── login/
│   │   │   └── page.tsx            # Login (POST /api/auth/login)
│   │   ├── registro/
│   │   │   └── page.tsx            # Registro (POST /api/auth/register)
│   │   └── recuperar-password/
│   │       └── page.tsx            # Recuperar contraseña
│   │
│   ├── (auth)/                     # Rutas protegidas (cliente)
│   │   ├── layout.tsx              # Verifica JWT
│   │   ├── perfil/
│   │   │   └── page.tsx            # Perfil (GET/PUT /api/usuarios/me)
│   │   ├── direcciones/
│   │   │   └── page.tsx            # CRUD direcciones
│   │   └── pedidos/
│   │       ├── page.tsx            # Historial (GET /api/pedidos)
│   │       └── [id]/
│   │           └── page.tsx        # Detalle (GET /api/pedidos/{id})
│   │
│   └── admin/                      # Rutas protegidas (admin)
│       ├── layout.tsx              # Layout admin (sidebar)
│       ├── page.tsx                # Dashboard
│       ├── productos/
│       │   ├── page.tsx            # Listado (GET /api/productos)
│       │   ├── nuevo/
│       │   │   └── page.tsx        # Crear (POST /api/productos)
│       │   └── [id]/
│       │       ├── page.tsx        # Editar (PUT /api/productos/{id})
│       │       └── variantes/
│       │           └── page.tsx    # Gestionar variantes
│       ├── categorias/
│       │   └── page.tsx            # CRUD categorías
│       ├── marcas/
│       │   └── page.tsx            # CRUD marcas
│       ├── pedidos/
│       │   ├── page.tsx            # Listado (GET /api/admin/pedidos)
│       │   └── [id]/
│       │       └── page.tsx        # Detalle + cambiar estado
│       └── usuarios/
│           └── page.tsx            # Listado (GET /api/usuarios)
│
├── components/
│   ├── ui/                         # Componentes ShadcnUI generados
│   ├── layout/
│   │   ├── Navbar.tsx
│   │   ├── Footer.tsx
│   │   ├── SidebarAdmin.tsx
│   │   └── CartBadge.tsx
│   ├── product/
│   │   ├── ProductCard.tsx
│   │   ├── ProductGrid.tsx
│   │   ├── ProductGallery.tsx
│   │   ├── VariantSelector.tsx
│   │   └── ProductFilters.tsx
│   ├── cart/
│   │   ├── CartItem.tsx
│   │   └── CartSummary.tsx
│   ├── checkout/
│   │   ├── ShippingForm.tsx
│   │   ├── PaymentSelector.tsx
│   │   └── OrderSummary.tsx
│   └── admin/
│       ├── DataTable.tsx
│       ├── ProductForm.tsx
│       ├── OrderStatusBadge.tsx
│       └── StatsCard.tsx
│
├── lib/
│   ├── api-client.ts               # Cliente HTTP (fetch wrapper con JWT)
│   ├── utils.ts                    # Utilidades (cn, formatPrice, etc.)
│   └── constants.ts                # Constantes (rutas, estados, etc.)
│
├── hooks/
│   ├── use-auth.ts                 # Hook de autenticación
│   ├── use-cart.ts                 # Hook de carrito (wrapper de store)
│   └── use-pagination.ts           # Hook de paginación
│
├── stores/
│   ├── auth-store.ts               # Estado de autenticación (Zustand)
│   ├── cart-store.ts               # Estado del carrito (Zustand)
│   └── ui-store.ts                 # Estado UI (modals, toasts, sidebar)
│
├── schemas/                        # Esquemas Zod
│   ├── auth.ts                     # loginSchema, registerSchema
│   ├── producto.ts                 # createProductoSchema
│   ├── pedido.ts                   # checkoutSchema
│   └── direccion.ts               # direccionSchema
│
├── types/                          # Tipos TypeScript
│   ├── api.ts                      # Respuestas de API genéricas
│   ├── product.ts                  # Producto, Variante, Categoria, Marca
│   ├── order.ts                    # Pedido, LineaPedido, EstadoPedido
│   ├── payment.ts                  # Pago, MetodoPago
│   └── user.ts                     # Usuario, Direccion
│
└── public/
    ├── images/
    └── icons/
```

## 3. Mapeo páginas ↔ endpoints backend

### 3.1 Módulo público

| Página               | Endpoint(s) que consume                               | Método |
| -------------------- | ----------------------------------------------------- | ------ |
| Home                 | `GET /api/productos?destacados=true`                  | SSR    |
|                      | `GET /api/categorias`                                 | SSR    |
| Catálogo             | `GET /api/productos?page=&categoria=&marca=&precioMin=&precioMax=&search=` | CSR |
| Detalle producto     | `GET /api/productos/{slug}`                           | SSR    |
|                      | `GET /api/productos/{id}/variantes`                   | CSR    |
| Carrito              | `GET /api/carrito`                                    | CSR    |
| Checkout             | `POST /api/pedidos`                                   | CSR    |
|                      | `POST /api/pagos/mercado-pago` (si corresponde)       | CSR    |
| Confirmación         | `GET /api/pedidos/{id}`                               | SSR    |

### 3.2 Módulo autenticación

| Página               | Endpoint(s) que consume                               | Método |
| -------------------- | ----------------------------------------------------- | ------ |
| Login                | `POST /api/auth/login`                                | CSR    |
| Registro             | `POST /api/auth/register`                             | CSR    |
| Recuperar password   | `POST /api/auth/forgot-password`                      | CSR    |
|                      | `POST /api/auth/reset-password`                       | CSR    |

### 3.3 Módulo cliente (protegido)

| Página               | Endpoint(s) que consume                               | Método |
| -------------------- | ----------------------------------------------------- | ------ |
| Perfil               | `GET /api/usuarios/me` + `PUT /api/usuarios/me`      | CSR    |
| Direcciones          | `GET /api/usuarios/me/direcciones`                    | CSR    |
|                      | `POST /api/usuarios/me/direcciones`                   | CSR    |
|                      | `PUT /api/usuarios/me/direcciones/{id}`               | CSR    |
|                      | `DELETE /api/usuarios/me/direcciones/{id}`            | CSR    |
|                      | `PATCH /api/usuarios/me/direcciones/{id}/predeterminada` | CSR |
| Historial pedidos    | `GET /api/pedidos?page=`                              | CSR    |
| Detalle pedido       | `GET /api/pedidos/{id}`                               | SSR    |
|                      | `POST /api/pedidos/{id}/cancelar`                     | CSR    |

### 3.4 Módulo admin (protegido)

| Página                | Endpoint(s) que consume                                | Método |
| --------------------- | ------------------------------------------------------ | ------ |
| Dashboard             | `GET /api/admin/pedidos?estado=&fecha=` (resumen)     | CSR    |
| Productos (lista)     | `GET /api/productos?page=&search=`                    | CSR    |
| Producto (crear)      | `POST /api/productos`                                 | CSR    |
|                       | `POST /api/productos/{id}/imagenes`                   | CSR    |
| Producto (editar)     | `PUT /api/productos/{id}`                             | CSR    |
|                       | `DELETE /api/productos/{id}/imagenes/{imagenId}`      | CSR    |
| Variantes             | `GET /api/productos/{productoId}/variantes`           | CSR    |
|                       | `POST /api/productos/{productoId}/variantes`          | CSR    |
|                       | `PUT /api/productos/{productoId}/variantes/{id}`      | CSR    |
|                       | `DELETE /api/productos/{productoId}/variantes/{id}`   | CSR    |
|                       | `PATCH /api/productos/{productoId}/variantes/{id}/stock` | CSR |
| Categorías            | `GET /api/categorias` + `POST` + `PUT` + `DELETE`     | CSR    |
| Marcas                | `GET /api/marcas` + `POST` + `PUT` + `DELETE`         | CSR    |
| Pedidos (lista)       | `GET /api/admin/pedidos?page=&estado=&fecha=`         | CSR    |
| Pedido (detalle)      | `GET /api/admin/pedidos/{id}`                         | CSR    |
|                       | `PUT /api/admin/pedidos/{id}/estado`                  | CSR    |
| Usuarios              | `GET /api/usuarios?page=`                             | CSR    |

## 4. Stores de Zustand

### 4.1 auth-store.ts

```typescript
interface AuthState {
  user: Usuario | null;
  token: string | null;
  refreshToken: string | null;
  isAuthenticated: boolean;
  login: (email: string, password: string) => Promise<void>;
  register: (data: RegisterInput) => Promise<void>;
  logout: () => void;
  refreshSession: () => Promise<void>;
}
```

- Persistencia en `localStorage` via `zustand/middleware` (persist).
- Al iniciar sesión, guarda token y datos del usuario.
- `logout()` limpia el store y redirige al login.
- `refreshSession()` llama a `POST /api/auth/refresh` cuando el token expira.

### 4.2 cart-store.ts

```typescript
interface CartState {
  items: CartItem[];
  totalItems: number;
  subtotal: number;
  sesionId: string | null;
  fetchCart: () => Promise<void>;
  addItem: (productoId: number, varianteId: number | null, cantidad: number) => Promise<void>;
  updateQuantity: (itemId: number, cantidad: number) => Promise<void>;
  removeItem: (itemId: number) => Promise<void>;
  clearCart: () => Promise<void>;
}
```

- Para invitados se envía `X-Sesion-Id` header (GUID generado en frontend y persistido en `localStorage`).
- Si el usuario se loguea, el carrito se migra automáticamente.
- `totalItems` y `subtotal` se calculan localmente para UI inmediata, pero el backend es la fuente de verdad.

### 4.3 ui-store.ts

```typescript
interface UIState {
  sidebarOpen: boolean;
  searchQuery: string;
  toasts: Toast[];
  toggleSidebar: () => void;
  setSearchQuery: (q: string) => void;
  addToast: (toast: Toast) => void;
  removeToast: (id: string) => void;
}
```

- Maneja estado UI global (sidebar admin, búsqueda, toasts/notificaciones).

## 5. Esquemas Zod

### 5.1 auth.ts

```typescript
export const loginSchema = z.object({
  email: z.string().email("Email inválido"),
  password: z.string().min(6, "Mínimo 6 caracteres"),
});

export const registerSchema = z.object({
  nombre: z.string().min(2, "Nombre muy corto"),
  apellido: z.string().min(2, "Apellido muy corto"),
  email: z.string().email("Email inválido"),
  telefono: z.string().optional(),
  password: z.string().min(6, "Mínimo 6 caracteres"),
  confirmarPassword: z.string(),
}).refine((data) => data.password === data.confirmarPassword, {
  message: "Las contraseñas no coinciden",
  path: ["confirmarPassword"],
});
```

### 5.2 producto.ts

```typescript
export const createProductoSchema = z.object({
  nombre: z.string().min(3, "Nombre muy corto"),
  descripcion: z.string().min(10, "Descripción muy corta"),
  precio: z.number().positive("El precio debe ser positivo"),
  categoriaId: z.number().int().positive(),
  marcaId: z.number().int().positive(),
  esMarcaPropia: z.boolean().default(false),
  imagenes: z.array(z.instanceof(File)).max(5).optional(),
});
```

### 5.3 pedido.ts

```typescript
export const checkoutSchema = z.object({
  tipoEntrega: z.enum(["envio", "retiro"]),
  direccionId: z.number().int().optional(),
  // Si es retiro, no requiere dirección
  calle: z.string().optional(),
  numero: z.string().optional(),
  ciudad: z.string().optional(),
  codigoPostal: z.string().optional(),
  metodoPago: z.enum(["tarjeta", "transferencia", "mercado_pago", "efectivo"]),
  notas: z.string().max(500).optional(),
}).refine(
  (data) => data.tipoEntrega === "retiro" || (data.direccionId || (data.calle && data.numero)),
  { message: "Dirección de envío requerida", path: ["calle"] }
);
```

### 5.4 direccion.ts

```typescript
export const direccionSchema = z.object({
  calle: z.string().min(3, "Calle requerida"),
  numero: z.string().min(1, "Número requerido"),
  complemento: z.string().optional(),
  ciudad: z.string().min(3, "Ciudad requerida"),
  provincia: z.string().optional(),
  codigoPostal: z.string().min(3, "Código postal requerido"),
  referencia: z.string().optional(),
  esPredeterminada: z.boolean().default(false),
});
```

## 6. Componentes ShadcnUI por módulo

### Módulo público

| Componente ShadcnUI | Uso                                                 |
| ------------------- | --------------------------------------------------- |
| Card                | ProductCard, categorías en Home                     |
| Button              | "Agregar al carrito", "Comprar", "Ver más"          |
| Input               | Búsqueda, formularios                               |
| Select              | Filtro por categoría, ordenar por precio            |
| Badge               | Precio, descuento, stock bajo                       |
| Sheet               | Carrito lateral (slide-over)                        |
| Carousel            | Galería de imágenes del producto                    |
| Dialog              | Confirmación de eliminación, selector de variante   |
| Separator           | Separar secciones en detalle de producto            |
| Skeleton            | Loading state para cards y detalle                  |

### Módulo cliente

| Componente ShadcnUI | Uso                                                 |
| ------------------- | --------------------------------------------------- |
| Table               | Historial de pedidos, direcciones guardadas         |
| Dialog              | Confirmar cancelación de pedido                     |
| Form                | Editar perfil, agregar dirección                    |
| Toast               | Feedback de acciones (guardado, error)              |
| Tabs                | Perfil (Datos personales / Direcciones / Pedidos)   |

### Módulo admin

| Componente ShadcnUI | Uso                                                 |
| ------------------- | --------------------------------------------------- |
| Table + DataTable   | Listado de productos, pedidos, usuarios, marcas     |
| Dialog              | Confirmar eliminación, detalle rápido               |
| Form                | Crear/editar producto, categoría, marca             |
| Select              | Cambiar estado de pedido, filtrar listados          |
| Badge               | Estado del pedido (color por estado)                |
| Sheet               | Panel lateral de filtros                            |
| Command + Popover   | Search & select de categorías/marcas                |
| Checkbox            | Selección múltiple de productos                     |
| RadioGroup          | Selección de método de pago (admin)                 |
| Avatar              | Avatar del usuario en la sidebar                    |
| ScrollArea          | Listados largos en modales                          |
| Separator           | Separar secciones en formularios                    |

## 7. Flujo de API client

```typescript
// lib/api-client.ts
const API_BASE = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5029/api";

interface FetchOptions extends RequestInit {
  auth?: boolean;        // incluir JWT
  sesion?: boolean;      // incluir X-Sesion-Id para invitados
}

async function apiClient<T>(endpoint: string, options: FetchOptions = {}): Promise<T> {
  const headers: HeadersInit = { "Content-Type": "application/json" };

  if (options.auth) {
    const token = useAuthStore.getState().token;
    headers["Authorization"] = `Bearer ${token}`;
  }
  if (options.sesion) {
    const sesionId = localStorage.getItem("sesionId");
    if (sesionId) headers["X-Sesion-Id"] = sesionId;
  }

  const res = await fetch(`${API_BASE}${endpoint}`, { ...options, headers });
  if (!res.ok) throw new ApiError(res.status, await res.json());
  return res.json();
}
```

**Estrategia de fetching:**
- **SSR (Server Side Rendering):** Para páginas públicas que necesitan SEO (Home, Catálogo, Detalle producto).
- **ISR (Incremental Static Regeneration):** Para catálogo si los productos no cambian frecuentemente.
- **CSR (Client Side Rendering):** Para todo lo que requiere autenticación (carrito, checkout, admin).

## 8. Mapa de rutas de navegación

| Ruta                      | Página           | Layout      | Auth     |
| ------------------------- | ---------------- | ----------- | -------- |
| `/`                       | Home             | Public      | -        |
| `/productos`              | Catálogo         | Public      | -        |
| `/productos/[slug]`       | Detalle prod.    | Public      | -        |
| `/categorias/[slug]`      | Por categoría    | Public      | -        |
| `/carrito`                | Carrito          | Public      | -        |
| `/checkout`               | Checkout         | Public      | -        |
| `/pedido/[id]`            | Confirmación     | Public      | -        |
| `/login`                  | Login            | Public      | -        |
| `/registro`               | Registro         | Public      | -        |
| `/perfil`                 | Mi perfil        | Auth        | Cliente  |
| `/direcciones`            | Mis direcciones  | Auth        | Cliente  |
| `/pedidos`                | Mis pedidos      | Auth        | Cliente  |
| `/pedidos/[id]`           | Detalle pedido   | Auth        | Cliente  |
| `/admin`                  | Dashboard        | Auth        | Admin    |
| `/admin/productos`        | Gestión prod.    | Auth        | Admin    |
| `/admin/productos/nuevo`  | Crear producto   | Auth        | Admin    |
| `/admin/productos/[id]`   | Editar producto  | Auth        | Admin    |
| `/admin/categorias`       | Gestión cat.     | Auth        | Admin    |
| `/admin/marcas`           | Gestión marcas   | Auth        | Admin    |
| `/admin/pedidos`          | Gestión pedidos  | Auth        | Admin    |
| `/admin/pedidos/[id]`     | Detalle pedido   | Auth        | Admin    |
| `/admin/usuarios`         | Gestión usuarios | Auth        | Admin    |

## 9. Preguntas de diseño visual

Para definir la identidad visual de Huellario, se deben responder las siguientes preguntas:

### 9.1 Paleta de colores

| Pregunta                                          | Opciones                                                                 |
| ------------------------------------------------- | ------------------------------------------------------------------------ |
| ¿Qué color principal representa a Huellario?      | Naranja cálido · Verde naturaleza · Azul confianza · Celeste · Marrón tierra |
| ¿Qué sensación debe transmitir la marca?          | Cálida y amigable · Moderna y minimalista · Natural y orgánica · Divertida y juvenil |
| ¿Prefieres colores vibrantes o tonos pastel?      | Vibrantes · Pastel · Mixto (vibrante con fondos pastel)                  |
| ¿Color de fondo general?                          | Blanco · Gris muy claro · Beige claro · Blanco con detalles de color     |
| ¿Color para botones principales (CTA)?            | Mismo que el color principal · Contraste fuerte · Degradado              |
| ¿Colores para estados de pedido?                  | Pendiente (amarillo) · Confirmado (azul) · En envío (celeste) · Entregado (verde) · Cancelado (rojo) |

### 9.2 Tipografía

| Pregunta                                          | Opciones                                                                 |
| ------------------------------------------------- | ------------------------------------------------------------------------ |
| ¿Tipografía para títulos (headers)?               | Poppins · Montserrat · Inter · Playfair Display · Plus Jakarta Sans      |
| ¿Tipografía para texto corporal?                  | Inter · Nunito · Open Sans · Lato · Poppins (light)                      |
| ¿Tipografía para logo?                            | Misma que títulos · Una script/display · Una sans-serif bold             |
| ¿Estilo general de tipografía?                    | Sans-serif moderna · Serif clásica · Redondeada y amigable · Geométrica  |

### 9.3 Logo e iconografía

| Pregunta                                          | Opciones                                                                 |
| ------------------------------------------------- | ------------------------------------------------------------------------ |
| ¿El logo incluye ícono de perro?                  | Sí, silueta de perro · Sí, huella · Sí, cara de perro · No, solo texto  |
| ¿Estilo del ícono?                                | Line art (trazos finos) · Relleno (solid) · Minimalista · Detallado      |
| ¿Forma del logo?                                  | Circular · Rectangular · Horizontal (texto + ícono al lado) · Vertical   |

### 9.4 Estilo visual general

| Pregunta                                          | Opciones                                                                 |
| ------------------------------------------------- | ------------------------------------------------------------------------ |
| ¿Esquinas de cards y botones?                     | Redondeadas (rounded-lg) · Muy redondeadas (rounded-xl) · Cuadradas      |
| ¿Sombras en elementos?                            | Sombras suaves (shadow-sm) · Sombras marcadas · Sin sombras              |
| ¿Fondo del panel admin?                           | Blanco · Gris oscuro (dark mode) · Personalizable                        |
| ¿Ilustraciones en páginas vacías (carrito vacío, 404)? | Sí, ilustraciones de perros · No, solo texto mensaje · Fotos reales |
| ¿Microinteracciones? (hover, transiciones)        | Transiciones suaves · Sin animaciones · Animaciones sutiles              |
| ¿Modo oscuro?                                     | Sí, desde el inicio · No · Más adelante                                  |

### 9.5 Experiencia móvil

| Pregunta                                          | Opciones                                                                 |
| ------------------------------------------------- | ------------------------------------------------------------------------ |
| ¿Menú móvil?                                      | Hamburguesa desplegable · Bottom navigation · Drawer lateral             |
| ¿Carrito en móvil?                                | Sheet inferior · Página separada · Flotante (FAB)                        |
| ¿Checkout en móvil?                               | Mismo layout que desktop (responsive) · Stepper (paso a paso)            |

## 10. Estados de UI por entidad

### Estados del producto (frontend badges)

```
Activo       → Badge verde "Activo"     (en listados admin)
Inactivo     → Badge gris "Inactivo"
Stock > 10   → Sin badge particular
Stock 1-10   → Badge amarillo "Quedan pocos"
Stock = 0    → Badge rojo "Sin stock" + botón compra deshabilitado
```

### Estados del pedido (frontend badges)

```
Pendiente    → Badge amarillo "Pendiente"
Confirmado   → Badge azul "Confirmado"
En envío     → Badge celeste "En envío"
Entregado    → Badge verde "Entregado"
Cancelado    → Badge rojo "Cancelado"
Reembolsado  → Badge gris "Reembolsado"
```

### Estilos de botones por acción

| Acción              | Variante ShadcnUI | Color           |
| ------------------- | ----------------- | --------------- |
| Agregar al carrito  | default           | Primary         |
| Comprar / Checkout  | default           | Primary (grande)|
| Editar (admin)      | outline           | Primary         |
| Eliminar            | destructive       | Rojo            |
| Cancelar pedido     | destructive       | Rojo            |
| Guardar formulario  | default           | Primary         |
| Volver / Cancelar   | ghost             | Gris            |
| Cerrar sesión       | outline           | Rojo            |
| Cambiar estado      | secondary         | Según estado    |
| Pagar               | default           | Primary (grande)|

## 11. Decisiones técnicas

- **Manejo de sesión de invitados:** Se genera un UUID en el frontend al cargar por primera vez, se persiste en `localStorage` como `sesionId`, y se envía como header `X-Sesion-Id` en todas las requests del carrito.
- **Migración de carrito:** Al registrarse o iniciar sesión, se envía el `sesionId` actual al backend para migrar los items al usuario autenticado.
- **Refresco de token:** Interceptor en `api-client.ts` que detecta `401`, llama a `POST /api/auth/refresh`, y reintenta la request original.
- **Paginación:** Componente `DataTable` reutilizable con controles de página, página actual resaltada, y selector de items por página (10/20/50).
- **Manejo de imágenes:** Subida directa al backend como `multipart/form-data` en `POST /api/productos/{id}/imagenes`. Preview local antes de subir.
- **SEO:** Las páginas públicas (Home, Catálogo, Detalle producto) usan SSR/ISR para que los motores de búsqueda indexen correctamente. `metadata` API de Next.js para title, description, Open Graph.
- **Carga diferida (lazy loading):** Las imágenes de productos usan `next/image` con `loading="lazy"`. El panel admin carga bajo demanda (solo cuando el usuario navega a admin).
