import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import Login from "./pages/Login";
import Register from "./pages/Register";
import Dashboard from "./pages/Dashboard";
import Cursos from "./pages/Cursos";
import CursoDashboard from "./pages/Cursodashboard";
import GrupoDashboard from "./pages/GrupoDashboard";
import ActividadDetalle from "./pages/ActividadDetalle";
import RutaProtegida from "./components/RutaProtegida";

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Navigate to="/login" />} />
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />

        <Route path="/dashboard" element={<RutaProtegida><Dashboard /></RutaProtegida>} />
        <Route path="/cursos" element={<RutaProtegida><Cursos /></RutaProtegida>} />
        <Route path="/cursos/:idCurso" element={<RutaProtegida><CursoDashboard /></RutaProtegida>} />
        <Route path="/cursos/:idCurso/grupos/:idGrupo" element={<RutaProtegida><GrupoDashboard /></RutaProtegida>} />
        <Route path="/cursos/:idCurso/grupos/:idGrupo/actividades/:idActividad" element={<RutaProtegida><ActividadDetalle /></RutaProtegida>} />
      </Routes>
    </BrowserRouter>
  );
}

export default App;