# AgroStock

![CI](https://github.com/Camilogm1/Agrostock/actions/workflows/ci.yml/badge.svg)

Sistema web de gestión para pequeños productores agrícolas: registra **cultivos**, sus **cosechas**, el **inventario** que generan, los **clientes** y las **ventas**, descontando el stock automáticamente.

Proyecto de Ingeniería de Software, UPB Medellín, 2026.
**Equipo:** Juan Felipe Cardona · Juan Camilo González · Daniel Villa
**Profesora:** Yuri Marcela Escobar

---

## Contenido

- [Funcionalidades](#funcionalidades)
- [Stack](#stack)
- [Estructura del repositorio](#estructura-del-repositorio)
- [Cómo ejecutar el proyecto](#cómo-ejecutar-el-proyecto)
- [Usuarios de prueba y roles](#usuarios-de-prueba-y-roles)
- [API](#api)
- [Reglas de negocio](#reglas-de-negocio)
- [Pruebas y CI](#pruebas-y-ci)
- [Decisiones de diseño](#decisiones-de-diseño)
- [Estado del proyecto](#estado-del-proyecto)

---

## Funcionalidades

| Módulo | Qué permite | Requisitos |
|---|---|---|
| Autenticación | Inicio de sesión con usuario y contraseña; token JWT con rol | RNF-01, RNF-02 |
| Cultivos | Registrar, listar, editar y eliminar cultivos (no se pueden eliminar si tienen cosechas) | RF-01 a RF-04 |
| Cosechas | Registrar cosechas de un cultivo; el inventario sube automáticamente; historial por cultivo | RF-05 a RF-07, RF-10, RF-11 |
| Inventario | Consultar el stock disponible por producto, con filtro por nombre o tipo de cultivo | RF-08, RF-09 |
| Clientes | Registrar y buscar clientes; la identificación no se puede repetir | RF-12, RF-13, RNF-09 |
| Ventas | Registrar ventas validando stock y descontándolo; historial filtrable por cliente y fecha | RF-14 a RF-22 |

## Stack

- **Backend:** C# / ASP.NET Core Web API (.NET 8), Entity Framework Core 8 con Pomelo (MariaDB/MySQL)
- **Frontend:** React 18 + Vite + React Router + Axios
- **Base de datos:** MariaDB 11 (con Docker Compose)
- **Seguridad:** JWT con roles `Administrador` y `Vendedor`; contraseñas con BCrypt
- **Pruebas:** xUnit + EF Core InMemory
- **CI:** GitHub Actions (compila y prueba el backend, compila el frontend)

## Estructura del repositorio

```
├── backend/
│   ├── AgroStock.sln
│   ├── AgroStock.Api/
│   │   ├── Controllers/   Endpoints HTTP (delgados: delegan en los servicios)
│   │   ├── Services/      Lógica de negocio (validaciones, stock, transacciones)
│   │   ├── Models/        Entidades del dominio (diagrama de clases)
│   │   ├── DTOs/          Objetos de entrada y salida de la API
│   │   ├── Data/          DbContext, migraciones y datos iniciales
│   │   └── Exceptions/    Excepciones de negocio y su traducción a códigos HTTP
│   └── AgroStock.Api.Tests/   Pruebas unitarias de los servicios
├── frontend/
│   └── src/
│       ├── api/           Cliente Axios (token, manejo de errores, sesión vencida)
│       ├── context/       Contexto de autenticación
│       ├── components/    Navbar y rutas protegidas por rol
│       ├── pages/         Una página por módulo
│       └── utils/         Utilidades de fechas
├── docs/diseno-inicial/   Scripts SQL de la Entrega 1 (solo referencia, ver abajo)
├── docker-compose.yml     MariaDB para desarrollo local
└── .github/workflows/     Pipeline de CI
```

## Cómo ejecutar el proyecto

**Requisitos:** [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0), [Node.js 20+](https://nodejs.org/) y Docker (o MariaDB/MySQL instalado localmente).

### 1. Base de datos

```bash
docker compose up -d
```

Levanta MariaDB en `localhost:3306` con la base `agrostock` (usuario `root`, contraseña `agrostock123`). Si usas una instalación propia de MariaDB/MySQL, cambia `ConnectionStrings:DefaultConnection` en `backend/AgroStock.Api/appsettings.json`.

### 2. Backend

```bash
cd backend/AgroStock.Api
dotnet run
```

- La API queda en `http://localhost:5000` y Swagger en `http://localhost:5000/swagger`.
- En modo desarrollo, al arrancar se **aplican las migraciones automáticamente** (crea las tablas) y, si la base no tiene usuarios, se crean los [usuarios de prueba](#usuarios-de-prueba-y-roles). No hace falta correr `dotnet ef database update`.
- En Swagger, usa `POST /api/auth/login`, copia el `token` y pégalo en el botón **Authorize** para probar los demás endpoints.

### 3. Frontend

```bash
cd frontend
npm install
npm run dev
```

Abre `http://localhost:5173`. Si el backend corre en otra URL, crea `frontend/.env` a partir de `frontend/.env.example` y ajusta `VITE_API_URL`.

### Cambios en el modelo de datos

Si modificas una entidad o el `DbContext`, genera una migración nueva (requiere `dotnet tool install --global dotnet-ef`):

```bash
cd backend/AgroStock.Api
dotnet ef migrations add NombreDelCambio -o Data/Migrations
```

Se aplicará sola la próxima vez que arranque la API.

## Usuarios de prueba y roles

Se crean solo en desarrollo, desde `appsettings.Development.json`:

| Usuario | Contraseña | Rol |
|---|---|---|
| `admin` | `Admin123*` | Administrador |
| `vendedor` | `Vendedor123*` | Vendedor |

| Acción | Administrador | Vendedor |
|---|:---:|:---:|
| Ver cultivos, inventario, clientes y ventas | ✅ | ✅ |
| Registrar, editar o eliminar cultivos | ✅ | ❌ |
| Registrar cosechas | ✅ | ❌ |
| Registrar clientes y ventas | ✅ | ✅ |

## API

Todas las rutas, excepto el login, requieren el encabezado `Authorization: Bearer <token>`.

| Método | Ruta | Descripción | Rol |
|---|---|---|---|
| POST | `/api/auth/login` | Inicia sesión y devuelve el token | Público |
| GET | `/api/cultivos` | Lista los cultivos | Todos |
| POST | `/api/cultivos` | Registra un cultivo (y su inventario en 0) | Administrador |
| PUT | `/api/cultivos/{id}` | Modifica un cultivo | Administrador |
| DELETE | `/api/cultivos/{id}` | Elimina un cultivo sin cosechas | Administrador |
| GET | `/api/cultivos/{id}/cosechas` | Historial de cosechas del cultivo | Todos |
| POST | `/api/cosechas` | Registra una cosecha y suma al inventario | Administrador |
| GET | `/api/inventario?filtro=` | Stock disponible, filtrado por nombre o tipo | Todos |
| GET | `/api/clientes?texto=` | Lista clientes, filtrados por nombre o identificación | Todos |
| POST | `/api/clientes` | Registra un cliente | Todos |
| GET | `/api/ventas?cliente=&fecha=` | Historial de ventas, filtrado por cliente y/o fecha | Todos |
| POST | `/api/ventas` | Registra una venta y descuenta el inventario | Todos |

Los errores de negocio responden con `{ "mensaje": "..." }` y estos códigos:

| Código | Cuándo |
|---|---|
| 400 | Datos obligatorios vacíos o inválidos (RNF-06) |
| 401 / 403 | Sin sesión / rol sin permiso |
| 404 | El cultivo, cliente o inventario no existe (RF-10) |
| 409 | Cultivo con cosechas (RF-04), identificación repetida (RNF-09) o venta simultánea sobre el mismo stock |
| 422 | Stock insuficiente para la venta (RF-18) |

## Reglas de negocio

- **Inventario inicial:** al registrar un cultivo se crea su inventario con cantidad 0 (relación 1 a 1).
- **Cosechas:** la cantidad debe ser mayor a 0 y el cultivo debe existir. La cosecha y el aumento de stock se guardan en la misma transacción.
- **Eliminación de cultivos:** no se permite si tiene cosechas. Si no tiene, se elimina junto con su inventario (que está en 0).
- **Ventas:** se valida que haya stock suficiente; la venta, su detalle y el descuento de stock se guardan en una sola transacción. El stock tiene control de concurrencia optimista, así que dos ventas simultáneas no pueden vender el mismo producto dos veces: una de ellas recibe un 409 y debe reintentarse.
- **Clientes:** el número de identificación es único (validado en el servicio y con un índice único en la base).

## Pruebas y CI

```bash
cd backend
dotnet test
```

Hay 20 pruebas unitarias con base de datos en memoria (no necesitan MariaDB):

| Clase | Cubre |
|---|---|
| `CultivoServiceTests` | Registro con inventario en 0, validaciones, listado, edición, eliminación con y sin cosechas |
| `CosechaServiceTests` | Aumento de inventario, cantidad no positiva, cultivo inexistente |
| `ClienteServiceTests` | Identificación repetida, campos obligatorios, búsqueda |
| `VentaServiceTests` | Descuento de stock, stock insuficiente sin efectos, cliente inexistente, filtros del historial |

El pipeline `.github/workflows/ci.yml` corre en cada push y pull request: compila la solución, ejecuta las pruebas y compila el frontend.

## Decisiones de diseño

- **Servicios en vez de métodos en las entidades.** El diagrama de clases pone métodos como `registrar()` o `autenticar()` en las entidades; en el código viven en `Services/`, que es el patrón habitual de ASP.NET Core (entidades simples + lógica inyectada). El comportamiento es el mismo que describen las historias de usuario y el diagrama de secuencia.
- **La lógica de negocio vive en el backend, no en triggers.** Los scripts de `docs/diseno-inicial/` (Entrega 1) proponían triggers en MariaDB para el inventario y las ventas. El proyecto final implementa esas reglas en los servicios de C#, para poder probarlas con xUnit y devolver mensajes claros. La estructura real de la base la generan las migraciones de Entity Framework (`backend/AgroStock.Api/Data/Migrations`). **No ejecutes esos scripts sobre la base del proyecto:** los triggers duplicarían los movimientos de inventario.
- **Manejo de errores centralizado.** Los servicios lanzan excepciones de negocio y `ManejadorExcepciones` las traduce al código HTTP correspondiente, así que los controladores no repiten `try/catch`.

## Estado del proyecto

**Avance estimado: ~75%.** Sprints 1 a 3 implementados; Sprint 4 (calidad y cierre) en curso.

### Hecho

- **Sprint 1 · Base técnica:** modelo de dominio, `DbContext` con todas las relaciones, migraciones, JWT con roles, CORS, Swagger, frontend con rutas protegidas.
- **Sprint 2 · Producción e inventario:** cultivos (CRUD completo), cosechas con actualización automática de inventario, historial de cosechas, consulta de inventario con filtro.
- **Sprint 3 · Comercial:** clientes con identificación única, ventas con validación y descuento de stock, historial con filtros por cliente y fecha.
- **Calidad:** 20 pruebas unitarias sobre los 4 servicios principales, CI para backend y frontend, flujo completo verificado contra MariaDB real.

### Pendiente

- **Ventas:** filtro por rango de fechas (hoy filtra por un día exacto) y selector de producto agrupado por cultivo.
- **Usuarios:** no hay pantalla ni endpoint para crear usuarios; hoy solo existen los de prueba.
- **HU-22 · Cobertura:** pruebas de integración de los controladores (autorización por rol, códigos HTTP).
- **HU-23 · Usabilidad:** diseño visual, indicadores de carga y diseño responsivo (hoy la interfaz es funcional pero básica).
- **HU-24 · Documentación:** manual de usuario por rol y diccionario de datos.
- **HU-25 · Demo:** guion y datos de prueba para la sustentación.

## Uso de IA

Se usó Claude (Anthropic) como asistente de desarrollo: generó el scaffolding inicial del backend y el frontend a partir de los diagramas UML y el backlog de la Entrega 1, y apoyó la revisión del código, la corrección de errores y las pruebas automatizadas.
