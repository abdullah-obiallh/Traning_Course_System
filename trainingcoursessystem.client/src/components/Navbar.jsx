import { Link, useNavigate } from "react-router-dom";
import { getUser, logoutUser } from "../auth/authStorage";
import { USER_ROLES } from "../constants/userRoles";

function Navbar() {
    const navigate = useNavigate();
    const user = getUser();

    function handleLogout() {
        logoutUser();
        navigate("/login");
    }

    return (
        <header className="navbar">
            <div className="navbar-brand">
                <Link to="/" className="brand-link">
                    Training Courses Platform
                </Link>
            </div>

            <nav className="navbar-links">
                <Link to="/courses">Courses</Link>

                {user && user.userRole === USER_ROLES.student && (
                    <Link to="/student/my-courses">My Courses</Link>
                )}

                {user && user.userRole === USER_ROLES.instructor && (
                    <Link to="/teacher/courses">Teacher Panel</Link>
                )}

                {user && user.userRole === USER_ROLES.admin && (
                    <>
                        <Link to="/admin/dashboard">Dashboard</Link>
                        <Link to="/admin/courses">Manage Courses</Link>
                        <Link to="/admin/users">Manage Users</Link>
                    </>
                )}

                {user ? (
                    <>
                        <span className="navbar-user">
                            {user.fullName} - {user.userRole}
                        </span>

                        <button className="btn btn-outline" onClick={handleLogout}>
                            Logout
                        </button>
                    </>
                ) : (
                    <>
                        <Link to="/login">Login</Link>
                            <Link style={{ color: "white" }} to="/register"  className="btn btn-primary">
                                Register
                        </Link>
                    </>
                )}
            </nav>
        </header>
    );
}

export default Navbar;