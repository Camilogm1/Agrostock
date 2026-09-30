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
<!-- TODO: reemplazar OWNER/REPO por el repo real del equipo en GitHub -->
![CI](https://github.com/Camilogm1/Agrostock/actions/workflows/ci.yml/badge.svg)


## Integrantes
- Juan Felipe Cardona
- Juan Camilo González
- Daniel Villa
