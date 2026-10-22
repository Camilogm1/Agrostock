import { useEffect, useState } from "react";
import axiosClient from "../api/axiosClient";
import { useAuth } from "../context/AuthContext";
import { formatearFecha } from "../utils/fechas";

const FORM_VACIO = { nombre: "", tipo: "", lote: "", fechaSiembra: "" };

export default function Cultivos() {
  const { esAdmin } = useAuth();

  const [cultivos, setCultivos] = useState([]);
  const [form, setForm] = useState(FORM_VACIO);
  const [editandoId, setEditandoId] = useState(null);
  const [error, setError] = useState("");

  async function cargar() {
    try {
      const { data } = await axiosClient.get("/cultivos");
      setCultivos(data);
    } catch (err) {
      setError(err.message);
    }
  }

  useEffect(() => { cargar(); }, []);

  async function handleSubmit(e) {
    e.preventDefault();
    setError("");
    try {
      if (editandoId) {
        await axiosClient.put(`/cultivos/${editandoId}`, form); // RF-03
      } else {
        await axiosClient.post("/cultivos", form); // RF-01
      }
      setForm(FORM_VACIO);
      setEditandoId(null);
      cargar();
    } catch (err) {
      setError(err.message);
    }
  }

  function handleEditar(cultivo) {
    setEditandoId(cultivo.idCultivo);
    setForm({
      nombre: cultivo.nombre,
      tipo: cultivo.tipo,
      lote: cultivo.lote,
      fechaSiembra: cultivo.fechaSiembra?.substring(0, 10) ?? "",
    });
  }

  function handleCancelarEdicion() {
    setEditandoId(null);
    setForm(FORM_VACIO);
  }

  async function handleEliminar(cultivo) {
    if (!window.confirm(`¿Eliminar el cultivo "${cultivo.nombre}"?`)) return;
    setError("");
    try {
      await axiosClient.delete(`/cultivos/${cultivo.idCultivo}`); // RF-04
      cargar();
    } catch (err) {
      setError(err.message);
    }
  }

  return (
    <main className="page-content">
      <div className="page-heading">
        <div>
          <span className="eyebrow">GESTIÓN DEL CAMPO</span>
          <h1>Mis cultivos</h1>
          <p>Organiza tus cultivos y mantén cada lote bajo control.</p>
        </div>
        <div className="heading-date">
          <span className="heading-date-dot" />
          <span>Temporada en curso</span>
        </div>
      </div>

      <section className="summary-card" aria-label="Resumen de cultivos">
        <div className="summary-icon" aria-hidden="true">
          <svg viewBox="0 0 24 24" fill="none">
            <path d="M19.5 4.5C12 4.5 6 7.5 6 13a5 5 0 0 0 5 5c5.5 0 8.5-6 8.5-13.5Z" />
            <path d="M4 21c2.5-5.5 6.5-8.5 12-11" />
          </svg>
        </div>
        <div className="summary-copy">
          <span>Cultivos registrados</span>
          <strong>{cultivos.length}</strong>
        </div>
        <span className="summary-caption">En todos tus lotes</span>
      </section>

      {esAdmin && (
        <section className="panel form-panel">
          <div className="panel-heading">
            <div>
              <h2>{editandoId ? "Editar cultivo" : "Agregar cultivo"}</h2>
              <p>{editandoId ? "Actualiza los datos de este cultivo." : "Registra un nuevo cultivo en tu campo."}</p>
            </div>
            <span className="panel-heading-mark" aria-hidden="true">+</span>
          </div>
          <form className="crop-form" onSubmit={handleSubmit}>
            <label className="field">
              <span>Nombre del cultivo</span>
              <input placeholder="Ej. Maíz dulce" value={form.nombre}
                onChange={(e) => setForm({ ...form, nombre: e.target.value })} required />
            </label>
            <label className="field">
              <span>Tipo o variedad</span>
              <input placeholder="Ej. Hortaliza" value={form.tipo}
                onChange={(e) => setForm({ ...form, tipo: e.target.value })} required />
            </label>
            <label className="field">
              <span>Lote</span>
              <input placeholder="Ej. Lote A-01" value={form.lote}
                onChange={(e) => setForm({ ...form, lote: e.target.value })} required />
            </label>
            <label className="field">
              <span>Fecha de siembra</span>
              <input type="date" value={form.fechaSiembra}
                onChange={(e) => setForm({ ...form, fechaSiembra: e.target.value })} required />
            </label>
            <div className="form-actions">
              {editandoId && (
                <button className="button button-secondary" type="button" onClick={handleCancelarEdicion}>
                  Cancelar
                </button>
              )}
              <button className="button button-primary" type="submit">
                {editandoId ? "Guardar cambios" : "Registrar cultivo"}
              </button>
            </div>
          </form>
        </section>
      )}

      {error && <p className="alert alert-error" role="alert">{error}</p>}

      <section className="panel crops-panel">
        <div className="panel-heading crops-heading">
          <div>
            <h2>Todos los cultivos</h2>
            <p>Revisa la información de tus siembras y lotes.</p>
          </div>
          <span className="table-count">{cultivos.length} {cultivos.length === 1 ? "cultivo" : "cultivos"}</span>
        </div>
        <div className="table-scroll">
          <table className="data-table">
            <thead>
              <tr>
                <th>Nombre</th>
                <th>Tipo</th>
                <th>Lote</th>
                <th>Fecha de siembra</th>
                {esAdmin && <th>Acciones</th>}
              </tr>
            </thead>
            <tbody>
              {cultivos.map((c) => (
                <tr key={c.idCultivo}>
                  <td className="crop-name">{c.nombre}</td>
                  <td><span className="crop-type">{c.tipo}</span></td>
                  <td>{c.lote}</td>
                  <td>{formatearFecha(c.fechaSiembra)}</td>
                  {esAdmin && (
                    <td>
                      <div className="row-actions">
                        <button className="text-button" onClick={() => handleEditar(c)}>Editar</button>
                        <button className="text-button text-button-danger" onClick={() => handleEliminar(c)}>
                          Eliminar
                        </button>
                      </div>
                    </td>
                  )}
                </tr>
              ))}
              {cultivos.length === 0 && (
                <tr>
                  <td className="empty-state" colSpan={esAdmin ? 5 : 4}>
                    <span className="empty-state-icon" aria-hidden="true">✳</span>
                    <strong>Aún no hay cultivos registrados</strong>
                    <span>Cuando agregues un cultivo, aparecerá aquí.</span>
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
