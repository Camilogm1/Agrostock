# Plan de pruebas

## Pirámide de pruebas

| Nivel | Qué se verifica | Estado en este repositorio |
|---|---|---|
| Unitarias | Validaciones y reglas de `CultivoService` aisladas con EF Core InMemory. | 7 casos automatizados. |
| Integración | Rutas HTTP, códigos de respuesta, serialización, inyección y persistencia a través de la API. | No hay pruebas automatizadas. |
| Sistema / extremo a extremo | Recorrido de usuario por una interfaz web y persistencia en una base real. | No hay frontend ni pruebas automatizadas de este nivel. |

La CI de GitHub Actions restaura, compila y ejecuta las pruebas unitarias. El uso de EF Core InMemory no sustituye una prueba de integración con MariaDB ni valida sus triggers.

## Pruebas implementadas

Las siete ejecuciones corresponden a cinco métodos de prueba: una prueba de registro, tres casos de una teoría de validación, una prueba de listado y dos pruebas de consulta individual.

| Prueba | Nivel | Resultado esperado |
|---|---|---|
| `RegistrarAsync_ConDatosValidos_CreaCultivoConInventarioInicialEnCero` | Unitaria | Asigna ID, conserva nombre y crea inventario inicial en cero. |
| `RegistrarAsync_ConCampoObligatorioVacio_NoRegistraDatos` con nombre vacío | Unitaria | Lanza `ArgumentException`; no crea cultivo ni inventario. |
| `RegistrarAsync_ConCampoObligatorioVacio_NoRegistraDatos` con tipo en blanco | Unitaria | Lanza `ArgumentException`; no crea cultivo ni inventario. |
| `RegistrarAsync_ConCampoObligatorioVacio_NoRegistraDatos` con lote vacío | Unitaria | Lanza `ArgumentException`; no crea cultivo ni inventario. |
| `ListarAsync_DespuesDeRegistrarCultivos_DevuelveTodosLosRegistrados` | Unitaria | Devuelve los cultivos registrados. |
| `ObtenerPorIdAsync_ConCultivoRegistrado_DevuelveElCultivo` | Unitaria | Devuelve el cultivo solicitado por identificador. |
| `ObtenerPorIdAsync_SinCoincidencia_DevuelveNull` | Unitaria | Devuelve `null` cuando no existe el identificador. |

Fuente: [pruebas xUnit](../tests/AgroStockTaller.Tests/CultivoServiceTests.cs).

## Trazabilidad a criterios de aceptación

No se encontró el backlog formal en el repositorio. Los IDs siguientes son referencias provisionales reconstruidas de los comentarios y pruebas existentes; deben cotejarse con las historias oficiales antes de entregar.

| Referencia provisional | Dado | Cuando | Entonces | Prueba |
|---|---|---|---|---|
| RF-01 / AC-01 | Un administrador dispone de nombre, tipo, lote y fecha válidos. | Registra un cultivo. | El cultivo se crea y se devuelve con identificador. | `RegistrarAsync_ConDatosValidos_CreaCultivoConInventarioInicialEnCero` |
| RNF-06 / AC-02 | El nombre, tipo o lote está vacío o en blanco. | Se intenta registrar el cultivo. | La operación falla con `ArgumentException` y no guarda cultivo ni inventario. | `RegistrarAsync_ConCampoObligatorioVacio_NoRegistraDatos` (3 datos) |
| RF-07 / AC-03 | Se registra un cultivo válido. | El servicio termina el registro. | Existe inventario asociado con cantidad disponible igual a cero. | `RegistrarAsync_ConDatosValidos_CreaCultivoConInventarioInicialEnCero` |
| RF-02 / AC-04 | Hay cultivos registrados. | Se solicita el listado. | La respuesta contiene todos los cultivos guardados. | `ListarAsync_DespuesDeRegistrarCultivos_DevuelveTodosLosRegistrados` |
| RF-02 / AC-05 | Existe o no un cultivo con el identificador consultado. | Se solicita `GET /api/cultivos/{id}`. | El servicio devuelve el cultivo o `null`; el controlador responde `200` o `404`, respectivamente. | `ObtenerPorIdAsync_ConCultivoRegistrado_DevuelveElCultivo`; `ObtenerPorIdAsync_SinCoincidencia_DevuelveNull`; códigos HTTP solo verificados manualmente |

## Verificación manual disponible

Swagger permite probar `POST /api/cultivos` y `GET /api/cultivos` al ejecutar la API localmente. Este recorrido es una comprobación manual, no se cuenta como prueba automatizada ni reemplaza pruebas de integración. Véanse las instrucciones del [README](../README.md).

Nota de alcance: la prueba RF-07 refleja el comportamiento simplificado del prototipo. El trigger en `Base de datosV2.txt` crea el inventario al registrar la primera cosecha; esa diferencia debe resolverse al integrar ambos modelos.