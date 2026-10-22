import { useEffect, useState } from "react";
import axiosClient from "../api/axiosClient";

export default function Inventario() {
  const [items, setItems] = useState([]);
  const [filtro, setFiltro] = useState("");
  const [error, setError] = useState("");

  async function cargar() {
    try {
      const { data } = await axiosClient.get("/inventario", { params: { filtro } });
      setItems(data);
      setError("");
    } catch (err) {
      setError(err.message);
    }
  }

  useEffect(() => { cargar(); }, []);

  const cantidadTotal = items.reduce((total, item) => total + Number(item.cantidadDisponible || 0), 0);

  function handleBuscar(e) {
    e.preventDefault();
    cargar();
  }

  return (
    <main className="page-content">
      <div className="page-heading">
        <div>
          <span className="eyebrow">CONTROL DE EXISTENCIAS</span>
          <h1>Inventario</h1>
          <p>Consulta los productos disponibles y sus últimas actualizaciones.</p>
        </div>
        <div className="heading-date">
          <span className="heading-date-dot" />
          <span>Existencias al día</span>
        </div>
      </div>

      <section className="summary-grid" aria-label="Resumen del inventario">
        <div className="summary-card">
          <div className="summary-icon" aria-hidden="true">
            <svg viewBox="0 0 24 24" fill="none">
              <path d="M4 7.5 12 3l8 4.5v9L12 21l-8-4.5zm0 0 8 4.5m0 9v-9m8-4.5-8 4.5" />
            </svg>
          </div>
          <div className="summary-copy">
            <span>Productos registrados</span>
            <strong>{items.length}</strong>
          </div>
        </div>
        <div className="summary-card">
          <div className="summary-icon summary-icon-earth" aria-hidden="true">#</div>
          <div className="summary-copy">
            <span>Unidades disponibles</span>
            <strong>{cantidadTotal.toLocaleString()}</strong>
          </div>
        </div>
      </section>

      {error && <p className="alert alert-error" role="alert">{error}</p>}

      <section className="panel">
        <div className="panel-heading">
          <div>
            <h2>Existencias</h2>
            <p>Busca productos por cultivo o tipo.</p>
          </div>
          <span className="panel-heading-mark" aria-hidden="true">⌕</span>
        </div>
        <form className="search-form" onSubmit={handleBuscar}>
          <label className="field search-field" htmlFor="inventario-filtro">
            <span>Buscar en inventario</span>
            <input
              id="inventario-filtro"
              placeholder="Filtrar por cultivo o tipo"
              value={filtro}
              onChange={(e) => setFiltro(e.target.value)}
            />
          </label>
          <button className="button button-primary" type="submit">Buscar</button>
        </form>
        <div className="table-scroll">
          <table className="data-table">
            <thead>
              <tr>
                <th>Producto</th>
                <th>Cantidad disponible</th>
                <th>Última actualización</th>
              </tr>
            </thead>
            <tbody>
              {items.map((item) => (
                <tr key={item.idInventario}>
                  <td className="crop-name">{item.nombreProducto}</td>
                  <td><span className="stock-quantity">{item.cantidadDisponible}</span></td>
                  <td>{new Date(item.fechaActualizacion).toLocaleString()}</td>
                </tr>
              ))}
              {items.length === 0 && (
                <tr>
                  <td className="empty-state" colSpan={3}>
                    <span className="empty-state-icon" aria-hidden="true">✳</span>
                    <strong>No hay productos para mostrar</strong>
                    <span>Prueba con otro filtro o revisa el inventario más tarde.</span>
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
