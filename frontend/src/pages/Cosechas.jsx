import { useEffect, useState } from "react";
import axiosClient from "../api/axiosClient";
import { formatearFecha, hoy } from "../utils/fechas";

export default function Cosechas() {
  const [cultivos, setCultivos] = useState([]);
  const [form, setForm] = useState({ idCultivo: "", cantidad: "", fecha: hoy() });
  const [historial, setHistorial] = useState([]);
  const [error, setError] = useState("");
  const [mensaje, setMensaje] = useState("");

  async function cargarCultivos() {
    try {
      const { data } = await axiosClient.get("/cultivos");
      setCultivos(data);
    } catch (err) {
      setError(err.message);
    }
  }

  useEffect(() => { cargarCultivos(); }, []);

  async function cargarHistorial(idCultivo) {
    if (!idCultivo) { setHistorial([]); return; }
    try {
      const { data } = await axiosClient.get(`/cultivos/${idCultivo}/cosechas`); // RF-11
      setHistorial(data);
    } catch (err) {
      setError(err.message);
    }
  }

  async function handleSubmit(e) {
    e.preventDefault();
    setError("");
    setMensaje("");
    try {
      await axiosClient.post("/cosechas", {
        idCultivo: Number(form.idCultivo),
        cantidad: Number(form.cantidad),
        fecha: form.fecha,
      });
      setMensaje("Cosecha registrada. El inventario se actualizó automáticamente.");
      cargarHistorial(form.idCultivo);
      setForm({ ...form, cantidad: "" });
    } catch (err) {
      setError(err.message);
    }
  }

  function handleCultivoChange(e) {
    const idCultivo = e.target.value;
    setForm({ ...form, idCultivo });
    setMensaje("");
    cargarHistorial(idCultivo);
  }

  const cultivoSeleccionado = cultivos.find((cultivo) => String(cultivo.idCultivo) === form.idCultivo);

  return (
    <main className="page-content">
      <div className="page-heading">
        <div>
          <span className="eyebrow">PRODUCCIÓN DEL CAMPO</span>
          <h1>Cosechas</h1>
          <p>Registra la producción recolectada y consulta su historial.</p>
        </div>
        <div className="heading-date">
          <span className="heading-date-dot" />
          <span>Seguimiento de producción</span>
        </div>
      </div>

      <section className="summary-grid" aria-label="Resumen de cosechas">
        <div className="summary-card">
          <div className="summary-icon" aria-hidden="true">
            <svg viewBox="0 0 24 24" fill="none">
              <path d="M4 19h16M6 16l2-5 3 2 4-7 3 3" />
            </svg>
          </div>
          <div className="summary-copy">
            <span>Cultivos registrados</span>
            <strong>{cultivos.length}</strong>
          </div>
        </div>
        <div className="summary-card">
          <div className="summary-icon summary-icon-earth" aria-hidden="true">✳</div>
          <div className="summary-copy">
            <span>Registros del cultivo</span>
            <strong>{historial.length}</strong>
          </div>
        </div>
      </section>

      {mensaje && <p className="alert alert-success" role="status">{mensaje}</p>}
      {error && <p className="alert alert-error" role="alert">{error}</p>}

      <section className="panel">
        <div className="panel-heading">
          <div>
            <h2>Registrar cosecha</h2>
            <p>Elige el cultivo y registra la cantidad recolectada.</p>
          </div>
          <span className="panel-heading-mark" aria-hidden="true">+</span>
        </div>
        <form className="harvest-form" onSubmit={handleSubmit}>
          <label className="field">
            <span>Cultivo</span>
            <select value={form.idCultivo} onChange={handleCultivoChange} required>
              <option value="">Selecciona un cultivo</option>
              {cultivos.map((cultivo) => (
                <option key={cultivo.idCultivo} value={cultivo.idCultivo}>
                  {cultivo.nombre} ({cultivo.lote})
                </option>
              ))}
            </select>
          </label>
          <label className="field">
            <span>Cantidad cosechada</span>
            <input
              type="number"
              step="0.01"
              min="0.01"
              placeholder="Ej. 25"
              value={form.cantidad}
              onChange={(e) => setForm({ ...form, cantidad: e.target.value })}
              required
            />
          </label>
          <label className="field">
            <span>Fecha</span>
            <input type="date" value={form.fecha} onChange={(e) => setForm({ ...form, fecha: e.target.value })} required />
          </label>
          <div className="form-actions">
            <button className="button button-primary" type="submit">Registrar cosecha</button>
          </div>
        </form>
      </section>

      <section className="panel">
        <div className="panel-heading">
          <div>
            <h2>Historial de cosechas</h2>
            <p>{cultivoSeleccionado ? `Registros de ${cultivoSeleccionado.nombre} · ${cultivoSeleccionado.lote}` : "Selecciona un cultivo para consultar sus registros."}</p>
          </div>
          <span className="table-count">{historial.length} {historial.length === 1 ? "registro" : "registros"}</span>
        </div>
        <div className="table-scroll">
          <table className="data-table">
            <thead>
              <tr><th>Cantidad</th><th>Fecha</th></tr>
            </thead>
            <tbody>
              {historial.map((registro) => (
                <tr key={registro.idCosecha}>
                  <td><span className="stock-quantity">{registro.cantidad}</span></td>
                  <td>{formatearFecha(registro.fecha)}</td>
                </tr>
              ))}
              {historial.length === 0 && (
                <tr>
                  <td className="empty-state" colSpan={2}>
                    <span className="empty-state-icon" aria-hidden="true">✳</span>
                    <strong>{cultivoSeleccionado ? "Aún no hay cosechas registradas" : "Selecciona un cultivo"}</strong>
                    <span>{cultivoSeleccionado ? "Cuando registres una cosecha, aparecerá aquí." : "El historial se mostrará al elegir un cultivo."}</span>
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </section>
    </main>
  );
}
