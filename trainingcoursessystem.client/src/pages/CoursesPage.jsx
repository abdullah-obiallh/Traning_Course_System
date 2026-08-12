/* eslint-disable react-hooks/set-state-in-effect */
import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { apiGet } from "../api/apiClient";

function CoursesPage() {
    const [courses, setCourses] = useState([]);
    const [error, setError] = useState("");
    const [loading, setLoading] = useState(true);

  
    async function loadCourses() {
        try {
            const data = await apiGet("/api/courses");
            setCourses(data);
        } catch (err) {
            console.log(err.message);
            setError("Could not load courses.");
        } finally {
            setLoading(false);
        }
    }
      useEffect(() => {
        loadCourses();
    }, []);


    if (loading) {
        return <div className="page-container">Loading courses...</div>;
    }

    return (
        <main className="page-container">
            <section className="page-header">
                <div>
                    <h1>Available Courses</h1>
                    <p className="muted-text">
                        Browse training courses and view their lessons.
                    </p>
                </div>
            </section>

            {error && <div className="alert alert-error">{error}</div>}

            <section className="cards-grid">
                {courses.map((course) => (
                    <article className="course-card" key={course.courseId}>
                        <div className="course-badge">{course.levelName || "General"}</div>

                        <h2>{course.title}</h2>

                        <p className="course-description">
                            {course.description || "No description available."}
                        </p>

                        <div className="course-meta">
                            <span>{course.category || "No category"}</span>
                            <span>{course.durationHours} hours</span>
                            <span>{course.lessonsCount} lessons</span>
                        </div>

                        <p className="instructor-name">
                            Instructor: {course.instructorName}
                        </p>

                        <Link
                            to={`/courses/${course.courseId}`}
                            className="btn btn-primary full-width"
                        >
                            View Details
                        </Link>
                    </article>
                ))}
            </section>
        </main>
    );
}

export default CoursesPage;