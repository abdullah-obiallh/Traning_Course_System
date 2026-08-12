/* eslint-disable react-hooks/set-state-in-effect */
import { useEffect, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { apiGet, apiPut } from "../api/apiClient";
import { getUser } from "../auth/authStorage";
import { USER_ROLES } from "../constants/userRoles";

function AdminUsersPage() {
    const navigate = useNavigate();
    const user = getUser();

    const [users, setUsers] = useState([]);
    const [message, setMessage] = useState("");
    const [error, setError] = useState("");
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        if (!user) {
            navigate("/login");
            return;
        }

        if (user.userRole !== USER_ROLES.admin) {
            setError("Only admins can view this page.");
            setLoading(false);
            return;
        }

        loadUsers();
    }, []);

    async function loadUsers() {
        try {
            const data = await apiGet("/api/admin/users");
            setUsers(data);
        } catch (err) {
            console.log(err.message)
            setError("Could not load users.");
        } finally {
            setLoading(false);
        }
    }

    async function updateStatus(userId, isActive) {
        setMessage("");
        setError("");

        try {
            const result = await apiPut(`/api/admin/users/${userId}/status`, {
                isActive: isActive
            });

            setMessage(result.message);
            await loadUsers();
        } catch (err) {
            setError(err.message);
        }
    }

    async function updateRole(userId, userRole) {
        setMessage("");
        setError("");

        try {
            const result = await apiPut(`/api/admin/users/${userId}/role`, {
                userRole: userRole
            });

            setMessage(result.message);
            await loadUsers();
        } catch (err) {
            setError(err.message);
        }
    }

    if (loading) {
        return <div className="page-container">Loading users...</div>;
    }

    return (
        <main className="page-container">
            <section className="page-header">
                <div>
                    <h1>Manage Users</h1>
                    <p className="muted-text">
                        Activate accounts and assign users as students or instructors.
                    </p>
                </div>

                <Link to="/admin/dashboard" className="btn btn-outline">
                    Back to Dashboard
                </Link>
            </section>

            {message && <div className="alert alert-success">{message}</div>}
            {error && <div className="alert alert-error">{error}</div>}

            <section className="table-card">
                <table className="data-table">
                    <thead>
                        <tr>
                            <th>User</th>
                            <th>Role</th>
                            <th>Status</th>
                            <th>Created At</th>
                            <th>Change Role</th>
                            <th>Activation</th>
                        </tr>
                    </thead>

                    <tbody>
                        {users.map((item) => (
                            <tr key={item.userId}>
                                <td>
                                    <strong>{item.fullName}</strong>
                                    <span>{item.email}</span>
                                </td>

                                <td>{item.userRole}</td>

                                <td>
                                    <span className={item.isActive ? "status status-active" : "status status-withdrawn"}>
                                        {item.isActive ? "Active" : "Inactive"}
                                    </span>
                                </td>

                                <td>
                                    {new Date(item.createdAt).toLocaleDateString()}
                                </td>

                                <td>
                                    <select
                                        value={item.userRole}
                                        onChange={(event) => updateRole(item.userId, event.target.value)}
                                    >
                                        <option value="Student">Student</option>
                                        <option value="Instructor">Instructor</option>
                                    </select>
                                </td>

                                <td>
                                    {item.isActive ? (
                                        <button
                                            className="btn btn-danger"
                                            onClick={() => updateStatus(item.userId, false)}
                                        >
                                            Deactivate
                                        </button>
                                    ) : (
                                        <button
                                            className="btn btn-primary"
                                            onClick={() => updateStatus(item.userId, true)}
                                        >
                                            Activate
                                        </button>
                                    )}
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            </section>
        </main>
    );
}

export default AdminUsersPage;