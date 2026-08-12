import { Navigate } from "react-router-dom";
import { getUser } from "../auth/authStorage";

function ProtectedRoute({ allowedRoles, children }) {
    const user = getUser();

    if (!user) {
        return <Navigate to="/login" replace />;
    }

    if (!allowedRoles.includes(user.userRole)) {
        return <Navigate to="/unauthorized" replace />;
    }

    return children;
}

export default ProtectedRoute;