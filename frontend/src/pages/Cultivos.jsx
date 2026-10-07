import { useEffect, useState } from "react";
import axiosClient from "../api/axiosClient";
import { useAuth } from "../context/AuthContext";

const FORM_VACIO = { nombre: "", tipo: "", lote: "", fechaSiembra: "" };

export default function Cultivos() {
  const { usuario } = useAuth();
  const esAdmin = usuario?.rol === "Administrador";

  const [cultivos, setCultivos] = useState([]);
  const [form, setForm] = useState(FORM_VACIO);
  const [editandoId, setEditandoId] = useState(null); // RF-03 / HU-09
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

  async function handleEliminar(id) {
    try {
      await axiosClient.delete(`/cultivos/${id}`); // RF-04
      cargar();
    } catch (err) {
      setError(err.message); // ej: "tiene cosechas asociadas"
    }
  }

  return (
    <div style={{ padding: "1rem" }}>
      <h2>Cultivos</h2>

      {esAdmin && (
        <form onSubmit={handleSubmit} style={{ marginBottom: "1rem" }}>
          <input placeholder="Nombre" value={form.nombre}
            onChange={(e) => setForm({ ...form, nombre: e.target.value })} required />
          <input placeholder="Tipo" value={form.tipo}
            onChange={(e) => setForm({ ...form, tipo: e.target.value })} required />
          <input placeholder="Lote" value={form.lote}
            onChange={(e) => setForm({ ...form, lote: e.target.value })} required />
          <input type="date" value={form.fechaSiembra}
            onChange={(e) => setForm({ ...form, fechaSiembra: e.target.value })} required />
          <button type="submit">{editandoId ? "Guardar cambios" : "Registrar"}</button>
          {editandoId && <button type="button" onClick={handleCancelarEdicion}>Cancelar</button>}
        </form>
      )}

      {error && <p style={{ color: "red" }}>{error}</p>}

      <table border="1" cellPadding="6">
        <thead>
          <tr><th>Nombre</th><th>Tipo</th><th>Lote</th><th>Fecha siembra</th>{esAdmin && <th>Acciones</th>}</tr>
        </thead>
        <tbody>
          {cultivos.map((c) => (
            <tr key={c.idCultivo}>
              <td>{c.nombre}</td>
              <td>{c.tipo}</td>
              <td>{c.lote}</td>
              <td>{new Date(c.fechaSiembra).toLocaleDateString()}</td>
              {esAdmin && (
                <td>
                  <button onClick={() => handleEditar(c)}>Editar</button>{" "}
                  <button onClick={() => handleEliminar(c.idCultivo)}>Eliminar</button>
                </td>
              )}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
