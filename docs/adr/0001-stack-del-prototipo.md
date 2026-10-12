# ADR-0001: Stack del prototipo evaluable

- Estado: Aceptada para el prototipo actual
- Fecha: 2026-10-12

## Contexto

El repositorio contiene una API para el taller de CI, no la aplicación completa descrita por los modelos. La solución apunta a .NET 8, usa ASP.NET Core, EF Core InMemory y xUnit. El README indica que esta versión es autocontenida y que no instala un motor de base de datos. Los diagramas y el script SQL, en cambio, describen React, Laravel/Eloquent y MariaDB.

## Decisión

Se conserva el stack que ya compila en CI para ampliar el prototipo evaluable: ASP.NET Core sobre .NET 8, EF Core InMemory y xUnit. Esta decisión limita el alcance al prototipo del repositorio y no declara que sea el stack de producción de AgroStock.

## Alternativas descartadas

- Migrar ahora a React, Laravel/Eloquent y MariaDB: coincide con los diagramas, pero reemplaza la implementación existente y requiere configurar otro runtime y motor de datos para desarrollar y probar.
- Incorporar MariaDB a la API actual: permite persistencia real, pero no está configurado en el proyecto ni en el pipeline actual.

## Consecuencias

- Positivas: se reutiliza el código y el pipeline existente; las pruebas pueden ejecutarse sin un servicio externo.
- Negativas: los datos no persisten al reiniciar la API; EF Core InMemory no valida las restricciones, triggers ni transacciones del script MariaDB; la arquitectura del prototipo difiere de los diagramas.
- Pendiente: tratar la diferencia entre el prototipo y los modelos como una brecha explícita antes de afirmar que el prototipo implementa la arquitectura completa.

## Fuentes

- [README](../../README.md)
- [Proyecto API](../../src/AgroStockTaller.Api/AgroStockTaller.Api.csproj)
- [Registro de servicios](../../src/AgroStockTaller.Api/Program.cs)
- [Pipeline CI](../../.github/workflows/ci.yml)