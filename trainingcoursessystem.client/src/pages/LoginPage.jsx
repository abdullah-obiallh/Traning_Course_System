import { useEffect, useState } from "react";
import { useNavigate, Link } from "react-router-dom";
import { apiPost } from "../api/apiClient";
import { getUser, saveUser } from "../auth/authStorage";
import { getHomePathForRole } from "../utils/userNavigation";

function LoginPage() {
    const navigate = useNavigate();

    const [form, setForm] = useState({
        email: "",
        password: ""
    });

    const [error, setError] = useState("");

    useEffect(() => {
        const currentUser = getUser();

        if (currentUser) {
            goToUserHome(currentUser);
        }
    }, []);

    function goToUserHome(user) {
        navigate(getHomePathForRole(user.userRole));
    }

    function handleChange(event) {
        const { name, value } = event.target;

        setForm({
            ...form,
            [name]: value
        });
    }

    async function handleSubmit(event) {
        event.preventDefault();
        setError("");

        try {
            const user = await apiPost("/api/auth/login", form);

            saveUser(user);
            goToUserHome(user);
        } catch (err) {
            setError(err.message);
        }
    }

    return (
        <main className="auth-page">
            <section className="auth-card">
                <h1>Login</h1>
                <p className="muted-text">
                    Sign in to continue to your training account.
                </p>

                {error && <div className="alert alert-error">{error}</div>}

                <form onSubmit={handleSubmit} className="form">
                    <div className="form-group">
                        <label>Email</label>
                        <input
                            name="email"
                            type="email"
                            value={form.email}
                            onChange={handleChange}
                            placeholder=""
                        />
                    </div>

                    <div className="form-group">
                        <label>Password</label>
                        <input
                            name="password"
                            type="password"
                            value={form.password}
                            onChange={handleChange}
                            placeholder=""
                        />
                    </div>

                    <button className="btn btn-primary full-width" type="submit">
                        Login
                    </button>
                    <p className="auth-footer">
                        <Link to="/forgot-password">Forgot password?</Link>
                    </p>
                </form>

                <p className="auth-footer">
                    Don't have an account? <Link to="/register">Create account</Link>
                </p>
            </section>
        </main>
    );
}

export default LoginPage;