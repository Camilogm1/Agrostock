import { useEffect, useState } from "react";
import axiosClient from "../api/axiosClient";

export default function Cosechas() {
  const [cultivos, setCultivos] = useState([]);
  const [form, setForm] = useState({ idCultivo: "", cantidad: "", fecha: "" });
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
      }); // RF-05/06/07
      setMensaje("Cosecha registrada. El inventario se actualizó automáticamente.");
      cargarHistorial(form.idCultivo);
      setForm({ ...form, cantidad: "", fecha: "" });
    } catch (err) {
      setError(err.message); // ej: RF-10 cultivo inexistente
    }
  }

  return (
    <div style={{ padding: "1rem" }}>
      <h2>Cosechas</h2>

      <form onSubmit={handleSubmit} style={{ marginBottom: "1rem" }}>
        <select
          value={form.idCultivo}
          onChange={(e) => { setForm({ ...form, idCultivo: e.target.value }); cargarHistorial(e.target.value); }}
          required
        >
          <option value="">-- Selecciona un cultivo --</option>
          {cultivos.map((c) => (
            <option key={c.idCultivo} value={c.idCultivo}>{c.nombre} ({c.lote})</option>
          ))}
        </select>
        <input type="number" step="0.01" placeholder="Cantidad cosechada" value={form.cantidad}
          onChange={(e) => setForm({ ...form, cantidad: e.target.value })} required />
        <input type="date" value={form.fecha}
          onChange={(e) => setForm({ ...form, fecha: e.target.value })} required />
        <button type="submit">Registrar cosecha</button>
      </form>

      {mensaje && <p style={{ color: "green" }}>{mensaje}</p>}
      {error && <p style={{ color: "red" }}>{error}</p>}

      <h3>Historial de cosechas {form.idCultivo ? "del cultivo seleccionado" : ""}</h3>
      <table border="1" cellPadding="6">
        <thead><tr><th>Cantidad</th><th>Fecha</th></tr></thead>
        <tbody>
          {historial.map((h) => (
            <tr key={h.idCosecha}>
              <td>{h.cantidad}</td>
              <td>{new Date(h.fecha).toLocaleDateString()}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
