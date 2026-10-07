import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext";

export default function Navbar() {
  const { usuario, esAdmin, logout } = useAuth();
  const navigate = useNavigate();
  if (!usuario) return null;

  function handleLogout() {
    logout();
    navigate("/login");
  }

  return (
    <nav style={{ display: "flex", gap: "1rem", padding: "1rem", borderBottom: "1px solid #ccc" }}>
      <Link to="/cultivos">Cultivos</Link>
      {esAdmin && <Link to="/cosechas">Cosechas</Link>}
      <Link to="/inventario">Inventario</Link>
      <Link to="/clientes">Clientes</Link>
      <Link to="/ventas">Ventas</Link>
      <span style={{ marginLeft: "auto" }}>
        {usuario.nombreUsuario} ({usuario.rol})
        <button onClick={handleLogout} style={{ marginLeft: "0.5rem" }}>Salir</button>
      </span>
    </nav>
  );
}
