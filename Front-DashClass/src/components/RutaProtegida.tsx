import { Navigate } from "react-router-dom";
import { useAuth } from "../hooks/useAuth";

interface Props {
  children: React.ReactNode;
}

export default function RutaProtegida({ children }: Props) {
  const { isAuthenticated, isLoading } = useAuth();

  // Esperar a que termine la verificación antes de decidir
  if (isLoading) {
    return (
      <div style={{
        display: "flex",
        alignItems: "center",
        justifyContent: "center",
        height: "100vh",
        background: "var(--bg-base)",
        color: "var(--text-muted)",
        fontSize: "0.85rem",
      }}>
        Verificando sesión...
      </div>
    );
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  return <>{children}</>;
}