import { createContext, useContext, useState } from "react";
import axiosClient, { CLAVES_SESION } from "../api/axiosClient";

const AuthContext = createContext(null);

export function AuthProvider({ children }) {
  const [usuario, setUsuario] = useState(() => {
    const token = localStorage.getItem("agrostock_token");
    const nombre = localStorage.getItem("agrostock_usuario");
    const rol = localStorage.getItem("agrostock_rol");
    return token && nombre ? { nombreUsuario: nombre, rol } : null;
  });

  async function login(nombreUsuario, contrasena) {
    const { data } = await axiosClient.post("/auth/login", { nombreUsuario, contrasena });
    localStorage.setItem("agrostock_token", data.token);
    localStorage.setItem("agrostock_usuario", data.nombreUsuario);
    localStorage.setItem("agrostock_rol", data.rol);
    setUsuario({ nombreUsuario: data.nombreUsuario, rol: data.rol });
  }

  function logout() {
    CLAVES_SESION.forEach((clave) => localStorage.removeItem(clave));
    setUsuario(null);
  }

  const esAdmin = usuario?.rol === "Administrador";

  return (
    <AuthContext.Provider value={{ usuario, esAdmin, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  return useContext(AuthContext);
}
