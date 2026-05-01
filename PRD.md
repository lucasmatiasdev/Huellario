# PRD - Huellario

## 1. Resumen ejecutivo

Huellario es un ecommerce B2C especializado en accesorios para perros, orientado a dueños de mascotas particulares en el ámbito local (una ciudad). Ofrece productos de marca propia y de terceros, con modalidades de entrega que incluyen envío a domicilio y retiro en punto de retiro. La plataforma es 100 % digital, sin tienda física asociada.

## 2. Problema y oportunidad

Los dueños de perros en la ciudad carecen de una tienda digital especializada exclusivamente en accesorios de calidad para sus mascotas. Las opciones actuales son genéricas (supermercados, tiendas departamentales) o requieren desplazamiento a tiendas físicas con horarios limitados. Huellario resuelve esto ofreciendo un catálogo curado de accesorios con entrega flexible.

## 3. Objetivos del proyecto

- Proveer una plataforma de comercio electrónico funcional, rápida y confiable.
- Ofrecer una experiencia de compra intuitiva tanto para usuarios registrados como invitados.
- Facilitar la gestión de productos, stock, pedidos y clientes desde un panel administrativo.
- Sentar las bases técnicas para escalar a futuro (nuevas categorías, envío nacional, etc.).

## 4. Público objetivo

- **Perfil:** Dueños de mascotas (perros) particulares, residentes en la ciudad.
- **Edad:** 20–50 años.
- **Comportamiento:** Buscan productos de calidad para sus mascotas, valoran la comodidad del envío a domicilio y la posibilidad de retirar en persona.

## 5. Tipos de producto

Huellario vende exclusivamente **accesorios para perros**, que incluyen:

| Categoría              | Ejemplos                                        |
| ---------------------- | ----------------------------------------------- |
| Collares y correas     | Collares de cuero, nylon, arneses, correas retráctiles |
| Camas y descanso       | Camas almohadón, elevadas, mantas               |
| Juguetes               | Pelotas, mordedores, cuerdas, juguetes interactivos |
| Ropa y protección      | Abrigos, impermeables, botines, pañuelos        |
| Comederos y bebederos  | Bowls de acero, automáticos, elevados           |
| Transporte             | Bolsos transportadores, jaulas, asientos para auto |

Los productos pueden tener **variantes** (talla, color, tamaño) según la categoría. Por ejemplo, una cama puede venir en S/M/L; un collar puede venir en varios colores.

## 6. Modalidad de ecommerce

- **Modelo:** B2C (Business to Consumer).
- **Venta directa:** Huellario compra/ fabrica y vende directamente al consumidor final.
- **Mix de marcas:** 
  - **Marca propia:** Productos con la marca Huellario.
  - **Terceros:** Productos de marcas reconocidas del rubro.

## 7. Alcance geográfico

- **Cobertura inicial:** Local (una ciudad específica).
- **Futuro:** Potencial expansión a nivel nacional.

## 8. Logística y entregas

| Modalidad              | Descripción                                          |
| ---------------------- | ---------------------------------------------------- |
| Envío a domicilio      | Courier o servicio de mensajería local.              |
| Retiro en punto        | El cliente pasa a buscar por un punto de retiro (bodega o centro de distribución). |

## 9. Medios de pago

| Método                        | Descripción                             |
| ----------------------------- | --------------------------------------- |
| Tarjetas de crédito/débito    | Visa, Mastercard, Amex.                 |
| Transferencia bancaria        | Pago por transferencia directa.         |
| Mercado Pago / billeteras     | Pagos digitales y wallets.              |
| Efectivo / depósito           | Pago en efectivo o depósito en cuenta.  |

## 10. Manejo de inventario

Huellario cuenta con **control de stock** completo. Cada producto/variante tiene una cantidad disponible que se descuenta al realizar un pedido. El panel de administración permite gestionar entradas y salidas de inventario.

## 11. Funcionalidades del sistema

### 11.1 Catálogo de productos
- Navegación por categorías y subcategorías.
- Filtros por precio, marca, categoría, variante (talla/color).
- Búsqueda por nombre o palabra clave.
- Vista de detalle con imágenes, descripción, precio, variantes disponibles y stock.

