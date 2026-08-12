/* eslint-disable react-hooks/immutability */
import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { apiGet, apiPost } from "../api/apiClient";
import { getUser } from "../auth/authStorage";
import { USER_ROLES } from "../constants/userRoles";

function CourseDetailsPage() {
    const { id } = useParams();

    const [course, setCourse] = useState(null);
    const [message, setMessage] = useState("");
    const [error, setError] = useState("");
    const [loading, setLoading] = useState(true);

    const user = getUser();

    useEffect(() => {
        loadCourse();
    }, [id]);

    async function loadCourse() {
        try {
            const data = await apiGet(`/api/courses/${id}`);
            setCourse(data);
        } catch (err) {
            console.log(err.message)
            setError("Could not load course details.");
        } finally {
            setLoading(false);
        }
    }

    async function handleEnroll() {
        setMessage("");
        setError("");

        if (!user) {
            setError("Please login first.");
            return;
        }

        if (user.userRole !== USER_ROLES.student) {
            setError("Only students can enroll in courses.");
            return;
        }

        try {
            await apiPost("/api/enrollments", {
                studentId: user.userId,
                courseId: Number(id)
            });

            setMessage("You have enrolled successfully.");
        } catch (err) {
            setError(err.message);
        }
    }

    if (loading) {
        return <div className="page-container">Loading course...</div>;
    }

    if (!course) {
        return <div className="page-container">Course not found.</div>;
    }

    return (
        <main className="page-container">
            <section className="details-card">
                <div className="details-header">
                    <div>
                        <span className="course-badge">{course.levelName || "General"}</span>
                        <h1>{course.title}</h1>
                        <p className="muted-text">{course.description}</p>
                    </div>

                    <button className="btn btn-primary" onClick={handleEnroll}>
                        Enroll Now
                    </button>
                </div>

                {message && <div className="alert alert-success">{message}</div>}
                {error && <div className="alert alert-error">{error}</div>}

                <div className="info-row">
                    <div>
                        <strong>Instructor</strong>
                        <span>{course.instructorName}</span>
                    </div>

                    <div>
                        <strong>Category</strong>
                        <span>{course.category || "Not specified"}</span>
                    </div>

                    <div>
                        <strong>Duration</strong>
                        <span>{course.durationHours} hours</span>
                    </div>
                </div>
            </section>

            <section className="lessons-section">
                <h2>Course Lessons</h2>

                <div className="lessons-list">
                    {course.lessons.map((lesson) => (
                        <article className="lesson-item" key={lesson.lessonId}>
                            <div className="lesson-number">{lesson.lessonOrder}</div>

                            <div>
                                <h3>{lesson.title}</h3>
                                <p>{lesson.content || "No content available."}</p>
                            </div>
                        </article>
                    ))}
                </div>
            </section>
        </main>
    );
}

export default CourseDetailsPage;