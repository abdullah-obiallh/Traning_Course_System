import { useState } from "react";
import { Link } from "react-router-dom";
import { apiPost } from "../api/apiClient";

function ForgotPasswordPage() {
    const [step, setStep] = useState(1);

    const [form, setForm] = useState({
        email: "",
        code: "",
        newPassword: ""
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

    async function sendResetCode(event) {
        event.preventDefault();

        setMessage("");
        setError("");

        try {
            const result = await apiPost("/api/auth/forgot-password", {
                email: form.email
            });

            setMessage(result.message);
            setStep(2);
        } catch (err) {
            setError(err.message);
        }
    }

    async function verifyCode(event) {
        event.preventDefault();

        setMessage("");
        setError("");

        try {
            const result = await apiPost("/api/auth/verify-reset-code", {
                email: form.email,
                code: form.code
            });

            setMessage(result.message);
            setStep(3);
        } catch (err) {
            setError(err.message);
        }
    }

    async function resetPassword(event) {
        event.preventDefault();

        setMessage("");
        setError("");

        try {
            const result = await apiPost("/api/auth/reset-password", {
                email: form.email,
                code: form.code,
                newPassword: form.newPassword
            });

            setMessage(result.message);
            setForm({
                email: "",
                code: "",
                newPassword: ""
            });
            setStep(1);
        } catch (err) {
            setError(err.message);
        }
    }

    return (
        <main className="auth-page">
            <section className="auth-card">
                <h1>Reset Password</h1>

                <p className="muted-text">
                    Enter your email, verify the code, then set a new password.
                </p>

                {message && <div className="alert alert-success">{message}</div>}
                {error && <div className="alert alert-error">{error}</div>}

                {step === 1 && (
                    <form onSubmit={sendResetCode} className="form">
                        <div className="form-group">
                            <label>Email</label>
                            <input
                                name="email"
                                type="email"
                                value={form.email}
                                onChange={handleChange}
                                placeholder="your@email.com"
                            />
                        </div>

                        <button className="btn btn-primary full-width" type="submit">
                            Send Verification Code
                        </button>
                    </form>
                )}

                {step === 2 && (
                    <form onSubmit={verifyCode} className="form">
                        <div className="form-group">
                            <label>Verification Code</label>
                            <input
                                name="code"
                                value={form.code}
                                onChange={handleChange}
                                placeholder="123456"
                            />
                        </div>

                        <button className="btn btn-primary full-width" type="submit">
                            Verify Code
                        </button>
                    </form>
                )}

                {step === 3 && (
                    <form onSubmit={resetPassword} className="form">
                        <div className="form-group">
                            <label>New Password</label>
                            <input
                                name="newPassword"
                                type="password"
                                value={form.newPassword}
                                onChange={handleChange}
                                placeholder="At least 6 characters"
                            />
                        </div>

                        <button className="btn btn-primary full-width" type="submit">
                            Reset Password
                        </button>
                    </form>
                )}

                <p className="auth-footer">
                    Remember your password? <Link to="/login">Login</Link>
                </p>
            </section>
        </main>
    );
}

export default ForgotPasswordPage;