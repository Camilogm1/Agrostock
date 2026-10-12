# Matriz de decisión del stack

## Criterios y escala

Cada opción recibe una calificación de 1 a 5; 5 es el mejor ajuste. El puntaje total es la suma ponderada. El conocimiento previo pesa doble, como pide la rúbrica.

| Criterio | Peso | Interpretación |
|---|---:|---|
| Lo que el equipo ya sabe | 2 | Evidencia de uso en este repositorio; no sustituye una encuesta a los integrantes. |
| Ajuste a los modelos | 1 | Correspondencia con la arquitectura representada en los diagramas. |
| Continuidad con el repositorio | 1 | Reutilización del código y estructura existentes. |
| Pruebas y CI | 1 | Facilidad para probar y ejecutar en el pipeline actual. |
| Sencillez de ejecución local | 1 | Menos servicios y configuración necesarios para el prototipo. |

Las calificaciones de conocimiento se basan solo en evidencia comprobable: el backend .NET ya tiene código y pruebas; no hay frontend ni código PHP o Node en el repo. Por eso, un 1 significa "sin evidencia en este repositorio", no que el equipo desconozca la tecnología. El equipo debe confirmar estas puntuaciones antes de presentar la matriz como encuesta de experiencia.

## Frontend

| Opción | Conocimiento (x2) | Modelos | Continuidad | CI/pruebas | Ejecución local | Total / 30 |
|---|---:|---:|---:|---:|---:|---:|
| React | 1 (2) | 5 | 4 | 4 | 3 | **18** |
| Vue | 1 (2) | 2 | 4 | 4 | 4 | 16 |
| Blazor | 1 (2) | 1 | 5 | 4 | 5 | 17 |

**Recomendación:** React para una futura interfaz, por ser el frontend que aparece en los modelos. No está implementado en el prototipo actual; Swagger UI es la interfaz disponible para explorar la API.

**Por qué se descarta la segunda opción, Blazor:** se aparta del frontend React representado en los modelos y añadiría una aplicación web que hoy no existe ni hace falta para demostrar los endpoints de la API.

## Backend

| Opción | Conocimiento (x2) | Modelos | Continuidad | CI/pruebas | Ejecución local | Total / 30 |
|---|---:|---:|---:|---:|---:|---:|
| ASP.NET Core .NET 8 + EF Core InMemory | 5 (10) | 2 | 5 | 5 | 5 | **27** |
| Laravel + Eloquent + MariaDB | 1 (2) | 5 | 1 | 3 | 2 | 13 |
| Node.js + Express | 1 (2) | 2 | 1 | 4 | 3 | 12 |

**Decisión para el prototipo:** conservar ASP.NET Core .NET 8, EF Core InMemory y xUnit. Es el stack presente, compila en CI y el usuario confirmó que se debe seguir usando. Esta decisión no afirma que el prototipo ya corresponda a la arquitectura Laravel/MariaDB de los modelos.

**Por qué se descarta la segunda opción, Laravel:** exigiría reemplazar el backend y el pipeline .NET que ya pasan pruebas; además requiere configurar PHP y MariaDB, servicios que no están en el repo ni son necesarios para la demo autocontenida actual.

## Alcance y validación pendiente

La recomendación React es una evaluación para una interfaz futura, no una dependencia agregada. Los puntajes de familiaridad de todo el equipo deben confirmarse con sus integrantes; el repositorio por sí solo solo demuestra que existe código y experiencia de trabajo compartido en .NET.

Fuentes: [README](../README.md), [API](../src/AgroStockTaller.Api/AgroStockTaller.Api.csproj), [pruebas](../tests/AgroStockTaller.Tests/AgroStockTaller.Tests.csproj), [CI](../.github/workflows/ci.yml).