from __future__ import annotations

import html
import os
import re
import shutil
import subprocess
import tempfile
from datetime import date
from pathlib import Path

from reportlab.lib import colors
from reportlab.lib.enums import TA_CENTER, TA_LEFT
from reportlab.lib.pagesizes import letter
from reportlab.lib.styles import ParagraphStyle, getSampleStyleSheet
from reportlab.lib.units import mm
from reportlab.platypus import (
    BaseDocTemplate,
    Frame,
    PageBreak,
    PageTemplate,
    Paragraph,
    Spacer,
    Table,
    TableStyle,
)
from pypdf import PdfReader, PdfWriter


ROOT = Path(__file__).resolve().parents[1]
DOCS = ROOT / "docs"
OUTPUT = DOCS / "entrega-consolidada.pdf"
PAGE_WIDTH, PAGE_HEIGHT = letter
MARGIN = 17 * mm

SECTIONS = [
    ("1. Modelos UML y C4", None),
    ("2. Decisiones de arquitectura (ADR)", [
        DOCS / "adr/0001-stack-del-prototipo.md",
        DOCS / "adr/0002-logica-de-negocio-en-servicio.md",
        DOCS / "adr/0003-inventario-inicial-en-cero.md",
    ]),
    ("3. Matriz de decisión del stack", [DOCS / "matriz-decision-stack.md"]),
    ("4. Plan de pruebas", [DOCS / "plan-pruebas.md"]),
    ("5. Prototipo funcionando", [ROOT / "README.md"]),
    ("6. Registro de deuda técnica", [DOCS / "deuda-tecnica.md"]),
    ("7. Matriz de riesgos actualizada", [DOCS / "matriz-riesgos.md"]),
    ("8. Tabla de trazabilidad", [DOCS / "trazabilidad.md"]),
    ("9. Nota de uso de IA", [DOCS / "nota-uso-ia.md"]),
    ("10. Documento consolidado y fuentes", None),
]

DIAGRAMS = [
    ("Modelo 1. Diagrama de caso de uso", "Agrostock-Caso de uso.drawio.svg"),
    ("Modelo 3. Diagrama de secuencia", "Agrostock-Diagrama de secuencia.drawio.svg"),
    ("Modelo 4. Componentes y despliegue", "Agrostock-Diagrama de componentes y despliegue.drawio.svg"),
    ("Modelo 5. Diagrama C4: nivel 1", "Agrostock-C4 Nivel 1.drawio.svg"),
    ("Modelo 6. Diagrama C4: nivel 2", "Agrostock- C4 Nivel 2.drawio.svg"),
]
CLASS_DIAGRAM_URL = "https://drive.google.com/file/d/1EtywC6Lw0LVnIORtykZYBzBQQKQpjeBf/view?usp=sharing"

styles = getSampleStyleSheet()
styles.add(ParagraphStyle(
    name="CoverTitle", parent=styles["Title"], fontName="Helvetica-Bold",
    fontSize=28, leading=34, textColor=colors.HexColor("#174A35"),
    alignment=TA_CENTER, spaceAfter=12 * mm,
))
styles.add(ParagraphStyle(
    name="CoverSubtitle", parent=styles["Normal"], fontSize=15,
    leading=21, alignment=TA_CENTER, textColor=colors.HexColor("#40554C"),
))
styles.add(ParagraphStyle(
    name="SectionTitle", parent=styles["Heading1"], fontName="Helvetica-Bold",
    fontSize=19, leading=23, textColor=colors.HexColor("#174A35"),
    spaceBefore=4 * mm, spaceAfter=5 * mm,
))
styles.add(ParagraphStyle(
    name="Subsection", parent=styles["Heading2"], fontSize=13,
    leading=16, textColor=colors.HexColor("#245B48"),
    spaceBefore=4 * mm, spaceAfter=2 * mm,
))
styles.add(ParagraphStyle(
    name="BodySmall", parent=styles["BodyText"], fontSize=8.5,
    leading=11, alignment=TA_LEFT, spaceAfter=2 * mm,
))
styles.add(ParagraphStyle(
    name="TableCell", parent=styles["BodyText"], fontSize=7,
    leading=9, spaceAfter=0,
))
styles.add(ParagraphStyle(
    name="TableHeader", parent=styles["BodyText"], fontName="Helvetica-Bold",
    fontSize=7, leading=9, textColor=colors.white, spaceAfter=0,
))
styles.add(ParagraphStyle(
    name="TOCItem", parent=styles["BodyText"], fontSize=12,
    leading=19, leftIndent=4 * mm, textColor=colors.HexColor("#24382F"),
))


