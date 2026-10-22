import { NavLink, useNavigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext";

export default function Navbar() {
  const { usuario, esAdmin, logout } = useAuth();
  const navigate = useNavigate();
  if (!usuario) return null;

  function handleLogout() {
    logout();
    navigate("/login");
  }

  const enlaces = [
    { to: "/cultivos", label: "Cultivos", icon: "M12 3v18m0-18c-4 2-6 5-6 9 0 3 2 5 6 5m0-14c4 2 6 5 6 9 0 3-2 5-6 5" },
    ...(esAdmin ? [{ to: "/cosechas", label: "Cosechas", icon: "M4 19h16M6 16l2-5 3 2 4-7 3 3" }] : []),
    { to: "/inventario", label: "Inventario", icon: "M4 7.5 12 3l8 4.5v9L12 21l-8-4.5zm0 0 8 4.5m0 9v-9m8-4.5-8 4.5" },
    { to: "/clientes", label: "Clientes", icon: "M16 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2m6-10a4 4 0 1 0 0-8 4 4 0 0 0 0 8zm6-7.8a4 4 0 0 1 0 7.6m2 4.2a4 4 0 0 1 2 3.5V21" },
    { to: "/ventas", label: "Ventas", icon: "M4 19V5m0 14h17M8 15l3-4 3 2 5-7m-3 0h3v3" },
  ];

  return (
    <aside className="sidebar">
      <a className="brand" href="/cultivos" aria-label="AgroStock, ir a Cultivos">
        <img className="brand-logo" src="/agrostock-logo.png" alt="AgroStock — Tu cultivo, en mejores manos" />
      </a>

      <div className="nav-section-label">MENÚ PRINCIPAL</div>
      <nav className="primary-nav" aria-label="Navegación principal">
        {enlaces.map(({ to, label, icon }) => (
          <NavLink
            className={({ isActive }) => `nav-link${isActive ? " active" : ""}`}
            key={to}
            to={to}
          >
            <svg viewBox="0 0 24 24" fill="none" aria-hidden="true">
              <path d={icon} />
            </svg>
            <span>{label}</span>
            {to === "/cultivos" && <span className="nav-link-indicator" aria-hidden="true" />}
          </NavLink>
        ))}
      </nav>

      <div className="sidebar-note">
        <span className="sidebar-note-icon" aria-hidden="true">✳</span>
        <strong>Tu campo, en orden</strong>
        <span>Todo lo que necesitas para una buena temporada.</span>
      </div>

      <div className="sidebar-user">
        <span className="avatar" aria-hidden="true">
          {usuario.nombreUsuario?.slice(0, 1).toUpperCase()}
        </span>
        <span className="sidebar-user-copy">
          <strong>{usuario.nombreUsuario}</strong>
          <small>{usuario.rol}</small>
        </span>
        <button className="logout-button" type="button" onClick={handleLogout}>
          Salir
        </button>
      </div>
    </aside>
  );
}
