import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext";

export default function Login() {
  const [nombreUsuario, setNombreUsuario] = useState("");
  const [contrasena, setContrasena] = useState("");
  const [error, setError] = useState("");
  const { usuario, login, logout } = useAuth();
  const navigate = useNavigate();

  async function handleSubmit(e) {
    e.preventDefault();
    setError("");
    try {
      await login(nombreUsuario, contrasena);
      navigate("/cultivos");
    } catch (err) {
      setError(err.message);
    }
  }

  return (
    <main className="login-page">
      <section className="login-card">
        <div className="login-brand">
          <img src="/agrostock-logo.png" alt="AgroStock — Tu cultivo, en mejores manos" />
        </div>

        <div className="login-heading">
          <span className="eyebrow">QUÉ BUENO TENERTE AQUÍ</span>
          <h1>Inicia sesión</h1>
          <p>Ingresa a tu espacio de trabajo y pongamos el campo en orden.</p>
        </div>

        {usuario && (
          <div className="login-session">
            <span>Sesión activa: <strong>{usuario.nombreUsuario}</strong></span>
            <button className="login-logout-button" type="button" onClick={logout}>
              Cerrar sesión
            </button>
          </div>
        )}

        <form className="login-form" onSubmit={handleSubmit}>
          <label className="field" htmlFor="login-username">
            <span>Usuario</span>
            <input
              id="login-username"
              type="text"
              autoComplete="username"
              placeholder="Tu nombre de usuario"
              value={nombreUsuario}
              onChange={(e) => setNombreUsuario(e.target.value)}
              required
            />
          </label>
          <label className="field" htmlFor="login-password">
            <span>Contraseña</span>
            <input
              id="login-password"
              type="password"
              autoComplete="current-password"
              placeholder="Tu contraseña"
              value={contrasena}
              onChange={(e) => setContrasena(e.target.value)}
              required
            />
          </label>
          {error && <p className="alert alert-error" role="alert">{error}</p>}
          <button className="button button-primary login-submit" type="submit">
            Ingresar
            <span aria-hidden="true">→</span>
          </button>
        </form>

        <p className="login-footer">Cultivamos contigo, temporada tras temporada.</p>
      </section>
    </main>
  );
}
