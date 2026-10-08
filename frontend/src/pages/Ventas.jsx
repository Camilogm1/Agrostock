import { useEffect, useState } from "react";
import axiosClient from "../api/axiosClient";
import { formatearFecha, hoy } from "../utils/fechas";

const FORM_VACIO = { idCliente: "", idInventario: "", cantidad: "" };

export default function Ventas() {
  const [clientes, setClientes] = useState([]);
  const [inventario, setInventario] = useState([]);
  const [form, setForm] = useState({ ...FORM_VACIO, fecha: hoy() });
  const [ventas, setVentas] = useState([]);
  const [filtroCliente, setFiltroCliente] = useState("");
  const [filtroFecha, setFiltroFecha] = useState("");
  const [error, setError] = useState("");
  const [mensaje, setMensaje] = useState("");

  async function cargarListas() {
    try {
      const [resClientes, resInventario] = await Promise.all([
        axiosClient.get("/clientes"),
        axiosClient.get("/inventario"),
      ]);
      setClientes(resClientes.data);
      setInventario(resInventario.data);
    } catch (err) {
      setError(err.message);
    }
  }

  async function cargarVentas() {
    try {
      const { data } = await axiosClient.get("/ventas", { params: { cliente: filtroCliente, fecha: filtroFecha || undefined } }); // RF-20/RF-22
      setVentas(data);
    } catch (err) {
      setError(err.message);
    }
  }

  useEffect(() => { cargarListas(); cargarVentas(); }, []);

  async function handleSubmit(e) {
    e.preventDefault();
    setError("");
    setMensaje("");
    try {
      await axiosClient.post("/ventas", {
        idCliente: Number(form.idCliente),
        idInventario: Number(form.idInventario),
        cantidad: Number(form.cantidad),
        fecha: form.fecha,
      });
      setMensaje("Venta registrada. El inventario se descontó automáticamente.");
      setForm({ ...FORM_VACIO, fecha: form.fecha });
      cargarListas();
      cargarVentas();
    } catch (err) {
      setError(err.message);
    }
  }

  return (
    <div style={{ padding: "1rem" }}>
      <h2>Ventas</h2>

      <form onSubmit={handleSubmit} style={{ marginBottom: "1rem" }}>
        <select value={form.idCliente} onChange={(e) => setForm({ ...form, idCliente: e.target.value })} required>
          <option value="">-- Cliente --</option>
          {clientes.map((c) => <option key={c.idCliente} value={c.idCliente}>{c.nombre}</option>)}
        </select>

        <select value={form.idInventario} onChange={(e) => setForm({ ...form, idInventario: e.target.value })} required>
          <option value="">-- Producto --</option>
          {inventario.map((i) => (
            <option key={i.idInventario} value={i.idInventario} disabled={i.cantidadDisponible <= 0}>
              {i.nombreProducto} (disponible: {i.cantidadDisponible})
            </option>
          ))}
        </select>

        <input type="number" step="0.01" min="0.01" placeholder="Cantidad" value={form.cantidad}
          onChange={(e) => setForm({ ...form, cantidad: e.target.value })} required />
        <input type="date" value={form.fecha}
          onChange={(e) => setForm({ ...form, fecha: e.target.value })} required />
        <button type="submit">Registrar venta</button>
      </form>

      {mensaje && <p style={{ color: "green" }}>{mensaje}</p>}
      {error && <p style={{ color: "red" }}>{error}</p>}

      <h3>Historial de ventas</h3>
      <input placeholder="Filtrar por cliente" value={filtroCliente}
        onChange={(e) => setFiltroCliente(e.target.value)}
        onKeyDown={(e) => e.key === "Enter" && cargarVentas()} />
      <input type="date" value={filtroFecha} onChange={(e) => setFiltroFecha(e.target.value)} />
      <button onClick={cargarVentas}>Buscar</button>

      <table border="1" cellPadding="6" style={{ marginTop: "0.5rem" }}>
        <thead><tr><th>Cliente</th><th>Fecha</th><th>Producto</th><th>Cantidad</th></tr></thead>
        <tbody>
          {ventas.map((v) => (
            v.detalles.map((d, idx) => (
              <tr key={`${v.idVenta}-${idx}`}>
                <td>{v.nombreCliente}</td>
                <td>{formatearFecha(v.fecha)}</td>
                <td>{d.nombreProducto}</td>
                <td>{d.cantidad}</td>
              </tr>
            ))
          ))}
        </tbody>
      </table>
    </div>
  );
}
