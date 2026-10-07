import { useEffect, useState } from "react";
import axiosClient from "../api/axiosClient";

export default function Ventas() {
  const [clientes, setClientes] = useState([]);
  const [inventario, setInventario] = useState([]);
  const [form, setForm] = useState({ idCliente: "", idInventario: "", cantidad: "", fecha: "" });
  const [ventas, setVentas] = useState([]);
  const [filtroCliente, setFiltroCliente] = useState("");
  const [error, setError] = useState("");
  const [mensaje, setMensaje] = useState("");

  async function cargarListas() {
    try {
      const [resClientes, resInventario] = await Promise.all([
        axiosClient.get("/clientes"),
        axiosClient.get("/inventario"),
      ]);
      setClientes(resClientes.data);
      setInventario(resInventario.data); // RF-17: mostrar stock disponible en UI
    } catch (err) {
      setError(err.message);
    }
  }

  async function cargarVentas() {
    try {
      const { data } = await axiosClient.get("/ventas", { params: { cliente: filtroCliente } }); // RF-20/22
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
      }); // RF-14 a RF-19
      setMensaje("Venta registrada. El inventario se descontó automáticamente.");
      setForm({ idCliente: "", idInventario: "", cantidad: "", fecha: "" });
      cargarListas();
      cargarVentas();
    } catch (err) {
      setError(err.message); // RF-18: "Stock insuficiente para..."
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
            <option key={i.idInventario} value={i.idInventario}>
              {i.nombreProducto} (disponible: {i.cantidadDisponible})
            </option>
          ))}
        </select>

        <input type="number" step="0.01" placeholder="Cantidad" value={form.cantidad}
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
      <button onClick={cargarVentas}>Buscar</button>

      <table border="1" cellPadding="6" style={{ marginTop: "0.5rem" }}>
        <thead><tr><th>Cliente</th><th>Fecha</th><th>Producto</th><th>Cantidad</th></tr></thead>
        <tbody>
          {ventas.map((v) => (
            v.detalles.map((d, idx) => (
              <tr key={`${v.idVenta}-${idx}`}>
                <td>{v.nombreCliente}</td>
                <td>{new Date(v.fecha).toLocaleDateString()}</td>
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
