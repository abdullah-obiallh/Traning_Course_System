import { Link } from "react-router-dom";

function NotFoundPage() {
    return (
        <main className="center-page">
            <section className="message-card">
                <h1>404</h1>

                <p>
                    The page you are looking for does not exist.
                </p>

                <Link to="/courses" className="btn btn-primary">
                    Back to Home
                </Link>
            </section>
        </main>
    );
}

export default NotFoundPage;