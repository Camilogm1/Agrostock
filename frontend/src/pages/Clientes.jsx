import { useEffect, useState } from "react";
import axiosClient from "../api/axiosClient";

export default function Clientes() {
  const [clientes, setClientes] = useState([]);
  const [form, setForm] = useState({ nombre: "", numeroIdentificacion: "" });
  const [texto, setTexto] = useState("");
  const [error, setError] = useState("");

  async function cargar() {
    try {
      const { data } = await axiosClient.get("/clientes", { params: { texto } });
      setClientes(data);
      setError("");
    } catch (err) {
      setError(err.message);
    }
  }

  useEffect(() => { cargar(); }, []);

  async function handleSubmit(e) {
    e.preventDefault();
    setError("");
    try {
      await axiosClient.post("/clientes", form);
      setForm({ nombre: "", numeroIdentificacion: "" });
      cargar();
    } catch (err) {
      setError(err.message);
    }
  }

  function handleBuscar(e) {
    e.preventDefault();
    cargar();
  }

  return (
    <main className="page-content">
      <div className="page-heading">
        <div>
          <span className="eyebrow">RELACIONES COMERCIALES</span>
          <h1>Clientes</h1>
          <p>Administra los datos de las personas que compran tus productos.</p>
        </div>
        <div className="heading-date">
          <span className="heading-date-dot" />
          <span>Directorio de clientes</span>
        </div>
      </div>

      <section className="summary-card" aria-label="Resumen de clientes">
        <div className="summary-icon" aria-hidden="true">
          <svg viewBox="0 0 24 24" fill="none">
            <path d="M16 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2m6-10a4 4 0 1 0 0-8 4 4 0 0 0 0 8zm6-7.8a4 4 0 0 1 0 7.6m2 4.2a4 4 0 0 1 2 3.5V21" />
          </svg>
        </div>
        <div className="summary-copy">
          <span>Clientes en el listado</span>
          <strong>{clientes.length}</strong>
        </div>
        <span className="summary-caption">Directorio actualizado</span>
      </section>

      {error && <p className="alert alert-error" role="alert">{error}</p>}

      <section className="panel">
        <div className="panel-heading">
          <div>
            <h2>Agregar cliente</h2>
            <p>Completa sus datos para registrarlo en el directorio.</p>
          </div>
          <span className="panel-heading-mark" aria-hidden="true">+</span>
        </div>
        <form className="entity-form" onSubmit={handleSubmit}>
          <label className="field">
            <span>Nombre completo</span>
            <input
              placeholder="Ej. María Pérez"
              value={form.nombre}
              onChange={(e) => setForm({ ...form, nombre: e.target.value })}
              required
            />
          </label>
          <label className="field">
            <span>Número de identificación</span>
            <input
              placeholder="Ingresa la identificación"
              value={form.numeroIdentificacion}
              onChange={(e) => setForm({ ...form, numeroIdentificacion: e.target.value })}
              required
            />
          </label>
          <div className="form-actions">
            <button className="button button-primary" type="submit">Registrar cliente</button>
          </div>
        </form>
      </section>

      <section className="panel">
        <div className="panel-heading">
          <div>
            <h2>Directorio</h2>
            <p>Encuentra un cliente por nombre o identificación.</p>
          </div>
          <span className="table-count">{clientes.length} {clientes.length === 1 ? "cliente" : "clientes"}</span>
        </div>
        <form className="search-form" onSubmit={handleBuscar}>
          <label className="field search-field" htmlFor="clientes-busqueda">
            <span>Buscar cliente</span>
            <input
              id="clientes-busqueda"
              placeholder="Nombre o número de identificación"
              value={texto}
              onChange={(e) => setTexto(e.target.value)}
            />
          </label>
          <button className="button button-primary" type="submit">Buscar</button>
        </form>
        <div className="table-scroll">
          <table className="data-table">
            <thead>
              <tr><th>Nombre</th><th>Identificación</th></tr>
            </thead>
            <tbody>
              {clientes.map((cliente) => (
                <tr key={cliente.idCliente}>
                  <td className="crop-name">{cliente.nombre}</td>
                  <td>{cliente.numeroIdentificacion}</td>
                </tr>
              ))}
              {clientes.length === 0 && (
                <tr>
                  <td className="empty-state" colSpan={2}>
                    <span className="empty-state-icon" aria-hidden="true">✳</span>
                    <strong>No hay clientes para mostrar</strong>
                    <span>Registra un cliente o prueba con otro término de búsqueda.</span>
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