def inline_markup(text: str) -> str:
    text = html.escape(text, quote=False)
    text = re.sub(
        r"\[([^\]]+)\]\(([^)]+)\)",
        lambda match: f'<link href="{html.escape(match.group(2), quote=True)}" color="#176B55">{match.group(1)}</link>',
        text,
    )
    text = re.sub(r"`([^`]+)`", r'<font name="Courier">\1</font>', text)
    text = re.sub(r"\*\*(.+?)\*\*", r"<b>\1</b>", text)
    text = re.sub(r"(?<!\*)\*([^*]+)\*(?!\*)", r"<i>\1</i>", text)
    text = re.sub(r"!\[([^\]]*)\]\([^)]+\)", r"[\1]", text)
    return text


def markdown_table(lines: list[str], width: float) -> Table:
    raw_rows = []
    for line in lines:
        cells = [cell.strip() for cell in line.strip().strip("|").split("|")]
        if cells and all(re.fullmatch(r":?-{2,}:?", cell.replace(" ", "")) for cell in cells):
            continue
        raw_rows.append(cells)

    column_count = max(len(row) for row in raw_rows)
    data = []
    for row_index, row in enumerate(raw_rows):
        row += [""] * (column_count - len(row))
        style = styles["TableHeader"] if row_index == 0 else styles["TableCell"]
        data.append([Paragraph(inline_markup(cell), style) for cell in row])

    table = Table(data, colWidths=[width / column_count] * column_count, repeatRows=1, hAlign="LEFT")
    table.setStyle(TableStyle([
        ("BACKGROUND", (0, 0), (-1, 0), colors.HexColor("#245B48")),
        ("GRID", (0, 0), (-1, -1), 0.35, colors.HexColor("#AEBBB4")),
        ("VALIGN", (0, 0), (-1, -1), "TOP"),
        ("LEFTPADDING", (0, 0), (-1, -1), 4),
        ("RIGHTPADDING", (0, 0), (-1, -1), 4),
        ("TOPPADDING", (0, 0), (-1, -1), 4),
        ("BOTTOMPADDING", (0, 0), (-1, -1), 4),
        ("ROWBACKGROUNDS", (0, 1), (-1, -1), [colors.white, colors.HexColor("#F1F5F2")]),
    ]))
    return table


def markdown_flowables(path: Path, width: float) -> list:
    result = []
    lines = path.read_text(encoding="utf-8").splitlines()
    index = 0
    while index < len(lines):
        line = lines[index].strip()
        if not line or line == "---":
            index += 1
            continue
        if line.startswith("|"):
            table_lines = []
            while index < len(lines) and lines[index].strip().startswith("|"):
                table_lines.append(lines[index].strip())
                index += 1
            result.append(markdown_table(table_lines, width))
            result.append(Spacer(1, 3 * mm))
            continue
        heading = re.match(r"^(#{1,6})\s+(.+)$", line)
        if heading:
            level = len(heading.group(1))
            if level == 1:
                index += 1
                continue
            result.append(Paragraph(inline_markup(heading.group(2)), styles["Subsection"]))
        elif line.startswith("- "):
            result.append(Paragraph("&#8226; " + inline_markup(line[2:]), styles["BodySmall"]))
        elif re.match(r"^\d+\.\s+", line):
            result.append(Paragraph(inline_markup(line), styles["BodySmall"]))
        elif line.startswith("> "):
            result.append(Paragraph("<i>" + inline_markup(line[2:]) + "</i>", styles["BodySmall"]))
        else:
            result.append(Paragraph(inline_markup(line), styles["BodySmall"]))
        result.append(Spacer(1, 1.2 * mm))
        index += 1
    return result


