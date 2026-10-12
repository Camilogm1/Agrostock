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
`POST /api/cultivos`, `GET /api/cultivos` y `GET /api/cultivos/{id}` manualmente.

> Nota importante para Windows: si el repositorio está dentro de OneDrive, el ensamblado de pruebas puede quedar bloqueado por la protección de "Control de aplicaciones". Para evitarlo, clona o copia el proyecto en una carpeta normal como `C:\Users\<tu_usuario>\source\repos\Agrostock`.

## Cómo correr las pruebas
```bash
cd tests/AgroStockTaller.Tests
dotnet restore
dotnet test
```
Debe mostrar 7 casos aprobados y ninguno fallido.

## Estado del pipeline
![CI](https://github.com/Camilogm1/Agrostock/actions/workflows/ci.yml/badge.svg)

## Documentación de entrega
- [ADR](docs/adr/)
- [Matriz de decisión del stack](docs/matriz-decision-stack.md)
- [Plan de pruebas](docs/plan-pruebas.md)
- [Registro de deuda técnica](docs/deuda-tecnica.md)
- [Matriz de riesgos](docs/matriz-riesgos.md)
- [Trazabilidad](docs/trazabilidad.md)
- [Nota de uso de IA](docs/nota-uso-ia.md)
- [Modelos editables](docs/modelos/)
- [Documento consolidado en PDF](docs/entrega-consolidada.pdf)
- [Versión editable en Word](docs/entrega-consolidada.docx)

El Modelo 2 (diagrama de clases) se consulta desde
[Google Drive](https://drive.google.com/file/d/1EtywC6Lw0LVnIORtykZYBzBQQKQpjeBf/view?usp=sharing).
Para visualizarlo con facilidad, abre el enlace con draw.io (diagrams.net). La
pestaña homónima de los SVG locales solo contiene una anotación.

Para regenerar el PDF en Windows, instala las dependencias con
`py -m pip install -r docs/requirements-pdf.txt` y ejecuta
`py scripts/build_delivery_pdf.py`. Se requiere Microsoft Edge o Google Chrome.
Para regenerar la versión editable en Word, instala `docs/requirements-word.txt`
y ejecuta `py scripts/build_delivery_docx.py`.


## Integrantes
- Juan Felipe Cardona
- Juan Camilo González
- Daniel Villa
