# AgroStock — Taller de CI (Semana 12)

Versión mínima y autocontenida para el taller evaluable de integración continua.
**No es el proyecto completo de AgroStock** (ese tiene autenticación, MariaDB, y los
módulos de Cosechas/Inventario/Clientes/Ventas). Aquí solo vive la historia RF-01,
lo mínimo necesario para cumplir los 5 entregables del taller.

## Stack
- C# / ASP.NET Core Web API (.NET 8)
- Entity Framework Core (InMemory — no requiere instalar ninguna base de datos)
- xUnit

## Cómo levantar el proyecto
```bash
cd src/AgroStockTaller.Api
dotnet restore
dotnet run
```
Abre `http://localhost:5000/swagger` (o el puerto que indique la consola) para probar
`POST /api/cultivos` y `GET /api/cultivos` manualmente.

## Cómo correr las pruebas
```bash
cd tests/AgroStockTaller.Tests
dotnet restore
dotnet test
```
Debe mostrar: `Passed! - Failed: 0, Passed: 3, Skipped: 0, Total: 3`.

## Estado del pipeline
![CI](https://github.com/Camilogm1/Agrostock/actions/workflows/ci.yml/badge.svg)

### Estado del Proyecto
![Last Commit](https://img.shields.io/github/last-commit/Camilogm1/Agrostock/main?style=flat-square&logo=git)
![Commits](https://img.shields.io/github/commit-activity/m/Camilogm1/Agrostock?style=flat-square&logo=git)
![Status](https://img.shields.io/badge/status-active-success)
![Language](https://img.shields.io/badge/language-C%23-purple)
![Framework](https://img.shields.io/badge/framework-ASP.NET%20Core-blue)

## Historia implementada en este taller
**RF-01 — Registro de cultivos**

> Dado que soy un administrador autenticado y completo el formulario de cultivo
> con nombre, tipo, lote y fecha de siembra válidos,
> Cuando envío el formulario de registro,
> Entonces el sistema crea el cultivo, crea su inventario inicial en 0,
> y lo muestra en el listado de cultivos.

Endpoint: `POST /api/cultivos` · `GET /api/cultivos`.

## Pruebas automatizadas
1. `RegistrarAsync_ConDatosValidos_CreaCultivoConInventarioInicialEnCero` — feliz, deriva del criterio de aceptación de RF-01.
2. `RegistrarAsync_SinNombre_DeberiaFallarLaValidacion` — RNF-06, valida campos obligatorios.
3. `ListarAsync_DespuesDeRegistrarCultivos_DevuelveTodosLosRegistrados` — RF-02, consulta del listado.

## Nota de uso de IA
Se usó Claude (Anthropic) como asistente de desarrollo durante el taller: generó el
scaffolding completo (modelos, DbContext, servicio, controlador, Program.cs, las 3
pruebas y el archivo `ci.yml`), reducido deliberadamente a lo mínimo necesario para
este taller (sin autenticación ni MariaDB, que sí forman parte del proyecto completo).

## Integrantes
- Juan Felipe Cardona
- Juan Camilo González
- Daniel Villa
