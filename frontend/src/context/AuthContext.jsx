import { createContext, useContext, useState } from "react";
import axiosClient from "../api/axiosClient";

const AuthContext = createContext(null);

export function AuthProvider({ children }) {
  const [usuario, setUsuario] = useState(() => {
    const nombre = localStorage.getItem("agrostock_usuario");
    const rol = localStorage.getItem("agrostock_rol");
    return nombre ? { nombreUsuario: nombre, rol } : null;
  });

  async function login(nombreUsuario, contrasena) {
    const { data } = await axiosClient.post("/auth/login", { nombreUsuario, contrasena });
    localStorage.setItem("agrostock_token", data.token);
    localStorage.setItem("agrostock_usuario", data.nombreUsuario);
    localStorage.setItem("agrostock_rol", data.rol);
    setUsuario({ nombreUsuario: data.nombreUsuario, rol: data.rol });
  }

  function logout() {
    localStorage.removeItem("agrostock_token");
    localStorage.removeItem("agrostock_usuario");
    localStorage.removeItem("agrostock_rol");
    setUsuario(null);
  }

  return (
    <AuthContext.Provider value={{ usuario, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  return useContext(AuthContext);
}
