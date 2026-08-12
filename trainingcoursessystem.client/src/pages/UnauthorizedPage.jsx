import { Link } from "react-router-dom";

function UnauthorizedPage() {
    return (
        <main className="center-page">
            <section className="message-card">
                <h1>Access Denied</h1>

                <p>
                    You do not have permission to view this page.
                </p>

                <Link to="/courses" className="btn btn-primary">
                    Go to Courses
                </Link>
            </section>
        </main>
    );
}

export default UnauthorizedPage;