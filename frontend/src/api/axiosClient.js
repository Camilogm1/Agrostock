import axios from "axios";

export const CLAVES_SESION = ["agrostock_token", "agrostock_usuario", "agrostock_rol"];

const axiosClient = axios.create({
  baseURL: import.meta.env.VITE_API_URL ?? "http://localhost:5000/api",
});

axiosClient.interceptors.request.use((config) => {
  const token = localStorage.getItem("agrostock_token");
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

axiosClient.interceptors.response.use(
  (response) => response,
  (error) => {
    const status = error.response?.status;
    const esLogin = error.config?.url?.includes("/auth/login");

    // Token vencido o inválido: se cierra la sesión y se vuelve al login
    if (status === 401 && !esLogin) {
      CLAVES_SESION.forEach((clave) => localStorage.removeItem(clave));
      window.location.href = "/login";
    }

    return Promise.reject(new Error(obtenerMensaje(error)));
  }
);

function obtenerMensaje(error) {
  if (!error.response) return "No se pudo conectar con el servidor. Verifique que el backend esté encendido.";

  const { status, data } = error.response;
  if (data?.mensaje) return data.mensaje;
  if (status === 400) return "Hay datos inválidos o incompletos en el formulario.";
  if (status === 403) return "No tiene permisos para realizar esta acción.";
  return "Ocurrió un error inesperado.";
}

export default axiosClient;
