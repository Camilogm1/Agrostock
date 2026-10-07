import axios from "axios";

const axiosClient = axios.create({
  baseURL: "http://localhost:5000/api", // ajustar al puerto real del backend
});

axiosClient.interceptors.request.use((config) => {
  const token = localStorage.getItem("agrostock_token");
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

axiosClient.interceptors.response.use(
  (response) => response,
  (error) => {
    const mensaje = error.response?.data?.mensaje || "Ocurrió un error inesperado.";
    return Promise.reject(new Error(mensaje));
  }
);

export default axiosClient;