### 11.2 Carrito de compras
- Agregar/quitar productos y modificar cantidades.
- Persistencia del carrito durante la sesión.
- Visualización de subtotal, descuentos y total.

### 11.3 Registro e inicio de sesión
- Creación de cuenta con email y contraseña.
- Perfil de usuario con historial de pedidos y datos de envío.
- Recuperación de contraseña.

### 11.4 Compra como invitado
- Posibilidad de completar una compra sin registrarse.
- Captura de datos de contacto y envío necesarios para la entrega.

### 11.5 Proceso de pago (checkout)
- Selección de dirección de envío o modalidad de retiro.
- Selección de método de pago.
- Resumen del pedido antes de confirmar.
- Confirmación con número de pedido y resumen por email.

### 11.6 Panel de administración
- Gestión de productos (alta, edición, baja, variantes, stock, precios).
- Gestión de pedidos (listado, cambio de estado, detalle).
- Gestión de usuarios/clientes.
- Gestión de categorías y marcas.
- Visualización de stock actual.

### 11.7 Notificaciones por email (futuro)
- Confirmación de pedido.
- Actualización de estado de envío.
- Recordatorios de carrito abandonado (a considerar).

## 12. Casos de uso principales

| ID    | Caso de uso              | Descripción breve                                        |
| ----- | ------------------------ | -------------------------------------------------------- |
| CU-01 | Navegar catálogo          | El cliente explora productos por categoría o búsqueda.  |
| CU-02 | Ver detalle de producto   | El cliente consulta información, imágenes y variantes.  |
| CU-03 | Agregar al carrito        | El cliente añade un producto con variante y cantidad.    |
| CU-04 | Gestionar carrito         | El cliente modifica cantidades o elimina productos.      |
| CU-05 | Registrarse               | El cliente crea una cuenta en la plataforma.             |
| CU-06 | Iniciar sesión            | El cliente accede a su cuenta.                           |
| CU-07 | Comprar como invitado     | El cliente realiza un pedido sin registrarse.            |
| CU-08 | Realizar checkout         | El cliente completa la compra con datos de envío y pago. |
| CU-09 | Ver historial de pedidos  | El cliente consulta sus pedidos anteriores.              |
| CU-10 | Gestionar productos (admin) | El administrador da de alta, edita o elimina productos. |
| CU-11 | Gestionar stock (admin)   | El administrador actualiza cantidades de inventario.     |
| CU-12 | Gestionar pedidos (admin) | El administrador visualiza y cambia estados de pedidos.  |
| CU-13 | Gestionar categorías (admin) | El administrador crea y organiza categorías.           |
| CU-14 | Gestionar marcas (admin)  | El administrador administra las marcas disponibles.      |

## 13. Herramientas del proyecto

| Herramienta       | Propósito                                        |
| ----------------- | ------------------------------------------------ |
| Next.js           | Framework frontend (React)                       |
| .NET 8            | Backend con Clean Architecture                   |
| PostgreSQL / SQL Server | Base de datos relacional                  |
| Entity Framework  | ORM para .NET                                    |
| JWT               | Autenticación y autorización                     |
| Docker            | Contenedores para entorno de desarrollo          |
| Git + GitHub      | Control de versiones y repositorio               |
| Swagger / OpenAPI | Documentación de API                             |
| Mercado Pago API  | Procesamiento de pagos                           |

## 14. Roles de usuario

| Rol            | Descripción                                          |
| -------------- | ---------------------------------------------------- |
| Cliente        | Usuario final que navega, compra y consulta pedidos. |
| Invitado       | Usuario no registrado que puede comprar.             |
| Administrador  | Gestiona productos, pedidos, stock y usuarios.       |

## 15. Estados del pedido

| Estado       | Significado                                          |
| ------------ | ---------------------------------------------------- |
| Pendiente    | Pedido creado, esperando pago.                       |
| Confirmado   | Pago recibido, pedido en preparación.                |
| En envío     | Pedido despachado al courier o listo para retiro.    |
| Entregado    | Pedido recibido por el cliente.                      |
| Cancelado    | Pedido cancelado (por cliente o admin).              |
| Reembolsado  | Pago devuelto al cliente (si aplica).                |
