import { Navigate } from "react-router-dom";
import { getToken } from "../services/storageService";

function ProtectedRoute({ children }) {
  const secret = getToken();

  if (!secret) {
    return <Navigate to="/" replace />;
  }

  return children;
}

export default ProtectedRoute;