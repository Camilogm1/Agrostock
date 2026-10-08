# AgroStock

Sistema de gestión para pequeños productores agrícolas: cultivos, cosechas, inventario, clientes y ventas.

**Materia:** Ingeniería de Software — UPB, Medellín, 2026
**Estudiantes:** Juan Felipe Cardona · Juan Camilo González · Daniel Villa
**Profesora:** Yuri Marcela Escobar

---

## Stack

- **Backend:** C# / ASP.NET Core Web API (.NET 8) + Entity Framework Core
- **Frontend:** React (Vite) + React Router
- **Base de datos:** MariaDB (vía Docker Compose para desarrollo local)
- **Autenticación:** JWT con roles (Administrador / Vendedor)
- **Pruebas:** xUnit (backend)
- **CI:** GitHub Actions

---

## Cómo levantar el proyecto

### 1. Base de datos (Docker)
```bash
docker compose up -d
```
Esto levanta MariaDB en `localhost:3306`, con la base `agrostock` ya creada. Si prefieres instalar MariaDB directo en tu máquina en vez de Docker, ajusta la cadena de conexión en `backend/AgroStock.Api/appsettings.json`.

### 2. Backend
```bash
cd backend/AgroStock.Api
dotnet restore
dotnet ef database update   # crea las tablas (requiere dotnet-ef: dotnet tool install --global dotnet-ef)
dotnet run
```
La API queda disponible en la URL que indique la consola (normalmente `http://localhost:5000`), con Swagger en `/swagger`.

### 3. Frontend
```bash
cd frontend
npm install
npm run dev
```
Abre `http://localhost:5173`.

### 4. Pruebas automatizadas (no requieren base de datos)
```bash
cd backend/AgroStock.Api.Tests
dotnet restore
dotnet test
```

---

## Estado del proyecto: ~75% completado

Esta sección detalla **exactamente** qué está incluido y qué falta, mapeado contra la planificación de la Entrega 1 (RF, RNF, HU y Sprints).

### ✅ Incluido en esta entrega (el 75%)

**Sprint 1 — Base técnica**
- Modelo de dominio completo: `Usuario`, `Rol`, `Cultivo`, `Cosecha`, `Inventario`, `Cliente`, `Venta`, `DetalleVenta`.
- `AgroStockDbContext` con todas las relaciones del diagrama de clases: agregación Cultivo→Cosecha sin cascada (RF-04), composición Venta→DetalleVenta con cascada, relación 1-1 Cultivo↔Inventario, identificación única de Cliente (RNF-09).
- Backend configurado: JWT, CORS, inyección de dependencias, Swagger.
- Frontend configurado: Vite, enrutamiento completo (`App.jsx`/`main.jsx`), contexto de autenticación, cliente Axios con manejo de errores.

**Sprint 2 — Producción e Inventario (backend + frontend)**
- RF-01 a RF-04: CRUD completo de cultivos (crear, listar, **editar**, eliminar con validación de cosechas asociadas).
- RF-05 a RF-07, RF-10: registro de cosechas con actualización automática y transaccional del inventario; rechazo de cosechas para cultivos inexistentes.
- RF-08, RF-09: consulta de inventario con filtros.
- RF-11: historial de cosechas por cultivo.
- RNF-06: validación de campos obligatorios en el servicio (`ArgumentException` → `400 Bad Request`).
- **Frontend:** páginas `Cultivos.jsx` (con edición) y `Cosechas.jsx` completas y conectadas al backend.

**Sprint 3 — Módulo Comercial (backend + frontend)**
- RF-12, RF-13: CRUD de clientes con validación de identificación única.
- RF-14 a RF-19: registro de ventas con transacción completa (validación de stock, descuento automático, rollback ante fallo) — implementado literal según el diagrama de secuencia de la Entrega 1.
- RF-20 a RF-22: historial de ventas con filtro por cliente/fecha y detalle.
- **Frontend:** página `Ventas.jsx` completa (selección de cliente/producto con stock visible, registro, historial).

