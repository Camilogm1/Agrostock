# ADR-0002: Lógica de negocio en el servicio de cultivos

- Estado: Aceptada para el prototipo actual
- Fecha: 2026-10-12

## Contexto

La API expone `GET /api/cultivos` y `POST /api/cultivos`. El controlador delega en `ICultivoService`; `CultivoService` valida los datos y usa `AppDbContext` para persistirlos. No hay otras capas de acceso a datos en el prototipo.

## Decisión

Mantener los endpoints en `CultivosController` y las reglas del caso de uso en `CultivoService`, detrás de `ICultivoService`. El contexto de EF Core se inyecta al servicio.

## Alternativas descartadas

- Poner validaciones y persistencia directamente en el controlador: reduce archivos al inicio, pero acopla HTTP, reglas de negocio y acceso a datos, y dificulta probar el caso de uso sin pasar por la API.
- Añadir un repositorio genérico entre el servicio y EF Core: agrega una abstracción que no se reutiliza ni aporta comportamiento adicional en este prototipo pequeño.

## Consecuencias

- Positivas: las reglas del caso de uso se prueban con xUnit sin iniciar un servidor HTTP; el controlador queda como adaptador HTTP.
- Negativas: el servicio depende directamente de EF Core y el caso de uso está acoplado al modelo de persistencia; al agregar módulos se deberá revisar si conviene separar dominios o transacciones.

## Fuentes

- [Controlador](../../src/AgroStockTaller.Api/Controllers/CultivosController.cs)
- [Interfaz del servicio](../../src/AgroStockTaller.Api/Services/ICultivoService.cs)
- [Servicio](../../src/AgroStockTaller.Api/Services/CultivoService.cs)
- [Pruebas del servicio](../../tests/AgroStockTaller.Tests/CultivoServiceTests.cs)