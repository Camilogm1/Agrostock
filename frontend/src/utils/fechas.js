// Fecha de hoy en formato yyyy-mm-dd (hora local) para los <input type="date">
export function hoy() {
  const d = new Date();
  return new Date(d.getTime() - d.getTimezoneOffset() * 60000).toISOString().substring(0, 10);
}

// Las fechas sin hora llegan como "2026-03-01T00:00:00"; se muestran tal cual, sin conversión de zona horaria
export function formatearFecha(valor) {
  if (!valor) return "";
  const [anio, mes, dia] = valor.substring(0, 10).split("-");
  return `${dia}/${mes}/${anio}`;
}