**Calidad y DevOps**
- 4 pruebas automatizadas con xUnit sobre `CultivoService` (incluye la regla RF-04 de no eliminar cultivos con cosechas).
- Pipeline de CI (`ci.yml`) que compila y corre las pruebas en cada push/PR.
- `docker-compose.yml` para que los 3 integrantes trabajen sobre la misma base de datos sin depender de instalaciones distintas de MariaDB.
- Trazabilidad documentada entre el diagrama de clases y el código (ver nota de arquitectura más abajo).

### ❌ Qué falta (el 25% restante, explícito)

**Pendiente de Sprint 2/3**
- Selector de producto por **cultivo específico** en Ventas (hoy se selecciona directo por registro de inventario; falta UX más amigable si un cultivo tiene múltiples cosechas).
- Búsqueda de ventas por rango de fechas (hoy solo filtra por fecha exacta).

**Sprint 4 completo — esto es lo que más falta**
- **HU-21 (pruebas end-to-end):** no se ha verificado el flujo completo contra una base de datos MariaDB real corriendo — las pruebas actuales usan una base en memoria. Falta ejecutar el proyecto de punta a punta al menos una vez con Docker levantado.
- **HU-22 (cobertura de pruebas):** solo `CultivoService` tiene pruebas automatizadas. Faltan pruebas para `CosechaService`, `ClienteService` y, sobre todo, `VentaService` (la lógica más crítica del sistema, con la transacción de stock).
- **HU-23 (usabilidad):** la interfaz actual es funcional pero sin pulido visual — tablas HTML básicas, sin feedback de carga (spinners), sin diseño responsivo.
- **HU-24 (documentación):** falta el manual de usuario por rol y el diccionario de datos formal (más allá de lo que ya documenta el propio código).
- **HU-25 (demo):** no hay un guion de demo ni dataset de prueba preparado para la sustentación final.

**Infraestructura**
- No se han generado las migraciones de Entity Framework (`dotnet ef migrations add Inicial`) — hay que correrlas la primera vez que alguien levante el proyecto, siguiendo las instrucciones de arriba.
- No se ha probado el registro de usuarios/login contra una base de datos real (no hay un endpoint de registro de usuario ni un seed con un administrador de prueba — hay que insertarlo manualmente o agregar un script de seed).

---

## Nota de arquitectura: diagrama de clases vs. código

El diagrama de clases (Entrega 1, Modelo 2) ubica algunos métodos (`registrar()`, `buscar()`, `autenticar()`) directamente en las clases de entidad. En el código, esos métodos viven en la capa de **Servicios** (`CultivoService`, `VentaService`, `AuthService`, etc.), siguiendo el patrón estándar de ASP.NET Core de separar entidades de datos puras (POCO) de la lógica de negocio inyectada por dependencia. Esto no cambia el comportamiento funcional descrito en las historias de usuario ni en el diagrama de secuencia (Modelo 3).

---

## Nota de uso de IA

Se usó Claude (Anthropic) como asistente de desarrollo durante todo el proyecto: generó el scaffolding completo del backend y frontend a partir de los diagramas UML y el backlog de la Entrega 1 (modelos, DbContext, servicios, controladores, páginas de React, pipeline de CI), y ayudó a depurar errores reales encontrados al ejecutar el código (por ejemplo, un error de convención de claves primarias de Entity Framework Core).

<!-- TODO (equipo, antes de entregar): completar con el trabajo manual real:
quién ejecutó el proyecto por primera vez contra MariaDB, quién verificó cada
módulo manualmente, quién escribió pruebas adicionales si las hay, y cualquier
ajuste de diseño que hicieron ustedes sobre lo generado. -->

## Integrantes
- Juan Felipe Cardona
- Juan Camilo González
- Daniel Villa
