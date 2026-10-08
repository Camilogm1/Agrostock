import { Routes, Route, Navigate } from "react-router-dom";
import Navbar from "./components/Navbar";
import PrivateRoute from "./components/PrivateRoute";
import Login from "./pages/Login";
import Cultivos from "./pages/Cultivos";
import Cosechas from "./pages/Cosechas";
import Inventario from "./pages/Inventario";
import Clientes from "./pages/Clientes";
import Ventas from "./pages/Ventas";

export default function App() {
  return (
    <>
      <Navbar />
      <Routes>
        <Route path="/login" element={<Login />} />

        <Route path="/cultivos" element={
          <PrivateRoute><Cultivos /></PrivateRoute>
        } />
        <Route path="/cosechas" element={
          <PrivateRoute rolesPermitidos={["Administrador"]}><Cosechas /></PrivateRoute>
        } />
        <Route path="/inventario" element={
          <PrivateRoute><Inventario /></PrivateRoute>
        } />
        <Route path="/clientes" element={
          <PrivateRoute><Clientes /></PrivateRoute>
        } />
        <Route path="/ventas" element={
          <PrivateRoute><Ventas /></PrivateRoute>
        } />

        <Route path="/" element={<Navigate to="/cultivos" replace />} />
        <Route path="*" element={<Navigate to="/cultivos" replace />} />
      </Routes>
    </>
  );
}
