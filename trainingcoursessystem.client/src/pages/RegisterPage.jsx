import { useState } from "react";
import { Link } from "react-router-dom";
import { apiPost } from "../api/apiClient";

function RegisterPage() {
    const [form, setForm] = useState({
        fullName: "",
        email: "",
        password: ""
    });

    const [message, setMessage] = useState("");
    const [error, setError] = useState("");

    function handleChange(event) {
        const { name, value } = event.target;

        setForm({
            ...form,
            [name]: value
        });
    }

    async function handleSubmit(event) {
        event.preventDefault();

        setMessage("");
        setError("");

        try {
            const result = await apiPost("/api/auth/register", form);

            setMessage(result.message);
            setForm({
                fullName: "",
                email: "",
                password: ""
            });
        } catch (err) {
            setError(err.message);
        }
    }

    return (
        <main className="auth-page">
            <section className="auth-card">
                <h1>Create Account</h1>
                <p className="muted-text">
                    Create an account. Admin approval is required before login.
                </p>

                {message && <div className="alert alert-success">{message}</div>}
                {error && <div className="alert alert-error">{error}</div>}

                <form onSubmit={handleSubmit} className="form">
                    <div className="form-group">
                        <label>Full Name</label>
                        <input
                            name="fullName"
                            value={form.fullName}
                            onChange={handleChange}
                            placeholder="enter your full name"
                        />
                    </div>

                    <div className="form-group">
                        <label>Email</label>
                        <input
                            name="email"
                            type="email"
                            value={form.email}
                            onChange={handleChange}
                            placeholder="yourEmail@gmail.com"
                        />
                    </div>

                    <div className="form-group">
                        <label>Password</label>
                        <input
                            name="password"
                            type="password"
                            value={form.password}
                            onChange={handleChange}
                            placeholder="At least 6 characters"
                        />
                    </div>

                    <button className="btn btn-primary full-width" type="submit">
                        Create Account
                    </button>
                </form>

                <p className="auth-footer">
                    Already have an account? <Link to="/login">Login</Link>
                </p>
            </section>
        </main>
    );
}

export default RegisterPage;