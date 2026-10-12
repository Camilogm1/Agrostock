# Matriz de riesgos

Fecha de revisión: 2026-10-12. No se encontró en el repositorio la matriz original de la semana 8. Esta revisión registra evidencia disponible ahora; no permite reconstruir ni atribuir estados previos a riesgos que no estén documentados.

Escala: probabilidad e impacto de 1 (bajo) a 5 (alto); exposición = probabilidad × impacto.

| ID | Riesgo / evidencia | Prob. | Impacto | Exposición | Estado | Respuesta |
|---|---|---:|---:|---:|---|---|
| R-01 | La arquitectura del prototipo .NET difiere de los modelos React/Laravel/MariaDB. | 5 | 5 | 25 | Materializado | Mantener explícito que .NET es el stack del prototipo; acordar si los diagramas representan el producto futuro antes de declarar cumplimiento integral. |
| R-02 | EF Core InMemory pierde los datos al reiniciar y no reproduce el comportamiento de MariaDB. | 5 | 4 | 20 | Nuevo riesgo técnico | No usar la demo como persistencia real; añadir proveedor relacional y pruebas de integración si el alcance lo requiere. |
| R-03 | El caso de uso de registro puede guardar cultivo sin inventario si falla la segunda escritura con una base relacional. | 3 | 5 | 15 | Nuevo riesgo técnico | Hacer ambas escrituras atómicas y probar rollback con proveedor relacional. |
| R-04 | El prototipo solo expone registro y listado de cultivos; no demuestra cosechas, ventas, clientes ni autenticación de los modelos. | 5 | 5 | 25 | Brecha de alcance vigente | Priorizar historias del backlog y ampliar por módulo; no presentar la API actual como producto completo. |
| R-05 | Sin pruebas de integración, los contratos HTTP y la configuración real de almacenamiento pueden romperse sin detección automática. | 4 | 4 | 16 | Nuevo riesgo técnico | Incorporar pruebas de API con servidor de prueba y almacenamiento adecuado cuando se definan los contratos. |
| R-06 | Que un cambio se integre sin compilar o ejecutar pruebas. | 2 | 4 | 8 | Cerrado en el repo; Actions remoto por verificar | Existe workflow para push y pull request que restaura, compila y ejecuta pruebas. La ejecución local fue exitosa; verificar que el último run remoto esté verde antes de entregar. |
| R-07 | El Modelo 2 (diagrama de clases) depende de un enlace externo de Google Drive; la pestaña local del SVG solo contiene una anotación. | 3 | 4 | 12 | Mitigado con enlace en el PDF; permisos por verificar | Confirmar acceso para los evaluadores y, si las reglas del curso lo permiten, guardar una copia fuente editable en `docs/modelos/`. |

R-06 se considera cerrado respecto a la ausencia de automatización porque el workflow está versionado en [CI](../.github/workflows/ci.yml). No afirmo que otros riesgos históricos de semana 8 estén cerrados: falta su matriz fuente y no se consultó el resultado remoto de GitHub Actions.