def find_browser() -> Path:
    candidates = [shutil.which("msedge"), shutil.which("chrome")]
    for variable, relative in [
        ("PROGRAMFILES(X86)", "Microsoft/Edge/Application/msedge.exe"),
        ("PROGRAMFILES", "Microsoft/Edge/Application/msedge.exe"),
        ("PROGRAMFILES", "Google/Chrome/Application/chrome.exe"),
    ]:
        base = os.environ.get(variable)
        if base:
            candidates.append(str(Path(base) / relative))
    for candidate in candidates:
        if candidate and Path(candidate).is_file():
            return Path(candidate)
    raise RuntimeError("Instala Microsoft Edge o Google Chrome para renderizar los SVG.")


def render_diagram(path: Path, temp_dir: Path, browser: Path) -> Path:
    slug = re.sub(r"[^a-z0-9]+", "-", path.stem.lower()).strip("-")
    svg_path = temp_dir / f"{slug}.svg"
    html_path = temp_dir / f"{slug}.html"
    pdf_path = temp_dir / f"{slug}.pdf"
    profile_path = temp_dir / f"profile-{slug}"
    source = path.read_text(encoding="utf-8")
    source = source.replace("color-scheme: light dark", "color-scheme: light")
    svg_path.write_text(source, encoding="utf-8")
    html_path.write_text(
        "<!doctype html><html><head><meta charset='utf-8'>"
        "<style>@page{size:letter landscape;margin:0}"
        "html,body{width:100%;height:100%;margin:0;padding:0}"
        "img{display:block;width:100%;height:100%;object-fit:contain}</style>"
        f"</head><body><img src='{svg_path.as_uri()}'></body></html>",
        encoding="utf-8",
    )
    subprocess.run(
        [
            str(browser), "--headless=new", "--disable-gpu", "--disable-extensions",
            "--hide-scrollbars", "--no-pdf-header-footer",
            f"--user-data-dir={profile_path}", f"--print-to-pdf={pdf_path}",
            html_path.as_uri(),
        ],
        check=True,
        capture_output=True,
        text=True,
        timeout=90,
    )
    if not pdf_path.is_file():
        raise RuntimeError(f"El navegador no generó el PDF para {path.name}")
    if len(PdfReader(str(pdf_path)).pages) != 1:
        raise RuntimeError(f"Se esperaba una página para el modelo {path.name}")
    return pdf_path


def draw_page(canvas, document) -> None:
    canvas.saveState()
    if document.page > 1:
        canvas.setStrokeColor(colors.HexColor("#C5D0CA"))
        canvas.line(MARGIN, 13 * mm, PAGE_WIDTH - MARGIN, 13 * mm)
        canvas.setFont("Helvetica", 8)
        canvas.setFillColor(colors.HexColor("#66766E"))
        canvas.drawString(MARGIN, 8 * mm, "AgroStock | Entrega 2 | Rama Daniel")
        canvas.drawRightString(PAGE_WIDTH - MARGIN, 8 * mm, str(document.page))
    canvas.restoreState()


