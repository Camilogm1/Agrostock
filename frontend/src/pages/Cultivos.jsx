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
  const [mensaje, setMensaje] = useState("");
  const [verArchivados, setVerArchivados] = useState(false);

  async function cargar() {
    try {
      const { data } = await axiosClient.get("/cultivos", { params: { incluirArchivados: verArchivados } });
      setCultivos(data);
    } catch (err) {
      setError(err.message);
    }
  }

  useEffect(() => { cargar(); }, [verArchivados]);

  const activos = cultivos.filter((c) => c.activo).length;

  async function handleSubmit(e) {
    e.preventDefault();
    setError("");
    setMensaje("");
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
    if (!window.confirm(`¿Eliminar el cultivo "${cultivo.nombre}"? También se borrarán sus cosechas. Sus ventas se conservan en el historial.`)) return;
    setError("");
    setMensaje("");
    try {
      await axiosClient.delete(`/cultivos/${cultivo.idCultivo}`); // RF-04
      setMensaje(`Cultivo "${cultivo.nombre}" eliminado.`);
      cargar();
    } catch (err) {
      setError(err.message);
    }
  }

  // Para cultivos con cosechas o ventas: se ocultan sin borrar su historial
  async function handleArchivar(cultivo) {
    if (!window.confirm(`¿Archivar el cultivo "${cultivo.nombre}"? Dejará de aparecer en las listas, pero se conservan sus cosechas y ventas.`)) return;
    setError("");
    setMensaje("");
    try {
      await axiosClient.post(`/cultivos/${cultivo.idCultivo}/archivar`);
      setMensaje(`Cultivo "${cultivo.nombre}" archivado. Puedes verlo con "Mostrar archivados".`);
      cargar();
    } catch (err) {
      setError(err.message);
    }
  }

  async function handleRestaurar(cultivo) {
    setError("");
    setMensaje("");
    try {
      await axiosClient.post(`/cultivos/${cultivo.idCultivo}/restaurar`);
      setMensaje(`Cultivo "${cultivo.nombre}" restaurado.`);
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
          <strong>{activos}</strong>
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

      {mensaje && <p className="alert alert-success" role="status">{mensaje}</p>}
      {error && <p className="alert alert-error" role="alert">{error}</p>}

      <section className="panel crops-panel">
        <div className="panel-heading crops-heading">
          <div>
            <h2>Todos los cultivos</h2>
            <p>Revisa la información de tus siembras y lotes.</p>
          </div>
          <div className="crops-heading-actions">
            <label className="toggle-archived">
              <input type="checkbox" checked={verArchivados} onChange={(e) => setVerArchivados(e.target.checked)} />
              Mostrar archivados
            </label>
            <span className="table-count">{cultivos.length} {cultivos.length === 1 ? "cultivo" : "cultivos"}</span>
          </div>
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
                <tr key={c.idCultivo} className={c.activo ? undefined : "row-archived"}>
                  <td className="crop-name">
                    {c.nombre}
                    {!c.activo && <span className="archived-badge">Archivado</span>}
                  </td>
                  <td><span className="crop-type">{c.tipo}</span></td>
                  <td>{c.lote}</td>
                  <td>{formatearFecha(c.fechaSiembra)}</td>
                  {esAdmin && (
                    <td>
                      <div className="row-actions">
                        {c.activo ? (
                          <>
                            <button className="text-button" onClick={() => handleEditar(c)}>Editar</button>
                            <button className="text-button" onClick={() => handleArchivar(c)}>Archivar</button>
                            <button className="text-button text-button-danger" onClick={() => handleEliminar(c)}>
                              Eliminar
                            </button>
                          </>
                        ) : (
                          <button className="text-button" onClick={() => handleRestaurar(c)}>Restaurar</button>
                        )}
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
