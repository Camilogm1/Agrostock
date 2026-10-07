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
    } catch (err) {
      setError(err.message);
    }
  }

  useEffect(() => { cargar(); }, []);

  return (
    <div style={{ padding: "1rem" }}>
      <h2>Inventario</h2>
      <input placeholder="Filtrar por cultivo/tipo" value={filtro}
        onChange={(e) => setFiltro(e.target.value)}
        onKeyDown={(e) => e.key === "Enter" && cargar()} />
      <button onClick={cargar}>Buscar</button>

      {error && <p style={{ color: "red" }}>{error}</p>}

      <table border="1" cellPadding="6" style={{ marginTop: "1rem" }}>
        <thead><tr><th>Producto</th><th>Cantidad disponible</th><th>Última actualización</th></tr></thead>
        <tbody>
          {items.map((i) => (
            <tr key={i.idInventario}>
              <td>{i.nombreProducto}</td>
              <td>{i.cantidadDisponible}</td>
              <td>{new Date(i.fechaActualizacion).toLocaleString()}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