def build_pdf() -> None:
    available_width = PAGE_WIDTH - 2 * MARGIN
    browser = find_browser()
    with tempfile.TemporaryDirectory(prefix="agrostock-delivery-") as temporary:
        temp_dir = Path(temporary)
        base_pdf = temp_dir / "document-text.pdf"
        doc = BaseDocTemplate(
            str(base_pdf), pagesize=letter, leftMargin=MARGIN, rightMargin=MARGIN,
            topMargin=MARGIN, bottomMargin=19 * mm,
            title="AgroStock - Entrega 2", author="Equipo AgroStock",
        )
        frame = Frame(MARGIN, 19 * mm, available_width, PAGE_HEIGHT - 36 * mm, id="main")
        doc.addPageTemplates([PageTemplate(id="main", frames=frame, onPage=draw_page)])

        story = [
            Spacer(1, 31 * mm),
            Paragraph("AgroStock", styles["CoverTitle"]),
            Paragraph("Entrega 2<br/>Ingeniería de Software", styles["CoverSubtitle"]),
            Spacer(1, 25 * mm),
            Paragraph("Equipo", styles["Subsection"]),
            Paragraph("Juan Felipe Cardona<br/>Camilo González<br/>Daniel Villa", styles["CoverSubtitle"]),
            Spacer(1, 16 * mm),
            Paragraph(date.today().strftime("%Y-%m-%d"), styles["CoverSubtitle"]),
            Spacer(1, 8 * mm),
            Paragraph(
                '<link href="https://github.com/Camilogm1/Agrostock" color="#176B55">Repositorio: Camilogm1/Agrostock</link>',
                styles["CoverSubtitle"],
            ),
            PageBreak(),
            Paragraph("Índice", styles["SectionTitle"]),
        ]
        for title, _ in SECTIONS:
            story.append(Paragraph(inline_markup(title), styles["TOCItem"]))
        story.append(PageBreak())

        for title, sources in SECTIONS:
            story.append(Paragraph(inline_markup(title), styles["SectionTitle"]))
            if title.startswith("1."):
                story.append(Paragraph(
                    "El taller solicita seis modelos. El diagrama de clases se entrega como Modelo 2 mediante el enlace de Drive de la página siguiente. La pestaña con ese nombre en los SVG locales contiene solo una anotación; el modelo vinculado es el que debe usarse para su visualización.",
                    styles["BodySmall"],
                ))
                story.append(PageBreak())
                for diagram_index, (caption, filename) in enumerate(DIAGRAMS):
                    marker = f"DIAGRAM_PLACEHOLDER:{filename}"
                    story.append(Paragraph(inline_markup(caption), styles["Subsection"]))
                    story.append(Paragraph(marker, styles["BodySmall"]))
                    story.append(PageBreak())
                    if diagram_index == 0:
                        story.append(Paragraph("Modelo 2. Diagrama de clases", styles["SectionTitle"]))
                        story.append(Paragraph(
                            f'<link href="{CLASS_DIAGRAM_URL}" color="#176B55">Abrir el Modelo 2 en Google Drive</link>',
                            styles["BodySmall"],
                        ))
                        story.append(Paragraph(
                            "Para visualizarlo con facilidad, abre el enlace con draw.io (diagrams.net).",
                            styles["BodySmall"],
                        ))
                        story.append(PageBreak())
            elif title.startswith("10."):
                story.extend([
                    Paragraph("Este PDF consolida los componentes de la entrega. Las cinco fuentes SVG están en `docs/modelos/`; el Modelo 2 se consulta desde el enlace externo de Google Drive. Las instrucciones del generador y sus dependencias están en `scripts/build_delivery_pdf.py` y `docs/requirements-pdf.txt`.", styles["BodySmall"]),
                    Paragraph("Para regenerarlo desde la raíz del repositorio:", styles["Subsection"]),
                    Paragraph("py -m pip install -r docs/requirements-pdf.txt<br/>py scripts/build_delivery_pdf.py", styles["BodySmall"]),
                    Paragraph("El prototipo es una API .NET autocontenida. El alcance y las diferencias respecto a los modelos están descritos en ADR-0001 y la matriz de riesgos; no se presenta como implementación completa de React/Laravel/MariaDB.", styles["BodySmall"]),
                ])
            else:
                for source in sources or []:
                    story.extend(markdown_flowables(source, available_width))
            story.append(PageBreak())

        doc.build(story)
        diagram_readers = {}
        for _, filename in DIAGRAMS:
            rendered = render_diagram(DOCS / "modelos" / filename, temp_dir, browser)
            diagram_readers[filename] = PdfReader(str(rendered))

        writer = PdfWriter()
        for page in PdfReader(str(base_pdf)).pages:
            text = page.extract_text() or ""
            filename = next((name for _, name in DIAGRAMS if f"DIAGRAM_PLACEHOLDER:{name}" in text), None)
            if filename:
                writer.add_page(diagram_readers[filename].pages[0])
            else:
                writer.add_page(page)
        writer.add_metadata({"/Title": "AgroStock - Entrega 2", "/Author": "Equipo AgroStock"})
        with OUTPUT.open("wb") as output_file:
            writer.write(output_file)
    print(f"PDF generado: {OUTPUT.relative_to(ROOT)} ({OUTPUT.stat().st_size:,} bytes)")


if __name__ == "__main__":
    build_pdf()