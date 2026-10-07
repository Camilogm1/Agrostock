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

  return (
    <div style={{ padding: "1rem" }}>
      <h2>Clientes</h2>
      <form onSubmit={handleSubmit} style={{ marginBottom: "1rem" }}>
        <input placeholder="Nombre" value={form.nombre}
          onChange={(e) => setForm({ ...form, nombre: e.target.value })} required />
        <input placeholder="Identificación" value={form.numeroIdentificacion}
          onChange={(e) => setForm({ ...form, numeroIdentificacion: e.target.value })} required />
        <button type="submit">Registrar</button>
      </form>

      <input placeholder="Buscar por nombre o identificación" value={texto}
        onChange={(e) => setTexto(e.target.value)}
        onKeyDown={(e) => e.key === "Enter" && cargar()} />
      <button onClick={cargar}>Buscar</button>

      {error && <p style={{ color: "red" }}>{error}</p>}

      <table border="1" cellPadding="6" style={{ marginTop: "1rem" }}>
        <thead><tr><th>Nombre</th><th>Identificación</th></tr></thead>
        <tbody>
          {clientes.map((c) => (
            <tr key={c.idCliente}><td>{c.nombre}</td><td>{c.numeroIdentificacion}</td></tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
