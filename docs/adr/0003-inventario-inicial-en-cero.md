# ADR-0003: Inventario inicial en cero al registrar un cultivo

- Estado: Aceptada para el prototipo actual
- Fecha: 2026-10-12

## Contexto

El registro de un cultivo en el prototipo crea también una fila de inventario con cantidad disponible igual a cero. La prueba de aceptación de `RegistrarAsync` verifica ese comportamiento. El script `Base de datosV2.txt` describe otro flujo: el inventario se crea o incrementa al registrar una cosecha.

## Decisión

Para el prototipo actual, crear una fila de inventario en cero inmediatamente después de guardar el cultivo. Esta regla es parte del caso de uso simplificado y no sustituye el flujo de cosechas definido en el script SQL.

## Alternativas descartadas

- Crear el inventario únicamente al registrar la primera cosecha, como en el trigger de `Base de datosV2.txt`: coincide con el flujo del modelo de datos completo, pero requeriría implementar cosechas en esta API.
- No crear inventario al registrar el cultivo y esperar a una operación explícita de inventario: agrega un paso manual y no satisface la prueba existente del prototipo.

## Consecuencias

- Positivas: el cultivo recién creado siempre tiene inventario consultable; el comportamiento está cubierto por una prueba.
- Negativas: el registro y la creación del inventario son dos llamadas a `SaveChangesAsync`; con una base relacional, un fallo entre ambas puede dejar datos parciales si no se usa una transacción.
- Negativas: difiere del trigger de MariaDB, que inicializa el inventario en la primera cosecha. La integración de ambos comportamientos requiere una decisión antes de conectar una base persistente.

## Fuentes

- [Servicio de cultivos](../../src/AgroStockTaller.Api/Services/CultivoService.cs)
- [Pruebas de cultivos](../../tests/AgroStockTaller.Tests/CultivoServiceTests.cs)
- [Script de base de datos v2](../../Base%20de%20datosV2.txt)