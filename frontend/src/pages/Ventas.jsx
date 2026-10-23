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
      const { data } = await axiosClient.get("/ventas", {
        params: { cliente: filtroCliente, fecha: filtroFecha || undefined },
      }); // RF-20/RF-22
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

  function handleBuscar(e) {
    e.preventDefault();
    cargarVentas();
  }

  const lineasVendidas = ventas.reduce((total, venta) => total + venta.detalles.length, 0);

  return (
    <main className="page-content">
      <div className="page-heading">
        <div>
          <span className="eyebrow">MOVIMIENTO COMERCIAL</span>
          <h1>Ventas</h1>
          <p>Registra tus ventas y consulta el historial de movimientos.</p>
        </div>
        <div className="heading-date">
          <span className="heading-date-dot" />
          <span>Registro de ventas</span>
        </div>
      </div>

      <section className="summary-grid" aria-label="Resumen de ventas">
        <div className="summary-card">
          <div className="summary-icon" aria-hidden="true">
            <svg viewBox="0 0 24 24" fill="none">
              <path d="M4 19V5m0 14h17M8 15l3-4 3 2 5-7m-3 0h3v3" />
            </svg>
          </div>
          <div className="summary-copy">
            <span>Ventas en el historial</span>
            <strong>{ventas.length}</strong>
          </div>
        </div>
        <div className="summary-card">
          <div className="summary-icon summary-icon-earth" aria-hidden="true">↗</div>
          <div className="summary-copy">
            <span>Productos vendidos</span>
            <strong>{lineasVendidas}</strong>
          </div>
        </div>
      </section>

      {mensaje && <p className="alert alert-success" role="status">{mensaje}</p>}
      {error && <p className="alert alert-error" role="alert">{error}</p>}

      <section className="panel">
        <div className="panel-heading">
          <div>
            <h2>Registrar venta</h2>
            <p>Selecciona el cliente, el producto y la cantidad.</p>
          </div>
          <span className="panel-heading-mark" aria-hidden="true">+</span>
        </div>
        <form className="sale-form" onSubmit={handleSubmit}>
          <label className="field">
            <span>Cliente</span>
            <select value={form.idCliente} onChange={(e) => setForm({ ...form, idCliente: e.target.value })} required>
              <option value="">Selecciona un cliente</option>
              {clientes.map((cliente) => (
                <option key={cliente.idCliente} value={cliente.idCliente}>{cliente.nombre}</option>
              ))}
            </select>
          </label>
          <label className="field">
            <span>Producto</span>
            <select value={form.idInventario} onChange={(e) => setForm({ ...form, idInventario: e.target.value })} required>
              <option value="">Selecciona un producto</option>
              {inventario.map((item) => (
                <option key={item.idInventario} value={item.idInventario} disabled={item.cantidadDisponible <= 0}>
                  {item.nombreProducto} (disponible: {item.cantidadDisponible} kg)
                </option>
              ))}
            </select>
          </label>
          <label className="field">
            <span>Cantidad (kg)</span>
            <input
              type="number"
              step="0.01"
              min="0.01"
              placeholder="Ej. 5 kg"
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
            <button className="button button-primary" type="submit">Registrar venta</button>
          </div>
        </form>
      </section>

      <section className="panel">
        <div className="panel-heading">
          <div>
            <h2>Historial de ventas</h2>
            <p>Filtra los movimientos por cliente o fecha.</p>
          </div>
          <span className="table-count">{ventas.length} {ventas.length === 1 ? "venta" : "ventas"}</span>
        </div>
        <form className="search-form sales-search" onSubmit={handleBuscar}>
          <label className="field search-field" htmlFor="ventas-cliente">
            <span>Cliente</span>
            <input
              id="ventas-cliente"
              placeholder="Buscar por cliente"
              value={filtroCliente}
              onChange={(e) => setFiltroCliente(e.target.value)}
            />
          </label>
          <label className="field date-search-field" htmlFor="ventas-fecha">
            <span>Fecha</span>
            <input id="ventas-fecha" type="date" value={filtroFecha} onChange={(e) => setFiltroFecha(e.target.value)} />
          </label>
          <button className="button button-primary" type="submit">Buscar</button>
        </form>
        <div className="table-scroll">
          <table className="data-table">
            <thead>
              <tr><th>Cliente</th><th>Fecha</th><th>Producto</th><th>Cantidad (kg)</th></tr>
            </thead>
            <tbody>
              {ventas.flatMap((venta) => venta.detalles.map((detalle, index) => (
                <tr key={`${venta.idVenta}-${index}`}>
                  <td className="crop-name">{venta.nombreCliente}</td>
                  <td>{formatearFecha(venta.fecha)}</td>
                  <td>{detalle.nombreProducto}</td>
                  <td><span className="stock-quantity">{detalle.cantidad} kg</span></td>
                </tr>
              )))}
              {lineasVendidas === 0 && (
                <tr>
                  <td className="empty-state" colSpan={4}>
                    <span className="empty-state-icon" aria-hidden="true">✳</span>
                    <strong>No hay ventas para mostrar</strong>
                    <span>Las ventas registradas aparecerán aquí.</span>
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
